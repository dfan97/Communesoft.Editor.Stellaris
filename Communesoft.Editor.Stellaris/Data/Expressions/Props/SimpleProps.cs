using System.Xml.Linq;
using Communesoft.Common.Utils.Convert;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents <see cref="SimpleExpression"/> properties
	/// </summary>
	public class SimpleExpressionProps : ExpressionProps, IExpressionRawValueLimitsProps, IExpressionEnumsProp
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly decimal? minValue;
		/// <summary>
		/// Minimum for numeric value
		/// </summary>
		public decimal? MinValue
		{
			get => this.minValue;
			init => this.minValue = CheckMinMaxValue(value, this.maxValue, max: false);
		}
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly decimal? maxValue;
		/// <summary>
		/// Maximum for numeric value
		/// </summary>
		public decimal? MaxValue
		{
			get => this.maxValue;
			init => this.maxValue = CheckMinMaxValue(value, this.minValue, max: true);
		}
		
		/// <summary>
		/// Enumeration values restrict or extend the expression
		/// </summary>
		public EnumExpressionValues Enums { get; init; }

		public SimpleExpressionProps(ExpressionProps props = null) : base(props)
		{
			if (props is IExpressionRawValueLimitsProps lps)
			{
				this.MinValue = lps.MinValue;
				this.MaxValue = lps.MaxValue;
			}
			if (props is IExpressionEnumsProp ep)
			{
				this.Enums = ep.Enums;
			}
		}
		public SimpleExpressionProps(XElement xml) : base(xml)
		{
			this.MinValue = xml.GetAttributeValue(XmlConstants.MinValue)?.ToDecimal();
			this.MaxValue = xml.GetAttributeValue(XmlConstants.MaxValue)?.ToDecimal();
			
			this.Enums = EnumExpressionValues.TryCreate(xml);
		}
	}
}