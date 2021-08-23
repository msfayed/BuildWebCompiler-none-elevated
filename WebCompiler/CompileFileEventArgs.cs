using System;

namespace WebCompiler
{
	public class CompileFileEventArgs : EventArgs
	{
		public bool ContainsChanges { get; set; }

		public Config Config { get; set; }

		public string BaseFolder { get; set; }

		public CompileFileEventArgs(Config config, string baseFolder, bool containsChanges)
		{
			ContainsChanges = containsChanges;
			Config = config;
			BaseFolder = baseFolder;
		}
	}
}
