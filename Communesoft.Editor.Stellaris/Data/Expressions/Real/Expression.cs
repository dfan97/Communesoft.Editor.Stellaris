namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Defines an expression with operands and operation
	/// </summary>
	public interface IExpression : IComplexContent
	{
		/// <summary>
		/// The metadata
		/// </summary>
		IExpressionMetadata Metadata { get; }
		/// <summary>
		/// The statement before relation sign. Represents specific expression's identifier (not necessarily unique)
		/// </summary>
		string Entity { get; }
		/// <summary>
		/// The relation between <see cref="Entity"/> and <see cref="Value"/>
		/// </summary>
		RelationSigns Relation { get; }
		/// <summary>
		/// The statement after relation sign. Represents specific value
		/// </summary>
		IExpressionValue Value { get; }
	}
	/// <summary>
	/// Represents an expression with operands and operation
	/// </summary>
	/// <typeparam name="TMeta">Specific expression metadata</typeparam>
	/// <typeparam name="TValue">Specific value</typeparam>
	public abstract class Expression<TMeta, TValue> : IExpression
		where TMeta : class, IExpressionMetadata
		where TValue : class, IExpressionValue
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IExpressionMetadata IExpression.Metadata => this.Metadata;
		/// <summary>
		/// The metadata
		/// </summary>
		public TMeta Metadata { get; }
		
		/// <summary>
		/// The statement before relation sign. Represents specific expression's identifier (not necessarily unique)
		/// </summary>
		public string Entity { get; }
		/// <summary>
		/// The relation between <see cref="Entity"/> and <see cref="Value"/>
		/// </summary>
		public RelationSigns Relation { get; }
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IExpressionValue IExpression.Value => this.Value;
		/// <summary>
		/// The statement after relation sign. Represents specific value
		/// </summary>
		public TValue Value { get; }

		/// <summary>
		/// Creates an expression
		/// </summary>
		/// <param name="metadata">The expression metadata</param>
		/// <param name="entity">The expression entity</param>
		/// <param name="relation">The expression relation</param>
		/// <param name="value">The expression value</param>
		public Expression(TMeta metadata, string entity, RelationSigns relation, TValue value)
		{
			this.Metadata = metadata;
			this.Entity = entity;
			this.Relation = relation;
			this.Value = value;
		}

		public override string ToString() => $"{this.Entity} {this.Relation.GetString()} {this.Value}";
	}
	
	/// <summary>
	/// Defines an expression value
	/// </summary>
	public interface IExpressionValue : IAnnotatable, ISimpleContent, IListContent, IComplexContent
	{
		/// <summary>
		/// The metadata
		/// </summary>
		IExpressionValueMetadata Metadata { get; }
		/// <summary>
		/// The content
		/// </summary>
		IList<IExpressionContent> Value { get; }
	}
	/// <summary>
	/// Represents a value of specific type
	/// </summary>
	/// <typeparam name="TValue">Specific value type</typeparam>
	/// <typeparam name="TMeta">Specific value metadata</typeparam>
	public abstract class ExpressionValue<TValue, TMeta> : IExpressionValue
		where TValue : class, IExpressionContent
		where TMeta : class, IExpressionValueMetadata
	{
		/// <summary>
		/// The annotation
		/// </summary>
		public string Annotation { get; }
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IExpressionValueMetadata IExpressionValue.Metadata => this.Metadata;
		/// <summary>
		/// The metadata
		/// </summary>
		public TMeta Metadata { get; }
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IList<IExpressionContent> IExpressionValue.Value => this.Value.OfType<IExpressionContent>().ToArray();
		/// <summary>
		/// The content
		/// </summary>
		public IList<TValue> Value { get; }

		/// <summary>
		/// Creates an expression value
		/// </summary>
		/// <param name="annotation">The value annotation</param>
		/// <param name="metadata">The value metadata</param>
		/// <param name="value">The value content</param>
		public ExpressionValue(string annotation, TMeta metadata, IList<TValue> value)
		{
			this.Annotation = annotation;
			this.Metadata = metadata;
			this.Value = value;
		}

		public override string ToString()
		{
			string s = this.Metadata?.Type.ToString() ?? "";
			if (this.Value != null)
			{
				s += $" #{this.Value.Count}";
			}
			return s.Trim();
		}
	}
	
	/// <summary>
	/// Represents a raw token value
	/// </summary>
	public sealed class RawExpressionValue : ISimpleContent, IListContent
	{
		public string Value { get; }
		// TODO: можно использовать StringToken, так же как для relation использовать RelationToken
		// TODO: либо в Type, либо сюда нужно добавить enum, чтобы определять, что перед нами - ссылка, строка, число итп, также технически это может совпадать с типами из StringToken
		
		public RawExpressionValue(string value)
		{
			this.Value = value;
		}

		public override string ToString() => this.Value;
	}
}