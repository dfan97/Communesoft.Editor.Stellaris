using System.IO;
using System.Text;
using System.Xml.Linq;
using Communesoft.Common.Utils.Convert;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	internal static class Metadata
	{
		private static readonly string pathXsd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Metadata", "Xsd");
		private static readonly string pathXml = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Metadata", "Xml");

		/// <summary>
		/// Contains all expression files structure
		/// </summary>
		internal static SortedDictionary<string, ExpressionFileStructure> Files { get; private set; }
		/// <summary>
		/// Contains all types structure
		/// </summary>
		internal static SortedDictionary<string, ExpressionType> Types { get; private set; }

		/// <summary>
		/// Contains all types reference
		/// </summary>
		internal static SortedDictionary<string, ExpressionTypeReference> TypeReferences { get; private set; }
		/// <summary>
		/// Contains all expression directories path with annotation
		/// </summary>
		internal static SortedDictionary<string, string> Directories { get; private set; }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="baseTypePath">The containing type path</param>
		/// <exception cref="MetadataParseException">Parsing errors</exception>
		internal static (string entity, RelationTypes relation) GetEntityRelationStatements(XElement xml, string baseTypePath)
		{
			string entity = xml.GetAttributeValue(XmlConstants.Entity);
			if (entity.IsNullOrWhiteSpace())
			{
				throw new MetadataParseException($"Doesn't contain '{XmlConstants.Entity}' attribute or it's value is empty");
			}
			// The entity contains a reference
			if (entity.IndexOfAny(new[] { '.', ':' }) >= 0)
			{
				(_, entity) = ParseReferenceStatement(entity, baseTypePath);
			}
			
			RelationTypes relation = RelationUtilities.ConvertToRelationType(xml.GetAttributeValue(XmlConstants.Relation));
			
			return (entity, relation);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="typePath">The current type path value - absolute or relative</param>
		/// <param name="baseTypePath">The containing type path</param>
		/// <exception cref="MetadataParseException">The type path creating errors</exception>
		internal static (ExpressionTypeReference type, string typePath) ParseReferenceStatement(string typePath, string baseTypePath)
		{
			if (typePath.StartsWith(':'))
			{
				// Specify to use referenced type attribute or function
				// TODO: do-do. can be storage separately from type (3rd element in a tuple ExprEntity etc)
				// TODO: if several references in attrs, then 'entity:' or 'max:' else ':' (for Props)
			}
			else if (typePath.StartsWith('@'))
			{
				// Relative reference
				
				// Create absolute type reference from relative type reference
				int firstDot = typePath.IndexOf('.');
				if (firstDot < 0)
				{
					throw new MetadataParseException("Relative reference has wrong format");
				}
				
				string et = baseTypePath;
				if (firstDot > 1)
				{
					if (typePath[1..firstDot].TryToNullableInt32() is not int count)
					{
						throw new MetadataParseException("Relative reference doesn't contain a number");
					}

					// Count equal to 1 counted as the current containing type (doesn't change anything)
					count--;
					if (count < 0)
					{
						throw new MetadataParseException("Relative reference contains a non-positive number");
					}
					if (count > 0)
					{
						// Move up 'count' times
						int j = baseTypePath.Length;
						while (j > 0 && count > 0)
						{
							if (baseTypePath[--j] == '.')
							{
								count--;
							}
						}
						
						if (count > 0)
						{
							throw new MetadataParseException($"Relative reference number out of range. Extra rest is {count.ToString()}");
						}
						
						et = baseTypePath[..j];
					}
				}

				typePath = et + typePath[firstDot..];

				// TODO: ':' in the last
			}
			else
			{
				// Absolute reference
				
				int firstDot = typePath.IndexOf('.');
				if (firstDot < 0)
				{
					throw new MetadataParseException("Absolute reference has wrong format");
				}
				// Concatenate base type
				if (firstDot == 0)
				{
					typePath = GetRootType(baseTypePath) + typePath;
				}

				// TODO: ':' in the last
			}

			// Add reference "type" at runtime
			if (!TypeReferences.TryGetValue(typePath, out ExpressionTypeReference type))
			{
				TypeReferences.Add(typePath, type = new ExpressionTypeReference(typePath, ExpressionTypeReference.Contexts.Reference, null));
			}
			
			return (type, typePath);
			
			// Local Funcs
			
			static string GetRootType(string t) => t.IndexOf('.') is int d and > 0 ? t[..d] : t;
		}
		
		/// <summary>
		/// Parse metadata XML files and save they in the memory
		/// </summary>
		internal static void Initialize()
		{
			#region Load data from XSD

			const string typesXsd = "types.xsd";
			const string dirsXsd = "directories.xsd";
			
			foreach (FileInfo file in new DirectoryInfo(pathXsd).GetFiles())
			{
				const string common = "common";
				const string simple = "simple";
				const string list = "list";
				
				XElement root = XDocument.Load(file.OpenText()).Root;
				switch (file.Name)
				{
					case typesXsd:
					{
						Dictionary<string, ExpressionTypeReference> types = root
							.GetElementsNs("simpleType")
							.SelectMany(e =>
							{
								string name = e.GetAttributeValue("name");
								if (name.NotIn(common, simple, list))
								{
									return Enumerable.Empty<ExpressionTypeReference>();
								}

								// Get built-in types from "types.xsd"
								IEnumerable<ExpressionTypeReference> types = e
									.GetElementsNs("union/simpleType/restriction/enumeration")
									.Select(enumeration =>
									{
										string typeName = enumeration.GetAttributeValue("value");
										// TODO: проверка регулярками из this:reference
										string typeComment = enumeration.GetValueNs("annotation/documentation");
										ExpressionTypeReference.Contexts typeContext = name switch
										{
											common => ExpressionTypeReference.Contexts.Simple | ExpressionTypeReference.Contexts.Complex | ExpressionTypeReference.Contexts.List,
											simple => ExpressionTypeReference.Contexts.Simple,
											list   => ExpressionTypeReference.Contexts.List,

											_ => throw new MetadataParseException($"Type '{name}' unsupported")
										};

										return new ExpressionTypeReference(typeName, typeContext, Utilities.TrimAnnotation(typeComment?.Trim()));
									});

								return types;
							})
							.Append(new ExpressionTypeReference("{complex}", ExpressionTypeReference.Contexts.Complex, "Complex expression containing multiple custom values."))
							.Append(new ExpressionTypeReference("{sequence}", ExpressionTypeReference.Contexts.Complex, "Sequence expression containing multiple typed complex values."))
							.GroupBy(k => k.Name, v => v)
							.Select(g =>
							{
								ExpressionTypeReference[] ets = g.ToArray();
								if (ets.Length == 1)
								{
									return ets[0];
								}
								
								// Unite eponymous types
								
								ExpressionTypeReference.Contexts context = default;
								List<string> annotation = new(ets.Length);
								foreach (ExpressionTypeReference et in ets)
								{
									context |= et.Context;
									annotation.Add(et.Annotation);
								}

								return new ExpressionTypeReference(g.Key, context, string.Join(" Or. ", annotation));
							})
							.ToDictionary(k => k.Name, v => v);

						TypeReferences = new(types);
						
						break;
					}

					case dirsXsd:
					{
						Dictionary<string, string> dirs = root.GetElementsNs("simpleType/restriction/enumeration")
							.Select(enumeration =>
							{
								string dirPath = enumeration.GetAttributeValue("value");
								string dirComment = enumeration.GetValueNs("annotation/documentation");

								return (dirPath, dirComment: Utilities.TrimAnnotation(dirComment?.Trim()));
							})
							.ToDictionary(k => k.dirPath, v => v.dirComment);

						Directories = new(dirs);
						
						break;
					}
				}
			}

			if (TypeReferences == null)
			{
				throw new MetadataParseException($"The schema '{typesXsd}' was not found");
			}
			if (Directories == null)
			{
				throw new MetadataParseException($"The schema '{dirsXsd}' was not found");
			}

			#endregion // Load data from XSD

			#region Load data from XML

			Files = new();
			Types = new();
			Dictionary<string, StringBuilder> errors = new();
			foreach (FileInfo file in new DirectoryInfo(pathXml).GetFiles())
			{
				// TODO: пока эффект нерабочий
				if (file.Name.In("_effects.xml"))
				{
					continue;
				}

				// Get <reference> or <file> metadata elements
				XElement root = XDocument.Load(file.OpenText()).Root;
				IEnumerable<XElement> refs = root.Name.LocalName switch
				{
					XmlConstants.Reference  => new[] { root },
					XmlConstants.References => root.GetElementsNs(XmlConstants.Reference),

					_ => null
				};
				IEnumerable<XElement> files = root.Name.LocalName switch
				{
					XmlConstants.File  => new[] { root },
					XmlConstants.Files => root.GetElementsNs(XmlConstants.File),

					_ => null
				};

				if (refs == null && files == null)
				{
					errors.Add(file.Name, new StringBuilder($"Xml root element in the {file} must be one of the <{XmlConstants.Reference}>, <{XmlConstants.References}>, <{XmlConstants.File}>, <{XmlConstants.Files}>"));
					continue;
				}

				// Parse the file
				int i = 0;
				MetadataParseErrors metaErrors = new();
				StringBuilder metaErrorsString = new();
				foreach (XElement r in refs ?? files)
				{
					try
					{
						MetadataParseErrors errs;
						if (refs == null)
						{
							ExpressionFileProps props = new(r);
							
							// The path must be unique
							if (Files.ContainsKey(props.Path))
							{
								throw new MetadataParseException($"The file with path '{props.Path}' is already presence in the collection");
							}
							
							IList<IMetadataContent> exprs;
							if (r.GetAttributeValue(XmlConstants.Type) is not string type)
							{
								// Get the file content
								(exprs, errs) = CreateExpressions(r, "#file#");
							}
							else if (type.IsNullOrWhiteSpace())
							{
								throw new MetadataParseException($"The '{XmlConstants.Type}' attribute doesn't contain value");
							}
							else
							{
								string entity = r.GetAttributeValue(XmlConstants.Entity);

								// TODO: max=* по дефолту
								// Create file content from shorthand
								exprs = new List<IMetadataContent>
								{
									new ComplexExpressionMetadata(entity, RelationTypes.Assigning, new ComplexExpressionValueMetadata(new ComplexExpressionProps(null as ExpressionProps), ExpressionTypeReference.Find(type)))
								};
								errs = null;
							}

							Files.Add(props.Path, new ExpressionFileStructure(props, exprs));
						}
						else
						{
							ExpressionTypeReference type = ExpressionTypeReference.Find(r.GetAttributeValue(XmlConstants.Type));
							// Type is required
							if (type == null)
							{
								throw new MetadataParseException($"Doesn't contain '{XmlConstants.Type}' attribute or it's value is empty");
							}
							// The type must be unique
							if (Types.ContainsKey(type.Name))
							{
								throw new MetadataParseException($"The type '{type.Name}' is already presence in the collection");
							}

							ExpressionTypeProps props = new(r);
							IList<IMetadataContent> content;
							if (r.GetAttributeValue(XmlConstants.Custom) is string custom)
							{
								if (custom.TryToNullableBoolean() == null)
								{
									throw new MetadataParseException($"The '{XmlConstants.Custom}' attribute doesn't contain boolean value");
								}
								
								(content, errs) = CreateValues(r, type.Name);
								if (content?.Count == 1 && content[0] is ComplexExpressionValueMetadata cv)
								{
									// Compact complex content
									content = cv.Content;
									
									// Replace properties to inner
									props = new ExpressionTypeProps(cv.Props)
									{
										Definition = props.Definition
									};
								}
							}
							else
							{
								(content, errs) = CreateExpressions(r, type.Name);
							}
							
							Types.Add(type.Name, new ExpressionType(props, type, content));
						}
						
						metaErrors.Add(i, errs);
					}
					catch (Exception e)
					{
						metaErrors.Add(i, new MetadataParseErrors(e));
					}

					if (metaErrors.Contains(i))
					{
						CreateErrors(r, i, metaErrors.Get(i));
					}

					i++;
					
					// Local Funcs

					void CreateErrors(XElement xml, int index, MetadataParseErrors errors, int depth = 1)
					{
						// Head xml part to string
						XElement head = new(xml);
						head.RemoveNodes();
						if (depth != 1)
						{
							// Delete 'xmlns' for inner xml
							head.Name = head.Name.LocalName;
						}
						metaErrorsString.Append(new string('\t', depth)).AppendFormat("[{0}] ", (index + 1).ToString()).AppendLine(head.ToString());

						XElement[] elems = xml.GetElementsNs().ToArray();
						foreach (var error in errors)
						{
							CreateErrors(elems[error.Key], error.Key, error.Value, depth + 1);
						}

						// Found real error
						if (errors.Error is { } e)
						{
							metaErrorsString.Append(new string('\t', depth + 1)).AppendLine(e.Message);
						}
					}
				}

				// Store file errors
				if (metaErrorsString.Length != 0)
				{
					errors.Add(file.Name, metaErrorsString);
				}
			}
			
			#endregion // Load data from XML
			// TODO: test, need return further or something
			string error = string.Join(Environment.NewLine, errors.Select(kv => $"{kv.Key}:{Environment.NewLine}{kv.Value}"));

			#region Linking runtime type references

			foreach (ExpressionTypeReference etr in TypeReferences.Values.Where(tr => tr.Context == ExpressionTypeReference.Contexts.Reference))
			{
				try
				{
					string[] path = etr.Name.Split('.'); // TODO: ':'
					IList<IMetadataContent> content = Types[path[0]].Content;
					for (int i = 1; i < path.Length; i++)
					{
						bool isEnd = (i == path.Length - 1);
						
						// Meta entity searching
						if (path[i].StartsWith('#'))
						{
							if (isEnd)
							{
								throw new MetadataParseException($""); // TODO
							}
							
							foreach (IMetadataContent c in content)
							{
								if (c is not IChoiceExpressionMetadata choice)
								{
									continue;
								}
								
								// TODO: id to choice
								content = choice.Content.OfType<IMetadataContent>().ToList();
								break;
							}
							
							continue;
						}
						// Real entity searching
						foreach (IMetadataContent c in content)
						{
							if (c is not IExpressionMetadata cnt)
							{
								continue;
							}
							
							// Find by strict name or by type
							if (cnt.Entity.Name == path[i] ||
								cnt.Entity.Name == null && path[i] == cnt.Entity.Type.Name)
							{
								// TODO: cnt.Entity.Type.Name can be reference too => i++

								switch (cnt.Value)
								{
									case ComplexExpressionValueMetadata v:
									{
										if (isEnd)
										{
											content = new List<IMetadataContent>(1) { cnt.Value };
											break;
										}
										
										content = v.Content;
										if (content == null)
										{
											throw new MetadataParseException($""); // TODO
										}
										
										break;
									}

									case SimpleExpressionValueMetadata:
									case ListExpressionValueMetadata:
									{
										if (isEnd)
										{
											content = new List<IMetadataContent>(1) { cnt.Value };
											break;
										}
										throw new MetadataParseException($""); // TODO
									}

									default:
										throw new MetadataParseException($"The {cnt.Value.GetType()} value is not implemented");
								}
								
								break;
							}
						}
					}
					
					Types.Add(etr.Name, new ExpressionType(null, etr, content));
				}
				catch (Exception e)
				{
					// TODO: processing linking errors
				}
			}

			#endregion // Linking runtime type references
		}
		
		/// <summary>
		/// TODO:
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="exType"></param>
		private static (IList<IMetadataContent> expressions, MetadataParseErrors errors) CreateExpressions(XElement ex, string exType)
		{
			int i = -1;
			List<IMetadataContent> exprs = new();
			MetadataParseErrors errors = new();
			foreach (XElement e in ex.GetElementsNs())
			{
				i++;
				try
				{
					string eName = e.Name.LocalName;
					if (eName.In(XmlConstants.Annotation))
					{
						// Don't process not expression's elements
						continue;
					}

					#region Process <choice>

					if (eName == XmlConstants.Choice)
					{
						if (eName == ex.Name.LocalName)
						{
							throw new MetadataParseException($"Can't contains others <{XmlConstants.Choice}> elements");
						}
						
						ExpressionProps props = new(e);
						
						(IList<IMetadataContent> exs, MetadataParseErrors chsErrs) = CreateExpressions(e, $"{exType}.#?");
						
						errors.Add(i, chsErrs);
						try
						{
							if (exs == null)
							{
								throw new MetadataParseException("Doesn't contain any expression to choose");
							}
						
							IList<IExpressionMetadata> chs = exs.OfType<IExpressionMetadata>().ToList();
							if (exs.Count != chs.Count)
							{
								throw new MetadataParseException("Does contain unsupported elements");
							}

							exprs.Add(new ChoiceExpressionMetadata(props, chs));
						}
						catch (Exception error)
						{
							if (chsErrs == null)
							{
								// The first error
								throw;
							}
							chsErrs.Add(error);
						}
						
						continue;
					}

					#endregion // Process <choice>

					#region Get the right statement of the expression

					string eType = e.GetAttributeValue(XmlConstants.Type);
					// TODO: проверка регулярками из xsd-шки
					if (!ExpressionTypeReference.TryFind(eType, out ExpressionTypeReference type))
					{
						if (eType == null)
						{
							if (eName != XmlConstants.Complex)
							{
								throw new MetadataParseException($"Doesn't contain '{XmlConstants.Type}' attribute or it's value is empty");
							}
						}
						else
						{
							(type, eType) = ParseReferenceStatement(eType, exType);
						}
					}

					#endregion // Get the right statement of the expression

					MetadataParseErrors exprErrs = null;
					// Create expression objects
					IExpressionMetadata expr = eName switch
					{
						XmlConstants.Simple  => SimpleExpressionMetadata.Parse(e, type, exType),
						XmlConstants.List    => ListExpressionMetadata.Parse(e, type, exType),
						XmlConstants.Complex => ComplexExpressionMetadata.Parse(e, type, exType, CreateExpressions, out exprErrs),

						_ => throw new MetadataParseException($"The <{eName}> is not implemented")
					};

					errors.Add(i, exprErrs);
					if (expr != null)
					{
						exprs.Add(expr);
					}
				}
				catch (Exception error)
				{
					errors.Add(i, new MetadataParseErrors(error));
				}
			}
			return (exprs.GetValueOrDefault(), errors.Count == 0 ? null : errors);
		}

		/// <summary>
		/// TODO:
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="exType"></param>
		private static (IList<IMetadataContent> values, MetadataParseErrors errors) CreateValues(XElement ex, string exType)
		{
			int i = -1;
			List<IMetadataContent> values = new();
			MetadataParseErrors errors = new();
			foreach (XElement e in ex.GetElementsNs())
			{
				i++;
				try
				{
					string eName = e.Name.LocalName;
					if (eName.In(XmlConstants.Annotation))
					{
						// Don't process not template's elements
						continue;
					}
					
					ExpressionTypeReference type = ExpressionTypeReference.Find(e.GetAttributeValue(XmlConstants.Type));
					if (type == null && eName.NotIn(XmlConstants.Complex, XmlConstants.Relation))
					{
						throw new MetadataParseException($"Doesn't contain '{XmlConstants.Type}' attribute or it's value is empty");
					}

					MetadataParseErrors exprErrs = null;
					// Create value objects
					IExpressionValueMetadata value = eName switch
					{
						XmlConstants.Simple  => SimpleExpressionValueMetadata.Parse(e, type, exType),
						XmlConstants.List    => ListExpressionValueMetadata.Parse(e, type, exType),
						XmlConstants.Complex => ComplexExpressionValueMetadata.Parse(e, type, exType, CreateExpressions, out exprErrs),
						//XmlConstants.Relation => RelationExpressionMetadata.Parse(e),

						_ => throw new MetadataParseException($"The <{eName}> is not implemented")
					};

					errors.Add(i, exprErrs);
					if (value != null)
					{
						values.Add(value);
					}
				}
				catch (Exception error)
				{
					errors.Add(i, new MetadataParseErrors(error));
				}
			}
			return (values.GetValueOrDefault(), errors.Count == 0 ? null : errors);
		}
	}
}