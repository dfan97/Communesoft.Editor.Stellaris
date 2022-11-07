namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents an expression entity metadata
	/// </summary>
	/// <param name="Type">The entity type reference</param>
	/// <param name="Name">The entity name. Null if the entity not strictly named and can be customized with the type</param>
	public record struct ExpressionEntity(ExpressionTypeReference Type, string Name)
	{
		public override string ToString() => this.Name ?? this.Type.ToString();
	}
	
	/// <summary>
	/// Defines expression metadata
	/// </summary>
	public interface IExpressionMetadata : IMetadata, IMetadataContent, IChoiceContent
	{
		/// <summary>
		/// The entity metadata
		/// </summary>
		ExpressionEntity Entity { get; }
		/// <summary>
		/// The possible relations type between <see cref="Entity"/> and <see cref="Value"/>
		/// </summary>
		RelationTypes Relation { get; }
		/// <summary>
		/// The value metadata
		/// </summary>
		IExpressionValueMetadata Value { get; }
	}
	/// <summary>
	/// Represents expression metadata
	/// </summary>
	/// <typeparam name="TValue">Specific value metadata</typeparam>
	public abstract class ExpressionMetadata<TValue> : IExpressionMetadata
		where TValue : class, IExpressionValueMetadata
	{
		/// <summary>
		/// The entity metadata
		/// </summary>
		public ExpressionEntity Entity { get; }
		/// <summary>
		/// The possible relations type between <see cref="Entity"/> and <see cref="Value"/>
		/// </summary>
		public RelationTypes Relation { get; }
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IExpressionValueMetadata IExpressionMetadata.Value => this.Value;
		/// <summary>
		/// The value metadata
		/// </summary>
		public TValue Value { get; }

		/// <summary>
		/// Creates expression metadata
		/// </summary>
		/// <param name="entity">The expression entity</param>
		/// <param name="relation">The expression relations type</param>
		/// <param name="value">The expression value metadata</param>
		public ExpressionMetadata(string entity, RelationTypes relation, TValue value)
		{
			this.Entity = ExpressionTypeReference.TryFind(entity, out ExpressionTypeReference type) ? new(type, null) : new(ExpressionTypeReference.String, entity);
			this.Relation = relation;
			this.Value = value;
		}

		public override string ToString() => $"{this.Entity.ToString()} {(this.Relation == RelationTypes.Assigning ? "=" : ">=<")} {this.Value}";
	}
	
	/// <summary>
	/// Defines a value metadata
	/// </summary>
	public interface IExpressionValueMetadata : IMetadata, IMetadataContent
	{
		/// <summary>
		/// The properties
		/// </summary>
		IExpressionProps Props { get; }
		/// <summary>
		/// The type reference
		/// </summary>
		ExpressionTypeReference Type { get; }
	}
	/// <summary>
    /// Represents a value metadata
    /// </summary>
    /// <typeparam name="TProps">Specific metadata properties</typeparam>
    public abstract class ExpressionValueMetadata<TProps> : IExpressionValueMetadata
		where TProps : class, IExpressionProps
    {
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IExpressionProps IExpressionValueMetadata.Props => this.Props;
		/// <summary>
		/// The properties
		/// </summary>
    	public TProps Props { get; }
		/// <summary>
		/// The type reference
		/// </summary>
		public ExpressionTypeReference Type { get; }
    	
		/// <summary>
		/// Creates expression value metadata
		/// </summary>
		/// <param name="props">The value properties</param>
		/// <param name="type">The value type reference</param>
    	public ExpressionValueMetadata(TProps props, ExpressionTypeReference type)
    	{
    		this.Props = props;
			this.Type = type;
		}

		public override string ToString() => this.Type?.ToString();
	}
	
	/// <summary>
	/// Represents a value metadata base
	/// </summary>
	/// <typeparam name="TProps">Specific metadata properties</typeparam>
    public abstract class ExpressionValueMetadataStructure<TProps> : ExpressionValueMetadata<TProps>, IMetadataStructure
		where TProps : class, IExpressionProps
    {
		/// <summary>
		/// The metadata content
		/// </summary>
		public IList<IMetadataContent> Content { get; }

		/// <summary>
		/// Creates structured expression value metadata
		/// </summary>
		/// <param name="props">The value properties</param>
		/// <param name="type">The value type reference</param>
		/// <param name="content">The value structure</param>
		/// <exception cref="ArgumentException">Can't contains <see cref="IExpressionValueMetadataRootStructure"/> element in the structure</exception>
		public ExpressionValueMetadataStructure(TProps props, ExpressionTypeReference type, IList<IMetadataContent> content) : base(props, type)
		{
			if (content?.Any(c => c is IExpressionValueMetadataRootStructure) == true)
			{
				throw new ArgumentException($"{nameof(IExpressionValueMetadataRootStructure)} expressions cannot be nested", nameof(content));
			}
			
			this.Content = content;
		}

		public override string ToString()
		{
			string s = base.ToString();
			if (this.Content != null)
			{
				s += $" ~ #{this.Content.Count}";
			}
			return s;
		}
	}
}