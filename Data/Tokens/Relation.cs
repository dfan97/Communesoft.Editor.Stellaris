namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Contains relation types
	/// </summary>
	[Obsolete("Перевести на Boolean")]
	public enum RelationTypes : byte
	{
		/// <summary> Value assigning relation </summary>
		Assigning,
		/// <summary> Boolean relation </summary>
		Boolean
	}
	/// <summary>
	/// Contains relations
	/// </summary>
	public enum RelationSigns : byte
	{
		/// <summary> = </summary>
		Equal,
		/// <summary> != </summary>
		NotEqual,
		/// <summary> &gt; </summary>
		Greater,
		/// <summary> &gt;= </summary>
		GreaterOrEqual,
		/// <summary> &lt; </summary>
		Less,
		/// <summary> &lt;= </summary>
		LessOrEqual
	}
	/// <summary>
	/// Represents a relation between objects
	/// </summary>
	public sealed class RelationToken : Token
	{
		/// <summary>
		/// The relation is boolean
		/// </summary>
		public bool Boolean { get; }
		/// <summary>
		/// The relation type
		/// </summary>
		public RelationTypes Type { get; }
		/// <summary>
		/// The relation sign
		/// </summary>
		public RelationSigns Sign { get; }
		
		/// <summary>
		/// Possible expression's tokens
		/// </summary>
		public IList<IToken> Relations { get; }

		public RelationToken(ICollection<char> buffer, in TokenInfo info) : base(buffer, info)
		{
			this.Sign = this.RawValue switch
			{
				"="  => RelationSigns.Equal,
				"!=" => RelationSigns.NotEqual,
				">"  => RelationSigns.Greater,
				">=" => RelationSigns.GreaterOrEqual,
				"<"  => RelationSigns.Less,
				"<=" => RelationSigns.LessOrEqual,

				_ => throw new NotImplementedException($"{this.RawValue} is not implemented. {info.ToString()}")
			};

			this.Type = this.Sign switch
			{
				RelationSigns.Equal => RelationTypes.Assigning,
				_                   => RelationTypes.Boolean
			};
			this.Boolean = this.Sign == RelationSigns.Equal;

			this.Relations = new List<IToken>(8);
		}
		
		public static implicit operator RelationTypes(RelationToken token) => token.Type;
		public static implicit operator RelationSigns(RelationToken token) => token.Sign;
	}

	public static class RelationUtilities
	{
		/// <summary>
		/// Converts to relation type
		/// </summary>
		/// <exception cref="MetadataParseException">Unsupported relation statement type</exception>
		public static RelationTypes ConvertToRelationType(string relation) => relation switch
		{
			"assign" => RelationTypes.Assigning,
			"bool"   => RelationTypes.Boolean,
			null     => RelationTypes.Assigning,
							
			_ => throw new MetadataParseException($"Unsupported relation statement type: {relation}")
		};
		
		/// <summary>
		/// Gets string representation of the sign
		/// </summary>
		/// <exception cref="NotImplementedException"><paramref name="sign"/> is not implemented</exception>
		public static string GetString(this RelationSigns sign) => sign switch
		{
			RelationSigns.Equal          => "=",
			RelationSigns.NotEqual       => "!=",
			RelationSigns.Greater        => ">",
			RelationSigns.GreaterOrEqual => ">=",
			RelationSigns.Less           => "<",
			RelationSigns.LessOrEqual    => "<=",
			
			_ => throw new NotImplementedException($"{sign} is not implemented")
		};
	}
}