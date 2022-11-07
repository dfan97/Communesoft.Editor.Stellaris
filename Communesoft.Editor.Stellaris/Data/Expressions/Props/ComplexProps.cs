using System.Xml.Linq;
using Communesoft.Common.Utils.Convert;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents <see cref="ComplexExpression"/> properties
	/// </summary>
	public class ComplexExpressionProps : ExpressionProps, IExpressionShortProp
	{
		/// <summary>
		/// Represents 'sequence' declaration style possible values for a complex expression
		/// </summary>
		public enum SequenceValues
		{
			Unallowed,
			Allowed,
			Only
		}
		/// <summary>
		/// The expression 'sequence' declaration style realisation
		/// </summary>
		public SequenceValues Sequence { get; init; }
		/// <summary>
		/// The expression shorthand
		/// </summary>
		public ExpressionTypeReference Short { get; init; }

		public ComplexExpressionProps(ExpressionProps props = null) : base(props)
		{
			if (props is ComplexExpressionProps cep)
			{
				this.Sequence = cep.Sequence;
			}
			if (props is IExpressionShortProp sp)
			{
				this.Short = sp.Short;
			}
		}
		public ComplexExpressionProps(XElement xml) : base(xml)
		{
			if (xml.GetAttributeValue(XmlConstants.Sequence) is string seq)
			{
				this.Sequence = seq.ToEnumValue<SequenceValues>(options: ConvertToEnumOptions.StringOnly);
			}

			this.Short = ExpressionTypeReference.Find(xml.GetAttributeValue(XmlConstants.Short));
		}
	}
}