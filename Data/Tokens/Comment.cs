using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a comment to statement
	/// </summary>
	[DebuggerDisplay("{RawValue}")]
	public sealed class CommentToken : Token
	{
		/// <summary>
		/// Contained comments
		/// </summary>
		private List<CommentToken> comments;
		
		/// <summary>
		/// The comment locate on the same line with other statement and actually belong to it
		/// </summary>
		public bool IsAfterwards { get; }
		/// <summary>
		/// The comment contains other comments
		/// </summary>
		public bool IsMultiLine => this.comments.IsNotNullOrEmpty();

		/// <summary>
		/// Gets enumerable version of the comment for simplify
		/// </summary>
		private IEnumerable<CommentToken> ToEnumerable() => this.IsMultiLine ? this.comments : new[] { this };
		
		private CommentToken(CommentToken comment) : base(Array.Empty<char>(), comment.Info)
		{
			this.comments = new(comment.ToEnumerable());
		}
		public CommentToken(ICollection<char> buffer, in TokenInfo info, bool commentIsAfterwards) : base(buffer, info)
		{
			this.IsAfterwards = commentIsAfterwards;
		}
		
		private void InternalAdd(CommentToken comment)
		{
			this.comments.Add(comment);

			// Shares the same list
			comment.comments = this.comments;
		}
		/// <summary>
		/// Add a new comment (not adds itself)
		/// </summary>
		public void Add(CommentToken comment)
		{
			if (comment == this)
			{
				return;
			}
			
			this.comments ??= new() { this };
			if (!comment.IsMultiLine)
			{
				this.InternalAdd(comment);
			}
			else
			{
				foreach (CommentToken c in comment.comments)
				{
					this.InternalAdd(c);
				}
			}
		}

		/// <summary>
		/// Converts comment(s) to string
		/// </summary>
		public override string ToString()
		{
			string comment = this.RawValue;
			if (this.IsMultiLine)
			{
				comment = string.Join(Environment.NewLine, this.comments.Select(c => c.RawValue));
			}
			return Utilities.TrimAnnotation(comment);
		}

		/// <summary>
		/// Concatenates <paramref name="right"/> comment with <paramref name="left"/> comment
		/// </summary>
		public static CommentToken operator +(CommentToken left, CommentToken right)
		{
			CommentToken comment = null;
			if (left != null)
			{
				comment = new(left);
			}
			if (right != null)
			{
				if (comment == null)
				{
					comment = new(right);
				}
				else
				{
					comment.comments.AddRange(right.ToEnumerable());
				}
			}
			return comment;
		}
		/// <summary>
		/// Converts comment(s) to string
		/// </summary>
		public static implicit operator string(CommentToken comment) => comment?.ToString();
	}
}