using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebCompiler
{
	public abstract class DependencyResolverBase
	{
		private Dictionary<string, Dependencies> _dependencies;

		protected Dictionary<string, Dependencies> Dependencies => _dependencies;

		public abstract string[] SearchPatterns { get; }

		public abstract string FileExtension { get; }

		public Dictionary<string, Dependencies> GetDependencies(string projectRootPath)
		{
			if (_dependencies == null)
			{
				_dependencies = new Dictionary<string, Dependencies>();
				List<string> list = new List<string>();
				string[] searchPatterns = SearchPatterns;
				foreach (string searchPattern in searchPatterns)
				{
					list.AddRange(Directory.GetFiles(projectRootPath, searchPattern, SearchOption.AllDirectories));
				}
				foreach (string item in list.Select((string p) => p.ToLowerInvariant()))
				{
					UpdateFileDependencies(item);
				}
			}
			return _dependencies;
		}

		public abstract void UpdateFileDependencies(string path);
	}
}
