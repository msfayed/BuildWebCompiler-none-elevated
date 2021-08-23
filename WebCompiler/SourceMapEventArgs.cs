using System;

namespace WebCompiler
{
	public class SourceMapEventArgs : EventArgs
	{
		public bool ContainsChanges { get; set; }

		public string OriginalFile { get; set; }

		public string ResultFile { get; set; }

		public SourceMapEventArgs(string originalFile, string resultFile, bool containsChanges)
		{
			ContainsChanges = containsChanges;
			OriginalFile = originalFile;
			ResultFile = resultFile;
		}
	}
}
