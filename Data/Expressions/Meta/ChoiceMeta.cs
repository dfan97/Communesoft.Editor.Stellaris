namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Defines a &lt;choice&gt; metadata element
	/// </summary>
	public interface IChoiceExpressionMetadata : IMetadataContent
	{
		/// <summary>
		/// The choice properties
		/// </summary>
		ExpressionProps Props { get; }
		/// <summary>
		/// The expressions to choose. At least two
		/// </summary>
		IList<IExpressionMetadata> Content { get; }
	}
	/// <summary>
	/// Represents a &lt;choice&gt; metadata element
	/// </summary>
	public sealed class ChoiceExpressionMetadata : IChoiceExpressionMetadata
	{
		/// <summary>
		/// The choice properties
		/// </summary>
		public ExpressionProps Props { get; }
		/// <summary>
		/// The expressions to choose. At least two
		/// </summary>
		public IList<IExpressionMetadata> Content { get; }
		
		/// <summary>
		/// Creates choice between at least two expressions
		/// </summary>
		/// <param name="props">The choice properties</param>
		/// <param name="content">The expressions to choose</param>
		/// <exception cref="ArgumentException">The expressions count is less than two</exception>
		public ChoiceExpressionMetadata(ExpressionProps props, IList<IExpressionMetadata> content)
		{
			if (content?.Count is not >= 2)
			{
				throw new ArgumentException("Choice must be between at least two piece of content", nameof(content));
			}
			
			this.Props = props;
			this.Content = content;
		}

		public override string ToString() => $"#{this.Content.Count}";
	}
}