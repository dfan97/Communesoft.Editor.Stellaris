namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Defines an annotation
	/// </summary>
	public interface IAnnotatable
	{
		/// <summary>
		/// The annotation
		/// </summary>
		string Annotation { get; }
	}
	
	/// <summary>
	/// Defines type possible to including into <see cref="IExpressionValue"/>
	/// </summary>
	public interface IExpressionContent { }
	/// <summary>
	/// Defines type possible to including into <see cref="SimpleExpressionValue"/>
	/// </summary>
	public interface ISimpleContent : IExpressionContent { }
	/// <summary>
	/// Defines type possible to including into <see cref="ListExpressionValue"/>
	/// </summary>
	public interface IListContent : IExpressionContent { }
	/// <summary>
	/// Defines type possible to including into <see cref="ComplexExpressionValue"/>
	/// </summary>
	public interface IComplexContent : IExpressionContent { }
}