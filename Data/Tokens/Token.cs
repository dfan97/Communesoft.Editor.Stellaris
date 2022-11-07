namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Token file info
	/// </summary>
	public readonly struct TokenInfo
	{
		/// <summary>
		/// The file path
		/// </summary>
		public readonly string FilePath;
		/// <summary>
		/// The position in the file
		/// </summary>
		public readonly (long row, long column, long position) Location;

		/// <summary>
		/// Creates info tuple with the file name and the token position in it
		/// </summary>
		public TokenInfo(string filePath, (long row, long column, long position) location)
		{
			this.FilePath = filePath;
			this.Location = location;
		}
		/// <summary>
		/// Creates info tuple from the other and increment the token location
		/// TODO: remove?
		/// </summary>
		public TokenInfo(in TokenInfo info, ushort increment)
		{
			this.FilePath = info.FilePath;
			this.Location = info.Location;

			this.Location.column += increment;
			this.Location.position += increment;
		}

		/// <summary>
		/// Represents <see cref="Location"/> in a string
		/// </summary>
		public string StringLocation => $"Line: {this.Location.row.ToString()} Char in line: {this.Location.column.ToString()} Absolute position: {this.Location.position.ToString()}";
		
		public override string ToString() => $"File: {this.FilePath} {this.StringLocation}";
	}

	/// <summary>
	/// Defines a token
	/// </summary>
	public interface IToken
	{
		/// <summary>
		/// The token file info
		/// </summary>
		ref readonly TokenInfo Info { get; }
		/// <summary>
		/// The token value in the file
		/// </summary>
		string RawValue { get; }
		
		/// <summary>
		/// The token comment
		/// </summary>
		CommentToken Comment { get; }
		/// <summary>
		/// Add the comment to the token
		/// </summary>
		void AddComment(CommentToken comment);
	}
	/// <summary>
	/// Represents a token base type
	/// </summary>
	public abstract class Token : IToken
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly TokenInfo info;
		/// <summary>
		/// The token file info
		/// </summary>
		public ref readonly TokenInfo Info => ref this.info;
		/// <summary>
		/// The token value in the file
		/// </summary>
		public string RawValue { get; }
		/// <summary>
		/// The token comment
		/// </summary>
		public CommentToken Comment { get; private set; }

		/// <summary>
		/// Creates a token with the value and the file info
		/// </summary>
		/// <param name="buffer">The token value in chars. Will be cleared after assigning</param>
		/// <param name="info">The token file info</param>
		protected Token(ICollection<char> buffer, in TokenInfo info)
		{
			this.info = info;

			this.RawValue = new(buffer.ToArray());
			if (!buffer.IsReadOnly)
			{
				buffer.Clear();
			}
		}

		/// <summary>
		/// Add the comment to the token
		/// </summary>
		public void AddComment(CommentToken comment)
		{
			if (this.Comment == null)
			{
				this.Comment = comment;
			}
			else
			{
				this.Comment.Add(comment);
			}
		}
		
		public override string ToString() => this.RawValue;
	}
}