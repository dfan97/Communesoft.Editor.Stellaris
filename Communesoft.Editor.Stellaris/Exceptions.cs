using System.Collections;

namespace Communesoft.Editor.Stellaris
{
	public class MetadataParseException : Exception
	{
		public MetadataParseException(string message) : base(message)
		{
			
		}
	}
	
	/// <summary>
	/// TODO: реализовать свой интерфейс или как минимум ICollection Exception и KeyValuePair<int, MetadataParseErrors>
	/// </summary>
	public class MetadataParseErrors : IEnumerable<KeyValuePair<int, MetadataParseErrors>>
	{
		public Exception Error { get; private set; }
		private Dictionary<int, MetadataParseErrors> errors;

		/// <summary>
		/// Contained errors count
		/// </summary>
		public int Count => (this.Error == null ? 0 : 1) + (this.errors?.Count ?? 0);
			
		public MetadataParseErrors(Exception error = null)
		{
			if (error == null)
			{
				this.errors = new();
			}
			else
			{
				this.Error = error;
			}
		}

		public void Add(Exception error)
		{
			if (error != null)
			{
				this.Error = error;
			}
		}
		public void Add(int i, MetadataParseErrors errors)
		{
			if (errors != null)
			{
				this.errors.Add(i, errors);
			}
		}
		public bool Contains(int i) => this.errors?.ContainsKey(i) == true;
		public MetadataParseErrors Get(int i) => this.errors?[i];
		
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		public IEnumerator<KeyValuePair<int, MetadataParseErrors>> GetEnumerator()
		{
			if (this.errors != null)
			{
				foreach (var error in this.errors)
				{
					yield return error;
				}
			}
		}
	}
}