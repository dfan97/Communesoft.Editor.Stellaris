namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents a simple expression, containing a simple type value
	/// </summary>
	public sealed class SimpleExpression : Expression<SimpleExpressionMetadata, SimpleExpressionValue>
	{
		public SimpleExpression(SimpleExpressionMetadata metadata, string entity, RelationSigns relation, SimpleExpressionValue value) : base(metadata, entity, relation, value) { }
	}
	/// <summary>
	/// Represents a simple statement, containing a simple type value
	/// </summary>
	public sealed class SimpleExpressionValue : ExpressionValue<ISimpleContent, SimpleExpressionValueMetadata>
	{
		public SimpleExpressionValue(string annotation, SimpleExpressionValueMetadata metadata, RawExpressionValue value) : base(annotation, metadata, new List<ISimpleContent>(1) { value }) { }
		public SimpleExpressionValue(string annotation, SimpleExpressionValueMetadata metadata, IList<ISimpleContent> value) : base(annotation, metadata, value) { }
		
		public override string ToString()
		{
			if (this.Value is { Count: 1 } && this.Value[0] is RawExpressionValue)
			{
				return $"{this.Metadata?.Type} {this.Value[0]}".Trim();
			}
			return base.ToString();
		}
	}
}