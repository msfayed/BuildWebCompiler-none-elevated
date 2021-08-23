using System;

namespace WebCompiler
{
	public class MinifyFileEventArgs : EventArgs
	{
		public bool ContainsChanges { get; set; }

		public string OriginalFile { get; set; }

		public string ResultFile { get; set; }

		public MinifyFileEventArgs(string originalFile, string resultFile, bool containsChanges)
		{
			ContainsChanges = containsChanges;
			OriginalFile = originalFile;
			ResultFile = resultFile;
		}
	}
}
