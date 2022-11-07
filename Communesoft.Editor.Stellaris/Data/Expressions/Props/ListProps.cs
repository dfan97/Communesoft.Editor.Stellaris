using System.Xml.Linq;
using Communesoft.Common.Utils.Convert;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents <see cref="ListExpression"/> properties
	/// </summary>
	public class ListExpressionProps : ExpressionProps, IExpressionShortProp, IExpressionRawValueLimitsProps, IExpressionRawValueCountProps, IExpressionEnumsProp
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly decimal? minValue;
		/// <summary>
		/// Minimum for numeric values
		/// </summary>
		public decimal? MinValue
		{
			get => this.minValue;
			init => this.minValue = CheckMinMaxValue(value, this.maxValue, max: false);
		}
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly decimal? maxValue;
		/// <summary>
		/// Maximum for numeric values
		/// </summary>
		public decimal? MaxValue
		{
			get => this.maxValue;
			init => this.maxValue = CheckMinMaxValue(value, this.minValue, max: true);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int? minValueCount;
		/// <summary>
		/// Minimum list elements count
		/// </summary>
		public int? MinValueCount
		{
			get => this.minValueCount;
			init => this.minValueCount = (int?)CheckMinMaxValue(value, this.maxValueCount, max: false, positive: true);
		}
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int? maxValueCount;
		/// <summary>
		/// Maximum list elements count
		/// </summary>
		public int? MaxValueCount
		{
			get => this.maxValueCount;
			init => this.maxValueCount = (int?)CheckMinMaxValue(value, this.minValueCount, max: true, positive: true);
		}
		
		/// <summary>
		/// The expression shorthand
		/// </summary>
		public ExpressionTypeReference Short { get; init; }
		
		/// <summary>
		/// Enumeration values restrict or extend the expression
		/// </summary>
		public EnumExpressionValues Enums { get; init; }

		public ListExpressionProps(ExpressionProps props = null) : base(props)
		{
			if (props is IExpressionRawValueLimitsProps lps)
			{
				this.MinValue = lps.MinValue;
				this.MaxValue = lps.MaxValue;
			}
			if (props is IExpressionRawValueCountProps cps)
			{
				this.MinValueCount = cps.MinValueCount;
				this.MaxValueCount = cps.MaxValueCount;
			}
			if (props is IExpressionShortProp sp)
			{
				this.Short = sp.Short;
			}
			if (props is IExpressionEnumsProp ep)
			{
				this.Enums = ep.Enums;
			}
		}
		public ListExpressionProps(XElement xml) : base(xml)
		{
			this.MinValue = xml.GetAttributeValue(XmlConstants.MinValue)?.ToDecimal();
			this.MaxValue = xml.GetAttributeValue(XmlConstants.MaxValue)?.ToDecimal();
			if (xml.GetAttributeValue(XmlConstants.ValueCount)?.ToInt32() is int count)
			{
				if (xml.GetAttribute(XmlConstants.MinValueCount) != null || xml.GetAttribute(XmlConstants.MaxValueCount) != null)
				{
					throw new MetadataParseException($"Simultaneously accepted either '{XmlConstants.ValueCount}' attribute or '{XmlConstants.MinValueCount}' and '{XmlConstants.MaxValueCount}' attributes");
				}
				this.MinValueCount = this.MaxValueCount = count;
			}
			else
			{
				this.MinValueCount = xml.GetAttributeValue(XmlConstants.MinValueCount)?.ToInt32();
				this.MaxValueCount = xml.GetAttributeValue(XmlConstants.MaxValueCount)?.ToInt32();
			}
			
			this.Short = ExpressionTypeReference.Find(xml.GetAttributeValue(XmlConstants.Short));
			
			this.Enums = EnumExpressionValues.TryCreate(xml);
		}
	}
}