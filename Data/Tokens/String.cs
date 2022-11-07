using System.Globalization;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a string
	/// </summary>
	public sealed class StringToken : Token
	{
		/// <summary>
		/// Contains string token types
		/// </summary>
		public enum Types
		{
			/// <summary> Simple string or key </summary>
			String,
			/// <summary> Int number </summary>
			Int,
			/// <summary> Float number </summary>
			Float,
			/// <summary> 'yes' or 'no' values </summary>
			Boolean,
			/// <summary> String represents variable (start with '@') </summary>
			Variable,
			/// <summary> String represents inline math (start with '@\') </summary>
			Math,
			/// <summary> String represents path to file (contains path separators) </summary>
			Path,
			
			/// <summary> String represents complex sequence of chars </summary>
			Sequence
		}
		/// <summary>
		/// The string token type
		/// </summary>
		public Types Type { get; }

		/// <summary>
		/// Try get numeric value
		/// </summary>
		public decimal? TryGetNumber()
		{
			if (decimal.TryParse(this.RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
			{
				return value;
			}
			return null;
		}
		/// <summary>
		/// Try get boolean value
		/// </summary>
		public bool? TryGetBoolean() => this.RawValue switch
		{
			"yes" => true,
			"no" => false,
			_ => null
		};

		private static readonly char[] sequenceChars = { '.', ':', '@', '$' };
		
		public StringToken(ICollection<char> buffer, in TokenInfo info) : base(buffer, info)
		{
			if (this.TryGetBoolean().HasValue)
			{
				this.Type = Types.Boolean;
			}
			else if (this.RawValue.StartsWith("@\\[") || this.RawValue.StartsWith("@["))
			{
				// Need to parsing in higher level
				this.Type = Types.Math;
			}
			else if (this.RawValue.StartsWith('@'))
			{
				// TODO: variable (@) не может начинаться с '_' или цифры в отличие от идентификатора!!!
				this.Type = Types.Variable;
			}
			else if (this.RawValue.Contains('/'))
			{
				this.Type = Types.Path;
			}
			else if (this.TryGetNumber().HasValue)
			{
				this.Type = this.RawValue.Contains('.') ? Types.Float : Types.Int;
			}
			else if (this.RawValue.IndexOfAny(sequenceChars) != -1)
			{
				// Need to parsing in higher level
				this.Type = Types.Sequence;
			}
			else
			{
				this.Type = Types.String;
			}
		}
		
		public static implicit operator string(StringToken token) => token.RawValue;
	}
}