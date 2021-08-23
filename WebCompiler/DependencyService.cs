using System.Collections.Generic;
using System.IO;

namespace WebCompiler
{
	internal class DependencyService
	{
		private enum DependencyType
		{
			None,
			Sass,
			Less
		}

		private static Dictionary<DependencyType, DependencyResolverBase> _dependencies = new Dictionary<DependencyType, DependencyResolverBase>();

		public static Dictionary<string, Dependencies> GetDependencies(string projectRootPath, string sourceFile)
		{
			if (projectRootPath == null)
			{
				return null;
			}
			DependencyType dependencyType = GetDependencyType(sourceFile);
			if (!_dependencies.ContainsKey(dependencyType))
			{
				switch (dependencyType)
				{
				case DependencyType.Sass:
					_dependencies[dependencyType] = new SassDependencyResolver();
					break;
				case DependencyType.Less:
					_dependencies[dependencyType] = new LessDependencyResolver();
					break;
				}
			}
			if (_dependencies.ContainsKey(dependencyType))
			{
				_dependencies[dependencyType].UpdateFileDependencies(sourceFile);
				return _dependencies[dependencyType].GetDependencies(projectRootPath);
			}
			return null;
		}

		private static DependencyType GetDependencyType(string sourceFile)
		{
			switch (Path.GetExtension(sourceFile).ToUpperInvariant())
			{
			case ".LESS":
				return DependencyType.Less;
			case ".SCSS":
			case ".SASS":
				return DependencyType.Sass;
			case ".STYL":
			case ".STYLUS":
				return DependencyType.None;
			case ".COFFEE":
			case ".ICED":
				return DependencyType.None;
			case ".HBS":
			case ".HANDLEBARS":
				return DependencyType.None;
			case ".JS":
			case ".JSX":
			case ".ES6":
				return DependencyType.None;
			default:
				return DependencyType.None;
			}
		}
	}
}
