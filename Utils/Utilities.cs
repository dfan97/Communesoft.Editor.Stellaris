using System.Text.RegularExpressions;

namespace Communesoft.Editor.Stellaris.Utils
{
	public static class Utilities
	{
		/// <summary>
		/// Trims the annotation
		/// </summary>
		public static string TrimAnnotation(string annotation)
		{
			if (annotation.IsNullOrWhiteSpace())
			{
				return null;
			}
			
			int min = int.MaxValue;
			string[] lines = annotation.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines)
			{
				Match match = Regex.Match(line, "^[\\s]*");

				min = Math.Min(min, match.Value.Length);
			}
			return string.Join(Environment.NewLine, lines.Select(l => l[min..]));
		}
	}
}