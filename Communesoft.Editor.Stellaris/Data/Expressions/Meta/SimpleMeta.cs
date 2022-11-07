using System.Xml.Linq;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a &lt;simple&gt; metadata element
	/// </summary>
	public sealed class SimpleExpressionMetadata : ExpressionMetadata<SimpleExpressionValueMetadata>
	{
		/// <summary>
		/// Parses the xml to create <see cref="SimpleExpressionMetadata"/>
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="type">The expression type reference</param>
		/// <param name="baseTypePath">The type path, containing this expression</param>
		public static SimpleExpressionMetadata Parse(XElement xml, ExpressionTypeReference type, string baseTypePath)
		{
			var t = Metadata.GetEntityRelationStatements(xml, baseTypePath);
			var value = SimpleExpressionValueMetadata.Parse(xml, type, baseTypePath);

			return new SimpleExpressionMetadata(t.entity, t.relation, value);
		}
		
		private SimpleExpressionMetadata(string entity, RelationTypes relation, SimpleExpressionValueMetadata value) : base(entity, relation, value) { }
	}
	/// <summary>
	/// Represents a value metadata with <see cref="SimpleExpressionProps"/>
	/// </summary>
    public sealed class SimpleExpressionValueMetadata : ExpressionValueMetadata<SimpleExpressionProps>
    {
		/// <summary>
		/// Parses the xml to create <see cref="SimpleExpressionValueMetadata"/>
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="type">The value type reference</param>
		/// <param name="baseTypePath">The type path, containing this value</param>
		public static SimpleExpressionValueMetadata Parse(XElement xml, ExpressionTypeReference type, string baseTypePath)
		{
			return new SimpleExpressionValueMetadata(new SimpleExpressionProps(xml), type);
		}
		
		private SimpleExpressionValueMetadata(SimpleExpressionProps props, ExpressionTypeReference type) : base(props, type) { }
    }
}