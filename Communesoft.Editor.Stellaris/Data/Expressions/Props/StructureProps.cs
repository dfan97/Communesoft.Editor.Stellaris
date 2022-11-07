using System.Xml.Linq;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Represents <see cref="ExpressionFileStructure"/> properties
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay}")]
	public class ExpressionFileProps : ExpressionProps, IExpressionPathProp
	{
		#if DEBUG

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay
		{
			get
			{
				string path = this.Path.IsNullOrWhiteSpace() ? null : $"\"{this.Path}\"";
				string ann = this.Annotation.GetValueOrDefault();

				return path == null
					? ann
					: ann == null
						? path
						: $"{path} - {ann}";
			}
		}

		#endif

		/// <summary>
		/// The structure directory path
		/// </summary>
		public string Path { get; init; }

		public ExpressionFileProps(ExpressionProps props = null) : base(props)
		{
			if (props is IExpressionPathProp pp)
			{
				this.Path = pp.Path;
			}
		}
		public ExpressionFileProps(XElement xml) : base(xml)
		{
			this.Path = xml.GetAttributeValue(XmlConstants.Path);
			if (this.Path.IsNullOrWhiteSpace())
			{
				throw new MetadataParseException($"Doesn't contain '{XmlConstants.Path}' attribute");
			}
		}
	}
	
	/// <summary>
	/// Represents <see cref="ExpressionType"/> properties
	/// </summary>
	public class ExpressionTypeProps : ExpressionProps, IExpressionShortProp
	{
		/// <summary>
		/// An internal expression entity, which grants uniqueness to this
		/// </summary>
		public string Definition { get; init; }
		/// <summary>
		/// The expression shorthand
		/// </summary>
		public ExpressionTypeReference Short { get; init; }

		public ExpressionTypeProps(ExpressionProps props = null) : base(props)
		{
			if (props is ExpressionTypeProps tp)
			{
				this.Definition = tp.Definition;
			}
			if (props is IExpressionShortProp sp)
			{
				this.Short = sp.Short;
			}
		}
		public ExpressionTypeProps(XElement xml) : base(xml)
		{
			this.Definition = xml.GetAttributeValue("definition");
			this.Short = ExpressionTypeReference.Find(xml.GetAttributeValue(XmlConstants.Short));
		}
	}
}