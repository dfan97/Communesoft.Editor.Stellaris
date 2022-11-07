namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a complex token which contains other tokens
	/// </summary>
	public sealed class ComplexToken : Token
	{
		/// <summary>
		/// The inner tokens
		/// </summary>
		public IList<IToken> Value { get; }
		
		/// <summary>
		/// The complex token is start otherwise end
		/// </summary>
		public bool IsStart { get; }

		public ComplexToken(ICollection<char> buffer, in TokenInfo info) : base(buffer, info)
		{
			this.IsStart = this.RawValue == "{";
			bool isEnd = this.RawValue == "}";

			if (this.IsStart == isEnd)
			{
				throw new ArgumentOutOfRangeException(nameof(buffer), $"The complex token has incorrect input. Must be '{{' or '}}' only, was {this.RawValue}. {info.ToString()}");
			}

			if (this.IsStart)
			{
				this.Value = new List<IToken>(32);
			}
		}
	}
}