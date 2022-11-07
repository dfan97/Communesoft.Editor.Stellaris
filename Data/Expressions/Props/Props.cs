using System.Xml.Linq;
using Communesoft.Common.Utils.Convert;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Expression's properties
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay}")]
	public class ExpressionProps : IExpressionProps, IAnnotatable, IExpressionScopeProp, IExpressionDefaultProp, IExpressionAppearanceProps
	{
		#if DEBUG

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => this.Annotation;

		#endif

		/// <summary>
		/// The expression metadata comment, concatenated from statements comments and  comments
		/// </summary>
		public string Annotation { get; init; }

		/// <summary>
		/// Scope types, where the expression can appears
		/// </summary>
		public string Scope { get; init; } // TODO: либо свой enum, либо из xsd тащим через ExpressionType или свой тип
		/// <summary>
		/// A default value for the expression
		/// </summary>
		public string Default { get; init; }
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int min;
		/// <summary>
		/// Minimum count of appearing
		/// </summary>
		public int Min
		{
			get => this.min;
			init => this.min = (int)CheckMinMaxValue(value, this.max, max: false, positive: true);
		}
		
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int? max;
		/// <summary>
		/// Maximum count of appearing
		/// </summary>
		public int? Max
		{
			get => this.max;
			init => this.max = (int?)CheckMinMaxValue(value, this.min, max: true, positive: true);
		}
		
		public ExpressionProps(IExpressionProps props = null)
		{
			if (props is IAnnotatable a)
			{
				this.Annotation = a.Annotation;
			}
			if (props is IExpressionScopeProp sp)
			{
				this.Scope = sp.Scope;
			}
			if (props is IExpressionDefaultProp dp)
			{
				this.Default = dp.Default;
			}
			if (props is IExpressionAppearanceProps aps)
			{
				this.Min = aps.Min;
				this.Max = aps.Max;
			}
		}
		public ExpressionProps(XElement xml)
		{
			// TODO: type references
			this.Annotation = Utilities.TrimAnnotation(xml.GetValueNs(XmlConstants.Annotation));
			this.Scope = xml.GetAttributeValue(XmlConstants.Scope);
			this.Default = xml.GetAttributeValue(XmlConstants.Default);
			
			this.Min = xml.GetAttributeValue(XmlConstants.Min)?.ToInt32() ?? 0;
			this.Max = xml.GetAttributeValue(XmlConstants.Max) switch
			{
				null => 1,
				"*"  => null,

				string s => s.ToInt32()
			};
		}

		protected static decimal? CheckMinMaxValue(decimal? value, decimal? bound, bool max, bool positive = false)
		{
			string error = null;
			if (value.HasValue)
			{
				if (max)
				{
					if (value <= 0 && positive)
					{
						error = "Max <= 0";
					}
					else if (bound is decimal val && value < val)
					{
						error = "Max < Min";
					}
				}
				else
				{
					if (value < 0 && positive)
					{
						error = "Min < 0";
					}
					else if (bound is decimal val && value > val)
					{
						error = "Min > Max";
					}
				}
			}
			if (error != null)
			{
				throw new ArgumentOutOfRangeException(null, error);
			}
			
			return value;
		}
	}
}