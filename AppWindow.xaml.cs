using System.Windows;
using Communesoft.Editor.Stellaris.Data;

namespace Communesoft.Editor.Stellaris
{
	public partial class AppWindow : Window
	{
		public AppWindow()
		{
			this.InitializeComponent();
		}
	}
	
	public class AppWindowData : DependencyObject
	{
		private static readonly DependencyProperty PropertyTypeProperty = App.CreateDependencyProperty(nameof(Qwe));
		public IEnumerable<string> Qwe
		{
			get => (IEnumerable<string>)GetValue(PropertyTypeProperty);
			set => SetValue(PropertyTypeProperty, value);
		}
		
		private static readonly DependencyProperty PropertyTypeProperty2 = App.CreateDependencyProperty(nameof(Qwe2));
		public string Qwe2
		{
			get => (string)GetValue(PropertyTypeProperty2);
			set => SetValue(PropertyTypeProperty2, value);
		}

		// TODO: в параметр с возможностью выбора корневой папки и папки, где лежат .mod файлы, для их парсинга и получения доступа по путям
		/// <summary>
		/// Path to stellaris root directory
		/// </summary>
		private static readonly string stellarisPath = @"S:\0_0\Stellaris";
		/// <summary>
		/// Path to stellaris user data directory (mods, settings etc)
		/// </summary>
		private static readonly string stellarisModsPath = @"C:\Users\VIKTORK\Documents\Paradox Interactive\Stellaris";

		public AppWindowData()
		{
			//var sample = Deserializer.DeserializeSample();
			
			var root = Deserializer.Deserialize(stellarisPath);
		}
	}
}