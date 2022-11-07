using System.Xml.Linq;
using Communesoft.Common.Utils.Xml;
using Communesoft.Editor.Stellaris.Utils;

namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Defines a metadata
	/// </summary>
	public interface IRelationExpressionMetadata : IMetadataContent
	{
		/// <summary>
		/// Represents possible relations
		/// </summary>
		RelationTypes Type { get; }
	}
	/// <summary>
	/// Represents an expression template for a <see cref="RelationTypes"/>
	/// </summary>
	public sealed class RelationExpressionMetadata : IRelationExpressionMetadata
	{
		public static RelationExpressionMetadata Parse(XElement xml)
		{
			return new RelationExpressionMetadata(RelationUtilities.ConvertToRelationType(xml.GetAttributeValue(XmlConstants.Value)));
		}
		
		/// <summary>
		/// Represents possible relations
		/// </summary>
		public RelationTypes Type { get; }
		
		private RelationExpressionMetadata(RelationTypes type)
		{
			this.Type = type;
		}
	}
}