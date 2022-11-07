global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Linq;
global using Communesoft.Common.Utils;

using System.Windows;
using Communesoft.Editor.Stellaris.Data;

namespace Communesoft.Editor.Stellaris
{
	public partial class App : Application
	{
		public static DependencyProperty CreateDependencyProperty(string propertyName, PropertyMetadata metadata = null, ValidateValueCallback callback = null)
		{
			// Поднимаемся по стеку и получаем метод вызова, из которого определяем тип нужного объекта и возвращаемое значение нужного свойства
			StackFrame stack = new StackFrame(1);
			Type owner = stack.GetMethod().ReflectedType;
			Type prop = owner.GetProperty(propertyName).PropertyType;
			
			return DependencyProperty.Register(propertyName, prop, owner, metadata, callback);
		}

		public App()
		{
			// Load metadata
			Metadata.Initialize();
		}
	}
}