namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Defines metadata
	/// </summary>
	public interface IMetadata
	{
		
	}
	/// <summary>
	/// Defines a metadata structure
	/// </summary>
	public interface IMetadataStructure
	{
		/// <summary>
		/// The metadata structure
		/// </summary>
		IList<IMetadataContent> Content { get; }
	}
	
	/// <summary>
	/// Defines type possible to including into <see cref="ExpressionValueMetadataStructure{TProps}"/>
	/// </summary>
	public interface IMetadataContent { }
	/// <summary>
	/// Defines type possible to including into <see cref="ChoiceExpressionMetadata"/>
	/// </summary>
	[Obsolete("Replaced by IExpressionMetadata")]
	public interface IChoiceContent { }
}