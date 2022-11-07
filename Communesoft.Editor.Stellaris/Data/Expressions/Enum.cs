using System.Xml.Linq;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents an expression enumeration value
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay}")]
	public sealed class EnumExpressionValue
	{
		#if DEBUG
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay
		{
			get
			{
				string display = this.ToString();
				if (this.Annotation.IsNotNullOrWhiteSpace())
				{
					display += $" - {this.Annotation}";
				}
				return display;
			}
		}

		#endif
		
		/// <summary>
		/// The enum value comment
		/// </summary>
		public string Annotation { get; }
		/// <summary>
		/// The enum value
		/// </summary>
		public string Value { get; }
		
		public EnumExpressionValue(string annotation, string value)
		{
			this.Annotation = annotation;
			this.Value = value;
		}
		
		public override string ToString() => this.Value;
	}
	/// <summary>
	/// Represents enumeration values restrict or extend an expression
	/// </summary>
	[DebuggerDisplay("IsRestriction: {IsRestriction} Count = {Values.Count}")]
	public sealed class EnumExpressionValues
	{
		/// <summary>
		/// This values restrict (otherwise extend) an expression value scope
		/// </summary>
		public bool IsRestriction { get; }
		/// <summary>
		/// The enum values
		/// </summary>
		public List<EnumExpressionValue> Values { get; }
		
		public EnumExpressionValues(bool restriction, IList<EnumExpressionValue> values)
		{
			if (values?.Count is not > 0)
			{
				throw new ArgumentException("The enumeration must contains at least one element", nameof(values));
			}
			
			this.IsRestriction = restriction;
			this.Values = values.ToList();
		}

		/// <summary>
		/// Try get enum block in <paramref name="xml"/>
		/// </summary>
		public static EnumExpressionValues TryCreate(XElement xml)
		{
			EnumExpressionValues enums = null;
			XElement ext = xml.GetElementNs(XmlConstants.Extension);
			XElement restr = xml.GetElementNs(XmlConstants.Restriction);
			if (ext != null || restr != null)
			{
				var values = (ext ?? restr).GetElementsNs(XmlConstants.Enum).Select(ev =>
					new EnumExpressionValue(
						ev.GetValueNs(XmlConstants.Annotation),
						ev.GetAttributeValue(XmlConstants.Value)
					)
				);
				enums = new(restr != null, values.ToList());
			}

			return enums;
		}
	}
}