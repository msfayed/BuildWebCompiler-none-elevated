using System;
using System.IO;

namespace WebCompiler
{
	public static class FileHelpers
	{
		public static string MakeRelative(string baseFile, string file)
		{
			Uri uri = new Uri(baseFile, UriKind.RelativeOrAbsolute);
			Uri uri2 = new Uri(file, UriKind.RelativeOrAbsolute);
			return Uri.UnescapeDataString(uri.MakeRelativeUri(uri2).ToString());
		}

		public static void RemoveReadonlyFlagFromFile(string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			if (fileInfo.Exists && fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
		}

		public static void RemoveReadonlyFlagFromFile(FileInfo file)
		{
			RemoveReadonlyFlagFromFile(file.FullName);
		}

		public static bool HasFileContentChanged(string fileName, string newContent)
		{
			if (!File.Exists(fileName))
			{
				return true;
			}
			return File.ReadAllText(fileName) != newContent;
		}
	}
}
