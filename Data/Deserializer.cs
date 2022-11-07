using System.IO;
using System.Text;

namespace Communesoft.Editor.Stellaris.Data
{
	public static class Deserializer
	{
		private static readonly UTF8Encoding UTF8 = new(false);
		private static readonly UTF8Encoding UTF8_BOM = new(true);

		/// <summary>
		/// Deserializes the root directory at the path into processable content
		/// </summary>
		internal static ExpressionDirectory DeserializeSample()
		{
			ExpressionFileStructure filesStructure = null; // TODO: Create metadata for sample
			
			return DeserializeFiles(new ExpressionDirectory(null, new DirectoryInfo("Test"), filesStructure));
		}
		/// <summary>
		/// Deserializes the root directory at the path into processable content
		/// </summary>
		/// <param name="path">The path of the root directory</param>
		public static ExpressionDirectory Deserialize(string path)
		{
			string[] loading = Metadata.Directories.Keys.Select(p => p.Split('/', 2)[0]).Distinct().ToArray();
			
			ExpressionDirectory root = new(null, new DirectoryInfo(path), null);
			foreach (DirectoryInfo dir in root.Directory.GetDirectories())
			{
				if (dir.Name.NotIn(loading))
				{
					continue;
				}
				
				root.Directories.Add(Deserialize(root, dir, dir.Name, Metadata.Directories.Keys));
			}
			return root;
		}

		private static ExpressionDirectory Deserialize(ExpressionDirectory parent, DirectoryInfo directory, string path, IEnumerable<string> metaPaths)
		{
			ExpressionFileStructure filesStructure = Metadata.Files.TryGetValue(path);
			ExpressionDirectory filesDirectory = new(parent, directory, filesStructure);

			// Get flag that the current directory can be processed (if count is not 0)
			string[] paths = metaPaths.Where(p => p.StartsWith(directory.Name)).ToArray();
			// Get possible subdirectories of the current directory from metadata
			string[] subPaths = paths
				.Select(p => p.Split('/', 2)[^1])
				.Where(p => p != directory.Name)
				.ToArray();
			
			// Load subdirectories
			if (subPaths.IsNotNullOrEmpty())
			{
				foreach (DirectoryInfo subdir in directory.GetDirectories())
				{
					if (subdir.Name.NotIn(subPaths))
					{
						continue;
					}
					
					filesDirectory.Directories.Add(Deserialize(filesDirectory, subdir, $"{path}/{subdir.Name}", subPaths));
				}
			}
			// Load files
			if (paths.IsNotNullOrEmpty())
			{
				DeserializeFiles(filesDirectory);
			}
			return filesDirectory;
		}

		/// <summary>
		/// Deserializable file extensions
		/// </summary>
		private static readonly string[] deserializableFiles = { ".txt", ".asset", ".gui", ".gfx" };
		/// <summary>
		/// Deserializes files in the parent directory
		/// </summary>
		/// <returns>The parent directory</returns>
		private static ExpressionDirectory DeserializeFiles(ExpressionDirectory parent)
		{
			foreach (FileInfo fileInfo in parent.Directory.GetFiles())
			{
				// Deserialize only specific files
				if (fileInfo.Extension.NotIn(deserializableFiles))
				{
					continue;
				}
				
				(IList<IComplexContent> fileContent, Exception fileError) = (null, null);
				try
				{
					fileContent = Deserialize(fileInfo, parent.FileStructure);
				}
				catch (Exception e)
				{
					fileError = e;
				}
				
				// TODO: если в файле только комментарий, то тоже надо как-то обрабатывать!
				parent.Files.Add(new ExpressionFile(parent, fileInfo, fileContent, fileError));
			}
			return parent;
		}
		
		/// <summary>
		/// Deserializes the given file with metadata into processable content
		/// </summary>
		/// <param name="file">The file</param>
		/// <param name="fileStructure">The file metadata</param>
		/// <param name="withBOM">UTF8 with BOM</param>
		public static IList<IComplexContent> Deserialize(FileInfo file, ExpressionFileStructure fileStructure, bool withBOM = false)
		{
			IList<IToken> tokens = CreateTokens(file, withBOM);
			IList<IToken> statements = CreateStatements(tokens);
			IList<IComplexContent> objects = CreateObjects(statements, fileStructure?.Content);

			return objects;
		}

