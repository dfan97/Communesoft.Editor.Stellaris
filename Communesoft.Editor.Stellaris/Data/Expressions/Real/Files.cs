using System.IO;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents an expression's directory (https://stellaris.paradoxwikis.com/Modding#Game_structure), containing multiple expression's files and/or subdirectories
	/// </summary>
	public sealed class ExpressionDirectory
	{
		/// <summary>
		/// The directory, containing this directory
		/// </summary>
		public ExpressionDirectory Parent { get; }

		/// <summary>
		/// The real directory, containing files and/or subdirectories
		/// </summary>
		public DirectoryInfo Directory { get; }

		/// <summary>
		/// The files metadata
		/// </summary>
		public ExpressionFileStructure FileStructure { get; }
		
		/// <summary>
		/// Contained directories
		/// </summary>
		public IList<ExpressionDirectory> Directories { get; }
		/// <summary>
		/// Contained files
		/// </summary>
		public IList<ExpressionFile> Files { get; }

		/// <summary>
		/// Creates an expression's directory with the parent directory, the real directory reference with the contained files metadata
		/// </summary>
		/// <param name="parent">The parent directory</param>
		/// <param name="directory">The real directory</param>
		/// <param name="fileStructure">The files metadata</param>
		public ExpressionDirectory(ExpressionDirectory parent, DirectoryInfo directory, ExpressionFileStructure fileStructure)
		{
			this.Parent = parent;
			this.Directory = directory;
			this.FileStructure = fileStructure;
			
			this.Directories = new List<ExpressionDirectory>();
			this.Files = new List<ExpressionFile>();
		}
		
		public override string ToString()
		{
			string s = null;
			if (this.Directories.Count != 0)
			{
				s += $"Dirs: {this.Directories.Count} ";
			}
			if (this.Files.Count != 0)
			{
				s += $"Files: {this.Files.Count} ";
			}
			s += $"{this.FileStructure?.ToString() ?? $"\"{this.Directory.FullName}\""}";
			return s;
		}
	}
	/// <summary>
	/// Represents an expression's file (https://stellaris.paradoxwikis.com/Modding#Game_structure)
	/// </summary>
	public sealed class ExpressionFile
	{
		/// <summary>
		/// The directory, containing this file
		/// </summary>
		public ExpressionDirectory Parent { get; }
		
		/// <summary>
		/// The real file, containing <see cref="Content"/>
		/// </summary>
		public FileInfo File { get; }
		
		/// <summary>
		/// The file content
		/// </summary>
		public IList<IComplexContent> Content { get; }
		/// <summary>
		/// The error occurring due to parsing the file
		/// </summary>
		public Exception Error { get; }

		/// <summary>
		/// Creates an expression's file with the parent directory, the real file reference with the file content or the parsing error
		/// </summary>
		/// <param name="parent">The parent directory</param>
		/// <param name="file">The real file reference</param>
		/// <param name="content">The file content</param>
		/// <param name="error">The parsing error</param>
		public ExpressionFile(ExpressionDirectory parent, FileInfo file, IList<IComplexContent> content, Exception error)
		{
			this.Parent = parent;
			this.File = file;
			this.Content = content;
			this.Error = error;
		}
		
		public override string ToString()
		{
			string s = $"\"{this.File.FullName}\"";
			if (this.Content != null)
			{
				s += $" ~ #{this.Content.Count}";
			}
			if (this.Error != null)
			{
				s += " - HasErrors";
			}
			return s;
		}
	}
}