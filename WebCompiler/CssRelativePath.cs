using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WebCompiler
{
	internal static class CssRelativePath
	{
		private static readonly Regex _rxUrl = new Regex("url\\s*\\(\\s*([\"']?)([^:)]+)\\1\\s*\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static string Adjust(string content, Config config)
		{
			string text = content;
			string fullName = config.GetAbsoluteOutputFile().FullName;
			MatchCollection matchCollection = _rxUrl.Matches(text);
			if (matchCollection.Count > 0)
			{
				string directoryName = config.GetAbsoluteInputFile().DirectoryName;
				if (!Directory.Exists(directoryName))
				{
					return text;
				}
				{
					foreach (Match item in matchCollection)
					{
						string value = item.Groups[1].Value;
						string value2 = item.Groups[2].Value;
						if (value2.StartsWith("/", StringComparison.Ordinal))
						{
							continue;
						}
						string[] array = value2.Split(new char[1] { '?' }, 2, StringSplitOptions.RemoveEmptyEntries);
						string pathOnly = array[0];
						string text2 = ((array.Length == 2) ? array[1] : string.Empty);
						string absolutePath = GetAbsolutePath(directoryName, pathOnly);
						if (!string.IsNullOrEmpty(fullName) && !string.IsNullOrEmpty(absolutePath))
						{
							string text3 = FileHelpers.MakeRelative(fullName, absolutePath);
							if (!string.IsNullOrEmpty(text2))
							{
								text3 = text3 + "?" + text2;
							}
							string newValue = string.Format("url({0}{1}{0})", value, text3);
							text = text.Replace(item.Groups[0].Value, newValue);
						}
					}
					return text;
				}
			}
			return text;
		}

		private static string GetAbsolutePath(string cssFilePath, string pathOnly)
		{
			char[] invalidPathChars = Path.GetInvalidPathChars();
			foreach (char value in invalidPathChars)
			{
				if (pathOnly.IndexOf(value) > -1)
				{
					return null;
				}
			}
			return Path.GetFullPath(Path.Combine(cssFilePath, pathOnly));
		}
	}
}