		/// <summary>
		/// Performs tokenizing on <paramref name="file"/>
		/// </summary>
		/// <param name="file">File to tokenize</param>
		private static IList<IToken> CreateTokens(FileInfo file, bool withBOM)
		{
			List<char> buffer = new(64);
			List<IToken> tokens = new();
			//int commentsCount = 0;

			using FileStream fileReader = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
			using StreamReader reader = new(fileReader, withBOM ? UTF8_BOM : UTF8);

			char current;
			bool newline = true, eof = false;
			(long row, long column, long position) = (1L, 0L, 0L);
			do
			{
				// Read first char
				switch (ReadNext())
				{
					// Whitespace
					case char c when !IsNewLine() && char.IsWhiteSpace(c):
					{
						continue;
					}
					
					// Comment
					case '#':
					{
						// Several comments in sequence count as one
						//if (tokens.Count == 0 || tokens[^1] is not CommentToken c || c.IsAfterwards)
						//{
						//	commentsCount++;
						//}
						
						ReadComment();
						break;
					}

					// Relation
					case '<':
					case '>':
					{
						ReadGreaterLessRelation();
						break;
					}

					// Relation
					case '!':
					case '=':
					{
						ReadEqualRelation();
						break;
					}

					// Complex
					case '{':
					case '}':
					{
						buffer.Add(current);

						AddToken(new ComplexToken(buffer, CreateTokenInfo()));
						break;
					}

					// String
					case char c when char.IsLetterOrDigit(c) || c.In('-', '+', '@', '$', '_', '"'):
					{
						ReadString();
						break;
					}
				}

				if (IsNewLine())
				{
					if (PeekNext() == '\n')
					{
						ReadNext();
					}
					
					row++;
					column = 0L;
					newline = true;
				}

			} while (!eof);

			return tokens;

			#region Local Funcs

			char ReadNext()
			{
				column++;
				position++;
				
				int read = reader.Read();
				eof = read == -1;
				
				return current = (char)read;
			}
			
			char PeekNext()
			{
				int peek = reader.Peek();
				eof = peek == -1;
				
				return current = (char)peek;
			}
			
			bool IsNewLine() => current is '\r' or '\n';
			TokenInfo CreateTokenInfo() => new TokenInfo(file.FullName, (row, column, position));

			void AddToken(IToken token)
			{
				tokens.Add(token);
				// If token was added, then line has something
				newline = false;
			}

			void ReadComment()
			{
				TokenInfo info = CreateTokenInfo();
				do
				{
					ReadNext();
					if (IsNewLine() || eof)
					{
						// Exit when newline or EOF
						break;
					}

					if (current == '#')
					{
						// Ignore other '#'
						continue;
					}

					// Insert char
					buffer.Add(current);

				} while (true);

				AddToken(new CommentToken(buffer, info, !newline));
			}

			void ReadGreaterLessRelation()
			{
				TokenInfo info = CreateTokenInfo();
				
				// '>' or '<'
				buffer.Add(current);
				
				if (PeekNext() == '=')
				{
					// '>=' or '<='
					ReadNext();
					buffer.Add(current);
				}

				AddToken(new RelationToken(buffer, info));
			}

			void ReadEqualRelation()
			{
				TokenInfo info = CreateTokenInfo();
				
				// '!' or '='
				buffer.Add(current);

				if (current == '!')
				{
					if (ReadNext() != '=')
					{
						throw new MetadataParseException($"Cannot create token '!='. {CreateTokenInfo().ToString()}");
					}

					// '!='
					buffer.Add(current);
				}

				AddToken(new RelationToken(buffer, info));
			}

			void ReadString()
			{
				TokenInfo info = CreateTokenInfo();
				
				// Let whitespace in string
				bool whitespaces = current == '"';
				do
				{
					// Insert char
					if (!(buffer.Count == 0 && current == '"'))
					{
						buffer.Add(current);
					}

					PeekNext();
					if (current.In('=', '<', '>', '!', '{', '}', '#') || eof)
					{
						// Forbidden to read this chars, because it's other tokens' chars
						break;
					}

					ReadNext();
					if (whitespaces && current == '"')
					{
						break;
					}
					if (!whitespaces && char.IsWhiteSpace(current))
					{
						break;
					}

				} while (true);

				AddToken(new StringToken(buffer, info));
			}

			#endregion // Local Funcs
		}
		/// <summary>
		/// Performs creating statements from separated tokens
		/// </summary>
		/// <param name="tokens">Tokens collection</param>
		private static IList<IToken> CreateStatements(IList<IToken> tokens)
		{
			// Comments concatenation for bind up to statements
			CommentToken currentCommentBasis = null;
			
			// Stack for the possible expression links
			List<RelationToken> expressionLinks = new(64) { null };
			
			// Stack for switching statements context
			int currentContext = 0;
			List<IList<IToken>> context = new(64 /* How many times possible to nest complex statements without rearrange list */)
			{
				new List<IToken>(64) // Root context statements
			};
			
			// Unopened '}' tokens
			List<IToken> errorComplexEnd = new();

			IToken previousToken = null; // Except 'CommentToken'
			foreach (IToken token in tokens)
			{
				switch (token)
				{
					case CommentToken c:
					{
						// First met comment become a basis if current is null
						currentCommentBasis ??= c;
						
						if (c.IsAfterwards)
						{
							AddComment();
						}
						else
						{
							currentCommentBasis.Add(c);
						}
						
						break;
					}

					case RelationToken r:
					{
						switch (previousToken)
						{
							case null:
								throw new MetadataParseException($"Relation statement ({token}) is the first in the file. {token.Info.ToString()}");

							case RelationToken:
								throw new MetadataParseException($"Two relation statements ({previousToken} {token}) in sequence. {previousToken.Info.ToString()}");
						}

						// Unlink the previous token from the previous relation to link with the current
						expressionLinks[currentContext]?.Relations.Remove(previousToken);
						
						// Save link to containing expression
						expressionLinks[currentContext] = r;

						AddToContext();
						break;
					}
					
					case StringToken s:
					{
						AddToContext();
						break;
					}

					case ComplexToken c:
					{
						if (c.IsStart)
						{
							// Add complex statement and change context
							AddToContext();
							
							// Clear previous link
							previousToken = null;

							// Changing context to inner
							if (++currentContext == context.Count)
							{
								// Add new
								context.Add(c.Value);
								expressionLinks.Add(null);
							}
							else
							{
								// Change existing
								context[currentContext] = c.Value;
								expressionLinks[currentContext] = null;
							}
						}
						else if (currentContext == 0)
						{
							errorComplexEnd.Add(token);
						}
						else
						{
							// Add a comment
							AddComment();
							
							// Restore context to outer
							currentContext--;
							
							// Restore link to previous
							previousToken = context[currentContext][^1];
						}
						
						break;
					}
				}
				
				#region Local Funcs

				// Not invoked on CommentToken and '}' token
				void AddToContext()
				{
					AddComment();
					
					context[currentContext].Add(token);

					// Link current or previous token with the relation
					expressionLinks[currentContext]?.Relations.Add(token is RelationToken ? previousToken : token);
					
					// Move to next token
					previousToken = token;
				}

				void AddComment()
				{
					if (currentCommentBasis != null)
					{
						IToken bind = token;
						if (currentCommentBasis.IsAfterwards)
						{
							// If the first in the current context, then the comment belongs to the upper context token
							int popCount = (context[currentContext].Count == 0 ? 1 : 0);

							// Bind afterwards comment to the last token
							bind = context[currentContext - popCount][^1];
						}
						else if (token is ComplexToken { IsStart: false })
						{
							// The comments near the '}' token. Bind to the '{'
							bind = context[currentContext - 1][^1];
						}
						
						bind.AddComment(currentCommentBasis);
					}
				
					// Move to next comment basis
					currentCommentBasis = null;
				}

				#endregion // Local Funcs
			}

			// Unclosed '{' tokens
			List<IToken> errorComplexStart = new(currentContext);
			for (int i = 0; i < currentContext; i++)
			{
				errorComplexStart.Add(context[i][^1]);
			}

			string errorComplex = string.Join(
				Environment.NewLine,
				CreateError("The complex statements' opening bracket was not closed", errorComplexStart),
				CreateError("The complex statements' closing bracket was not opened", errorComplexEnd)
			);
			if (errorComplex.IsNotNullOrWhiteSpace())
			{
				throw new MetadataParseException(errorComplex.Trim());
			}
			
			// Return root context statements and comments
			return context[0];
			
			// Local Funcs

			string CreateError(string preamble, IList<IToken> errors) => errors.Count == 0 ? null : $"{preamble}. {string.Join("; ", errors.Select(t => t.Info.StringLocation))}";
		}
		/// <summary>
		/// Performs creating values and expressions from statements
		/// </summary>
		/// <param name="statements">Statements collection</param>
		/// <param name="metadata">File metadata</param>
		private static IList<IComplexContent> CreateObjects(IList<IToken> statements, IList<IMetadataContent> metadata)
		{
			List<IExpressionMetadata> exprs = metadata?.OfType<IExpressionMetadata>().ToList();
			List<IExpressionValueMetadata> vals = metadata?.OfType<IExpressionValueMetadata>().ToList();
			
			List<IComplexContent> content = new(statements.Count);
			for (int i = 0; i < statements.Count; i++)
			{
				IComplexContent cnt;
				// Peek to relation statement
				if (i + 1 < statements.Count && statements[i + 1] is RelationToken rel)
				{
					cnt = ReadExpression(rel, exprs);
				}
				else
				{
					// TODO: доработки, тут будут всякие проверки по коллекции vals скорее всего
					cnt = ReadValue(statements[i], vals?.FirstOrDefault());
				}

				content.Add(cnt);
				
				#region Local Funcs

				IExpression ReadExpression(RelationToken relation, IList<IExpressionMetadata> metadata)
				{
					// Do read this token
					i++;

					IList<IToken> rels = relation.Relations;
					StringToken entity = (StringToken)rels[0];
					
					// Try read comments
					CommentToken comment = entity.Comment + relation.Comment;
					
					// Try get the expression metadata
					IExpressionMetadata expr = null;
					IExpressionValueMetadata exprValue = null;
					if (metadata != null)
					{
						// Find by strict name
						expr = metadata.FirstOrDefault(m => m.Entity.Name == entity);
						if (expr == null)
						{
							// Find by type
							expr = metadata.FirstOrDefault(m => m.Entity.Name == null);
						}
						exprValue = expr?.Value;
					}
					
					if (rels.Count == 2)
					{
						// Standard expression
						
						// Do read next token
						i++;

						return ReadValue(rels[1], exprValue, comment) switch
						{
							SimpleExpressionValue simple   => new SimpleExpression(expr as SimpleExpressionMetadata, entity, relation, simple),
							ListExpressionValue list       => new ListExpression(expr as ListExpressionMetadata, entity, relation, list),
							ComplexExpressionValue complex => new ComplexExpression(expr as ComplexExpressionMetadata, entity, relation, complex),

							{ } value => throw new NotImplementedException($"{value.GetType().Name} value is not implemented")
						};
					}
					else
					{
						// Custom expression
						
						// The first token was read as entity
						List<ISimpleContent> content = new(rels.Count - 1);
						for (int r = 1; r < rels.Count; r++, i++)
						{
							content.Add(ReadValue(rels[r], exprValue));
							// TODO: может быть разное, что value будет отдельной метадатой регулироваться (когда не принадлежит к relation реально)
						}
						
						SimpleExpressionValue value = new(comment, null, content);
						
						return new SimpleExpression(expr as SimpleExpressionMetadata, entity, relation, value);
					}
				}
				
				IExpressionValue ReadValue(IToken value, IExpressionValueMetadata metadata, CommentToken comment = null)
				{
					// Try read comment
					comment += value.Comment;
					
					// Read value
					switch (value)
					{
						case StringToken simple:
						{
							return new SimpleExpressionValue(comment, metadata as SimpleExpressionValueMetadata, new RawExpressionValue(simple));
						}
						case ComplexToken complex:
						{
							// Try get containing metadata
							IList<IMetadataContent> metaContent = null;
							if (metadata != null)
							{
								// Try go further or get reference from type, if have no further content
								metaContent = (metadata as IMetadataStructure)?.Content;
								if (metaContent == null)
								{
									metaContent = Metadata.Types.TryGetValue(metadata.Type.Name)?.Content;
								}
							}
							
							IList<IComplexContent> content = CreateObjects(complex.Value, metaContent);

							// Choice between complex and list
							if (content?.All(c => c is IListContent) == true)
							{
								return new ListExpressionValue(comment, metadata as ListExpressionValueMetadata, content.OfType<IListContent>().ToList());
							}
							return new ComplexExpressionValue(comment, metadata as ComplexExpressionValueMetadata, content);
						}
						
						default:
							throw new NotImplementedException($"{value.GetType().Name} token is not implemented as a value");
					}
				}

				#endregion // Local Funcs
			}

			return content;
		}
	}
}