using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WebCompiler
{
	public class Config
	{
		[JsonIgnore]
		public string FileName { get; set; }

		[JsonProperty("outputFile")]
		public string OutputFile { get; set; }

		[JsonProperty("inputFile")]
		public string InputFile { get; set; }

		[JsonProperty("minify")]
		public Dictionary<string, object> Minify { get; set; } = new Dictionary<string, object>();


		[DefaultValue(true)]
		[JsonProperty("includeInProject")]
		public bool IncludeInProject { get; set; } = true;


		[JsonProperty("sourceMap")]
		public bool SourceMap { get; set; }

		[JsonProperty("options")]
		public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();


		internal string Output { get; set; }

		public FileInfo GetAbsoluteInputFile()
		{
			return new FileInfo(Path.Combine(new FileInfo(FileName).DirectoryName, InputFile.Replace("/", "\\")));
		}

		public FileInfo GetAbsoluteOutputFile()
		{
			return new FileInfo(Path.Combine(new FileInfo(FileName).DirectoryName, OutputFile.Replace("/", "\\")));
		}

		public bool CompilationRequired()
		{
			FileInfo absoluteInputFile = GetAbsoluteInputFile();
			FileInfo absoluteOutputFile = GetAbsoluteOutputFile();
			if (!absoluteOutputFile.Exists)
			{
				return true;
			}
			if (absoluteInputFile.LastWriteTimeUtc > absoluteOutputFile.LastWriteTimeUtc)
			{
				return true;
			}
			return HasDependenciesNewerThanOutput(absoluteInputFile, absoluteOutputFile);
		}

		private bool HasDependenciesNewerThanOutput(FileInfo input, FileInfo output)
		{
			Dictionary<string, Dependencies> dependencies = DependencyService.GetDependencies(new FileInfo(FileName).DirectoryName, input.FullName);
			if (dependencies != null)
			{
				string key = input.FullName.ToLowerInvariant();
				return CheckForNewerDependenciesRecursively(key, dependencies, output);
			}
			return false;
		}

		private bool CheckForNewerDependenciesRecursively(string key, Dictionary<string, Dependencies> dependencies, FileInfo output, HashSet<string> checkedDependencies = null)
		{
			if (checkedDependencies == null)
			{
				checkedDependencies = new HashSet<string>();
			}
			checkedDependencies.Add(key);
			if (!dependencies.ContainsKey(key))
			{
				return false;
			}
			string[] array = dependencies[key].DependentOn.ToArray();
			foreach (string text in array)
			{
				if (checkedDependencies.Contains(text))
				{
					continue;
				}
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.Exists)
				{
					if (fileInfo.LastWriteTimeUtc > output.LastWriteTimeUtc)
					{
						return true;
					}
					if (CheckForNewerDependenciesRecursively(text, dependencies, output, checkedDependencies))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			if (obj == this)
			{
				return true;
			}
			Config config = (Config)obj;
			return GetHashCode() == config.GetHashCode();
		}

		public override int GetHashCode()
		{
			return OutputFile.GetHashCode();
		}

		public bool ShouldSerializeIncludeInProject()
		{
			Config config = new Config();
			return IncludeInProject != config.IncludeInProject;
		}

		public bool ShouldSerializeMinify()
		{
			Config config = new Config();
			return !DictionaryEqual(Minify, config.Minify, null);
		}

		public bool ShouldSerializeOptions()
		{
			Config config = new Config();
			return !DictionaryEqual(Options, config.Options, null);
		}

		private static bool DictionaryEqual<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> valueComparer)
		{
			if (first == second)
			{
				return true;
			}
			if (first == null || second == null)
			{
				return false;
			}
			if (first.Count != second.Count)
			{
				return false;
			}
			valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
			foreach (KeyValuePair<TKey, TValue> item in first)
			{
				TValue value;
				if (!second.TryGetValue(item.Key, out value))
				{
					return false;
				}
				if (!valueComparer.Equals(item.Value, value))
				{
					return false;
				}
			}
			return true;
		}
	}
}
