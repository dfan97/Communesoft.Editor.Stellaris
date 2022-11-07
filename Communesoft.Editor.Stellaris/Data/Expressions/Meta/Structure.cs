namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Defines a metadata root structure
	/// </summary>
	public interface IExpressionValueMetadataRootStructure : IMetadataStructure, IExpressionValueMetadata
	{
		
	}
	
	/// <summary>
	/// Represents a &lt;file&gt; metadata element. Describes real files structure in a directory
	/// </summary>
	public sealed class ExpressionFileStructure : ExpressionValueMetadataStructure<ExpressionFileProps>, IExpressionValueMetadataRootStructure
	{
		/// <summary>
		/// Creates files structure metadata
		/// </summary>
		/// <param name="props">The files properties</param>
		/// <param name="content">The files structure</param>
		/// <exception cref="ArgumentException">The file structure must contains at least one expression</exception>
		public ExpressionFileStructure(ExpressionFileProps props, IList<IMetadataContent> content) : base(props, null, content)
		{
			if (content?.Count is not > 0)
			{
				throw new ArgumentException("The file structure must contains at least one expression", nameof(content));
			}
		}

		public override string ToString() => $"\"{this.Props.Path}\"{base.ToString()}";
	}
	
	/// <summary>
	/// Represents a &lt;reference&gt; metadata element. Describes an expression type with properties and content
	/// </summary>
	public sealed class ExpressionType : ExpressionValueMetadataStructure<ExpressionTypeProps>, IExpressionValueMetadataRootStructure
	{
		/// <summary>
		/// Creates an expression type
		/// </summary>
		/// <param name="props">The type properties</param>
		/// <param name="type">The type reference used by this type to reference it</param>
		/// <param name="content">The type structure</param>
		public ExpressionType(ExpressionTypeProps props, ExpressionTypeReference type, IList<IMetadataContent> content) : base(props, type, content) { }
	}
	/// <summary>
	/// Represents an expression type reference
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay}")]
	public sealed class ExpressionTypeReference
	{
		#if DEBUG

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => this.Annotation.IsNullOrWhiteSpace() ? this.ToString() : $"{this} - {this.Annotation}";

		#endif
		
		private static ExpressionTypeReference @string, complex, sequence, unknown;
		/// <summary>
		/// String type when a raw string statement is met
		/// </summary>
		public static ExpressionTypeReference String => @string ??= Find("{string}");
		/// <summary>
		/// Complex type when a raw complex statement is met
		/// </summary>
		public static ExpressionTypeReference Complex => complex ??= Find("{complex}");
		/// <summary>
		/// Sequence type when a marked (with the sequence="only" attribute) complex statement is met
		/// </summary>
		public static ExpressionTypeReference Sequence => sequence ??= Find("{sequence}");
		/// <summary>
		/// Unknown type when metadata is not found or is not full filled
		/// </summary>
		public static ExpressionTypeReference Unknown => unknown ??= Find("{?}");
		
		/// <summary>
		/// Finds a type with the name
		/// </summary>
		/// <param name="name">The type name</param>
		/// <returns>The found type</returns>
		/// <exception cref="KeyNotFoundException">The type with <paramref name="name"/> was not found</exception>
		public static ExpressionTypeReference Find(string name)
		{
			if (name.IsNullOrWhiteSpace())
			{
				return null;
			}
			if (TryFind(name, out ExpressionTypeReference type))
			{
				return type;
			}
			throw new KeyNotFoundException($"The '{name}' type was not found");
		}
		/// <summary>
		/// Tries find a type with the name
		/// </summary>
		/// <param name="name">The type name</param>
		/// <param name="type">The found type</param>
		public static bool TryFind(string name, out ExpressionTypeReference type)
		{
			if (name.IsNullOrWhiteSpace())
			{
				type = null;
				return false;
			}
			return Metadata.TypeReferences.TryGetValue(name, out type);
		}

		/// <summary>
		/// The type comment
		/// </summary>
		public string Annotation { get; }
		
		/// <summary>
		/// The type name
		/// </summary>
		public string Name { get; }
		
		/// <summary>
		/// Contains expression type context
		/// </summary>
		[Flags]
		public enum Contexts
		{
			/// <summary>
			/// Can occurs only in a simple expression
			/// </summary>
			Simple = 1 << 0,
			/// <summary>
			/// Can occurs only in a complex expression
			/// </summary>
			Complex = 1 << 1,
			/// <summary>
			/// Can occurs only in a list expression
			/// </summary>
			List = 1 << 2,
			
			/// <summary>
			/// Can occurs in any expression via context and represents runtime link to a type
			/// </summary>
			Reference = 1 << 3
		}
		/// <summary>
		/// The type context
		/// </summary>
		public Contexts Context { get; }

		/// <summary>
		/// Creates an expression type reference
		/// </summary>
		/// <param name="name">The type name or path</param>
		/// <param name="context">The type possible contexts</param>
		/// <param name="annotation">The type metadata annotation</param>
		public ExpressionTypeReference(string name, Contexts context, string annotation)
		{
			this.Name = name;
			this.Context = context;
			this.Annotation = annotation;
		}

		public override string ToString() => (this.Context & Contexts.Reference) != 0 ? $"[{this.Name}]" : this.Name;
	}
}