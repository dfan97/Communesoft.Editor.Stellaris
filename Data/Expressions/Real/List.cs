namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a complex expression, containing values with identical type
	/// </summary>
	public sealed class ListExpression : Expression<ListExpressionMetadata, ListExpressionValue>
	{
		public ListExpression(ListExpressionMetadata metadata, string entity, RelationSigns relation, ListExpressionValue value) : base(metadata, entity, relation, value) { }
	}
	/// <summary>
	/// Represents a complex statement, containing values with identical type
	/// </summary>
	public sealed class ListExpressionValue : ExpressionValue<IListContent, ListExpressionValueMetadata>
	{
		public ListExpressionValue(string annotation, ListExpressionValueMetadata metadata, IList<IListContent> value) : base(annotation, metadata, value) { }
	}
}