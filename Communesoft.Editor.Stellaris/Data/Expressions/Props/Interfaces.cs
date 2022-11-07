namespace Communesoft.Editor.Stellaris.Data
{
	/// <summary>
	/// Defines the expression metadata properties
	/// </summary>
	public interface IExpressionProps
	{
		
	}
	
	/// <summary>
	/// Defines the expression scopes
	/// </summary>
	public interface IExpressionScopeProp
	{
		/// <summary>
		/// Scope types, where the expression can appears
		/// </summary>
		string Scope { get; init; }
	}
	/// <summary>
	/// Defines the expression default value
	/// </summary>
	public interface IExpressionDefaultProp
	{
		/// <summary>
		/// A default value for the expression
		/// </summary>
		string Default { get; init; }
	}
	
	/// <summary>
	/// Defines the expression appearance count
	/// </summary>
	public interface IExpressionAppearanceProps
	{
		/// <summary>
		/// Minimum count of appearing
		/// </summary>
		int Min { get; init; }
		/// <summary>
		/// Maximum count of appearing
		/// </summary>
		int? Max { get; init; }
	}
	/// <summary>
	/// Defines the expression internal raw numeric value limits
	/// </summary>
	public interface IExpressionRawValueLimitsProps
	{
		/// <summary>
		/// Minimum for numeric value
		/// </summary>
		decimal? MinValue { get; init; }
		/// <summary>
		/// Maximum for numeric value
		/// </summary>
		decimal? MaxValue { get; init; }
	}
	/// <summary>
	/// Defines the expression internal raw value count
	/// </summary>
	public interface IExpressionRawValueCountProps
	{
		/// <summary>
		/// Minimum list elements count
		/// </summary>
		int? MinValueCount { get; init; }
		/// <summary>
		/// Maximum list elements count
		/// </summary>
		int? MaxValueCount { get; init; }
	}
	
	/// <summary>
	/// Defines the expression internal values restriction or extension
	/// </summary>
	public interface IExpressionEnumsProp
	{
		/// <summary>
		/// Enumeration values restrict or extend the expression
		/// </summary>
		EnumExpressionValues Enums { get; init; }
	}
	
	/// <summary>
	/// Defines the expression structure directory path
	/// </summary>
	public interface IExpressionPathProp
	{
		/// <summary>
		/// The expression structure directory path
		/// </summary>
		string Path { get; init; }
	}
	/// <summary>
	/// Defines the expression shorthand
	/// </summary>
	public interface IExpressionShortProp
	{
		/// <summary>
		/// The expression shorthand
		/// </summary>
		ExpressionTypeReference Short { get; init; }
	}
}