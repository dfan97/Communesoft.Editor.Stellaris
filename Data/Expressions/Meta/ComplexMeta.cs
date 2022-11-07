using System.Xml.Linq;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// TODO:
	/// </summary>
	/// <param name="xml">The xml to parse</param>
	/// <param name="baseTypePath">The type path, containing this expression</param>
	/// <returns>Parsed expressions and errors</returns>
	public delegate (IList<IMetadataContent> expressions, MetadataParseErrors errors) CreateExpressionsDelegate(XElement xml, string baseTypePath);

	/// <summary>
	/// Represents a &lt;complex&gt; metadata element
	/// </summary>
	public sealed class ComplexExpressionMetadata : ExpressionMetadata<ComplexExpressionValueMetadata>
	{
		/// <summary>
		/// Parses the xml to create <see cref="ComplexExpressionMetadata"/>
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="type">The expression type reference</param>
		/// <param name="baseTypePath">The type path, containing this expression</param>
		/// <param name="createExpressions">The function for create new subexpressions</param>
		/// <param name="errors">The errors while parsing</param>
		/// <exception cref="MetadataParseException">Parsing errors</exception>
		public static ComplexExpressionMetadata Parse(XElement xml, ExpressionTypeReference type, string baseTypePath, CreateExpressionsDelegate createExpressions, out MetadataParseErrors errors)
		{
			ComplexExpressionProps props = new(xml);
			if (type == null && props.Sequence != ComplexExpressionProps.SequenceValues.Unallowed)
			{
				throw new MetadataParseException($"The '{XmlConstants.Sequence}' attribute unacceptable in untyped complex expression");
			}
			
			(string entity, RelationTypes relation) = (ExpressionTypeReference.Sequence.Name, RelationTypes.Assigning);
			if (props.Sequence != ComplexExpressionProps.SequenceValues.Only)
			{
				(entity, relation) = Metadata.GetEntityRelationStatements(xml, baseTypePath);
			}
			
			var value = ComplexExpressionValueMetadata.Parse(xml, type, baseTypePath, createExpressions, out errors, props, entity);
			
			return new ComplexExpressionMetadata(entity, relation, value);
		}

		/// <summary>
		/// Creates complex expression metadata
		/// </summary>
		/// <param name="entity">The complex expression entity</param>
		/// <param name="relation">The complex expression relations type</param>
		/// <param name="value">The complex expression value metadata</param>
		internal ComplexExpressionMetadata(string entity, RelationTypes relation, ComplexExpressionValueMetadata value) : base(entity, relation, value) { }
	}
	/// <summary>
	/// Represents a value metadata with <see cref="ComplexExpressionProps"/>
	/// </summary>
    public sealed class ComplexExpressionValueMetadata : ExpressionValueMetadataStructure<ComplexExpressionProps>
    {
		/// <summary>
		/// Parses the xml to create <see cref="ComplexExpressionValueMetadata"/>
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="type">The value type reference</param>
		/// <param name="baseTypePath">The type path, containing this value</param>
		/// <param name="createExpressions">The function for create new subexpressions</param>
		/// <param name="errors">The errors while parsing</param>
		/// <param name="props">The value properties</param>
		/// <param name="entity">The expression entity</param>
		/// <exception cref="MetadataParseException">Parsing errors</exception>
		internal static ComplexExpressionValueMetadata Parse(XElement xml, ExpressionTypeReference type, string baseTypePath, CreateExpressionsDelegate createExpressions, out MetadataParseErrors errors,
															 ComplexExpressionProps props, string entity)
		{
			if (type != null)
			{
				if (xml.GetElements().Any(e => e.Name.LocalName != XmlConstants.Annotation))
				{
					// A typed complex statement must not has other expressions
					throw new MetadataParseException("A typed complex statement must not has other subexpressions");
				}

				errors = null;
				return new ComplexExpressionValueMetadata(props, type);
			}
			
			(IList<IMetadataContent> exprs, errors) = createExpressions(xml, $"{baseTypePath}.{entity}");
			
			return new ComplexExpressionValueMetadata(props, exprs);
		}
		/// <summary>
		/// Parses the xml to create <see cref="ComplexExpressionValueMetadata"/>
		/// </summary>
		/// <param name="xml">The xml to parse</param>
		/// <param name="type">The value type reference</param>
		/// <param name="baseTypePath">The type path, containing this value</param>
		/// <param name="createExpressions">The function for create new subexpressions</param>
		/// <param name="errors">The errors while parsing</param>
		/// <exception cref="MetadataParseException">Parsing errors</exception>
		public static ComplexExpressionValueMetadata Parse(XElement xml, ExpressionTypeReference type, string baseTypePath, CreateExpressionsDelegate createExpressions, out MetadataParseErrors errors)
		{
			return Parse(xml, type, baseTypePath, createExpressions, out errors, new ComplexExpressionProps(xml), "##complex");
		}

		/// <summary>
		/// Creates typed complex expression value metadata
		/// </summary>
		/// <param name="props">The complex value properties</param>
		/// <param name="type">The complex value type reference</param>
		internal ComplexExpressionValueMetadata(ComplexExpressionProps props, ExpressionTypeReference type) : base(props, type, null) { }
		/// <summary>
		/// Creates structured complex expression value metadata
		/// </summary>
		/// <param name="props">The complex value properties</param>
		/// <param name="content">The complex value structure</param>
		internal ComplexExpressionValueMetadata(ComplexExpressionProps props, IList<IMetadataContent> content) : base(props, ExpressionTypeReference.Complex, content) { }
	}
}