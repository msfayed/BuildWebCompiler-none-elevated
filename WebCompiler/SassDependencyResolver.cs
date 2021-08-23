using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WebCompiler
{
	internal class SassDependencyResolver : DependencyResolverBase
	{
		public override string[] SearchPatterns => new string[2] { "*.scss", "*.sass" };

		public override string FileExtension => ".scss";

		public override void UpdateFileDependencies(string path)
		{
			if (base.Dependencies == null)
			{
				return;
			}
			FileInfo fileInfo = new FileInfo(path);
			path = fileInfo.FullName.ToLowerInvariant();
			if (!base.Dependencies.ContainsKey(path))
			{
				base.Dependencies[path] = new Dependencies();
			}
			base.Dependencies[path].DependentOn = new HashSet<string>();
			foreach (string key2 in base.Dependencies.Keys)
			{
				string key = key2.ToLowerInvariant();
				if (base.Dependencies[key].DependentFiles.Contains(path))
				{
					base.Dependencies[key].DependentFiles.Remove(path);
				}
			}
			foreach (Match item in Regex.Matches(File.ReadAllText(fileInfo.FullName), "(?<=@import(?:[\\s]+))(?:(?:\\(\\w+\\)))?\\s*(?:url)?(?<url>[^;]+)", RegexOptions.Multiline))
			{
				foreach (FileInfo fileInfo3 in GetFileInfos(fileInfo, item))
				{
					if (fileInfo3 == null)
					{
						continue;
					}
					FileInfo fileInfo2 = fileInfo3;
					if (string.Compare(fileInfo3.Extension, FileExtension, StringComparison.OrdinalIgnoreCase) != 0)
					{
						fileInfo2 = new FileInfo(fileInfo3.FullName + FileExtension);
					}
					string text = fileInfo2.FullName.ToLowerInvariant();
					if (!File.Exists(text))
					{
						string directoryName = Path.GetDirectoryName(text);
						string fileName = Path.GetFileName(text);
						string text2 = Path.Combine(directoryName, "_" + fileName);
						if (!File.Exists(text2))
						{
							continue;
						}
						text = text2.ToLowerInvariant();
					}
					if (!base.Dependencies[path].DependentOn.Contains(text))
					{
						base.Dependencies[path].DependentOn.Add(text);
					}
					if (!base.Dependencies.ContainsKey(text))
					{
						base.Dependencies[text] = new Dependencies();
					}
					if (!base.Dependencies[text].DependentFiles.Contains(path))
					{
						base.Dependencies[text].DependentFiles.Add(path);
					}
				}
			}
		}

		private static IEnumerable<FileInfo> GetFileInfos(FileInfo info, Match match)
		{
			string text = match.Groups["url"].Value.Replace("'", "\"").Replace("(", "").Replace(")", "")
				.Replace(";", "")
				.Trim();
			List<FileInfo> list = new List<FileInfo>();
			string[] array = text.Split(new string[1] { "\"," }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array)
			{
				try
				{
					string path = text2.Replace("\"", "").Replace("/", "\\").Trim();
					list.Add(new FileInfo(Path.Combine(info.DirectoryName, path)));
				}
				catch (Exception)
				{
				}
			}
			return list;
		}
	}
}
