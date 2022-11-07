namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a complex expression, containing multiple expressions
	/// </summary>
	public sealed class ComplexExpression : Expression<ComplexExpressionMetadata, ComplexExpressionValue>
	{
		public ComplexExpression(ComplexExpressionMetadata metadata, string entity, RelationSigns relation, ComplexExpressionValue value) : base(metadata, entity, relation, value) { }
	}
	/// <summary>
	/// Represents a complex statement, containing multiple expressions
	/// </summary>
	public sealed class ComplexExpressionValue : ExpressionValue<IComplexContent, ComplexExpressionValueMetadata>
	{
		public ComplexExpressionValue(string annotation, ComplexExpressionValueMetadata metadata, IList<IComplexContent> value) : base(annotation, metadata, value) { }
	}
}