using System.Xml.Linq;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a &lt;list&gt; metadata element
	/// </summary>
	public sealed class ListExpressionMetadata : ExpressionMetadata<ListExpressionValueMetadata>
	{
		/// <summary>
		/// Parses the xml to create <see cref="ListExpressionMetadata"/>
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="type">The expression type reference</param>
		/// <param name="baseTypePath">The type path, containing this expression</param>
		public static ListExpressionMetadata Parse(XElement xml, ExpressionTypeReference type, string baseTypePath)
		{
			var t = Metadata.GetEntityRelationStatements(xml, baseTypePath);
			var value = ListExpressionValueMetadata.Parse(xml, type, baseTypePath);

			return new ListExpressionMetadata(t.entity, t.relation, value);
		}
		
		private ListExpressionMetadata(string entity, RelationTypes relation, ListExpressionValueMetadata value) : base(entity, relation, value) { }
	}
	/// <summary>
	/// Represents a value metadata with <see cref="ListExpressionProps"/>
	/// </summary>
    public sealed class ListExpressionValueMetadata : ExpressionValueMetadata<ListExpressionProps>
    {
		/// <summary>
		/// Parses the xml to create <see cref="ListExpressionValueMetadata"/>
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="type">The value type reference</param>
		/// <param name="baseTypePath">The type path, containing this value</param>
		public static ListExpressionValueMetadata Parse(XElement xml, ExpressionTypeReference type, string baseTypePath)
		{
			return new ListExpressionValueMetadata(new ListExpressionProps(xml), type);
		}
		
		private ListExpressionValueMetadata(ListExpressionProps props, ExpressionTypeReference type) : base(props, type) { }
    }
}