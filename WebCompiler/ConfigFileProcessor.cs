using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebCompiler
{
	public class ConfigFileProcessor
	{
		private static List<string> _processing = new List<string>();

		private static object _syncRoot = new object();

		public event EventHandler<CompileFileEventArgs> BeforeProcess;

		public event EventHandler<ConfigProcessedEventArgs> ConfigProcessed;

		public event EventHandler<CompileFileEventArgs> AfterProcess;

		public event EventHandler<SourceMapEventArgs> BeforeWritingSourceMap;

		public event EventHandler<SourceMapEventArgs> AfterWritingSourceMap;

		public IEnumerable<CompilerResult> Process(string configFile, IEnumerable<Config> configs = null, bool force = false)
		{
			if (_processing.Contains(configFile))
			{
				return Enumerable.Empty<CompilerResult>();
			}
			_processing.Add(configFile);
			List<CompilerResult> list = new List<CompilerResult>();
			try
			{
				FileInfo fileInfo = new FileInfo(configFile);
				configs = configs ?? ConfigHandler.GetConfigs(configFile);
				if (configs.Any())
				{
					OnConfigProcessed(configs.First(), 0, configs.Count());
				}
				foreach (Config config in configs)
				{
					if (force || config.CompilationRequired())
					{
						CompilerResult item = ProcessConfig(fileInfo.Directory.FullName, config);
						list.Add(item);
						OnConfigProcessed(config, list.Count, configs.Count());
					}
				}
				return list;
			}
			finally
			{
				if (_processing.Contains(configFile))
				{
					_processing.Remove(configFile);
				}
			}
		}

		public void DeleteOutputFiles(string configFile)
		{
			foreach (Config config in ConfigHandler.GetConfigs(configFile))
			{
				string fullName = config.GetAbsoluteOutputFile().FullName;
				string text = Path.ChangeExtension(fullName, ".min" + Path.GetExtension(fullName));
				string fileName = text + ".map";
				string fileName2 = text + ".gz";
				DeleteFile(fullName);
				DeleteFile(text);
				DeleteFile(fileName);
				DeleteFile(fileName2);
			}
		}

		private static void DeleteFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				FileHelpers.RemoveReadonlyFlagFromFile(fileName);
				File.Delete(fileName);
			}
		}

		public IEnumerable<CompilerResult> SourceFileChanged(string configFile, string sourceFile, string projectPath)
		{
			return SourceFileChanged(configFile, sourceFile, projectPath, new HashSet<string>());
		}

		private IEnumerable<CompilerResult> SourceFileChanged(string configFile, string sourceFile, string projectPath, HashSet<string> compiledFiles)
		{
			lock (_syncRoot)
			{
				string directoryName = Path.GetDirectoryName(configFile);
				List<CompilerResult> list = new List<CompilerResult>();
				IEnumerable<Config> configs = ConfigHandler.GetConfigs(configFile);
				foreach (Config item in configs)
				{
					string text = Path.Combine(directoryName, item.InputFile.Replace("/", "\\"));
					if (text.Equals(sourceFile, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(ProcessConfig(directoryName, item));
						compiledFiles.Add(text.ToLowerInvariant());
					}
				}
				Dictionary<string, Dependencies> dependencies = DependencyService.GetDependencies(projectPath, sourceFile);
				if (dependencies != null)
				{
					string key = sourceFile.ToLowerInvariant();
					if (dependencies.ContainsKey(key))
					{
						string[] array = dependencies[key].DependentFiles.ToArray();
						foreach (string text2 in array)
						{
							if (!compiledFiles.Contains(text2.ToLowerInvariant()))
							{
								list.AddRange(SourceFileChanged(configFile, text2, projectPath, compiledFiles));
							}
						}
					}
				}
				else if (list.Count == 0)
				{
					string extension = Path.GetExtension(sourceFile);
					foreach (Config item2 in configs)
					{
						if (Path.GetExtension(item2.InputFile).Equals(extension, StringComparison.OrdinalIgnoreCase))
						{
							list.Add(ProcessConfig(directoryName, item2));
						}
					}
				}
				return list;
			}
		}

		public static IEnumerable<Config> IsFileConfigured(string configFile, string sourceFile)
		{
			try
			{
				IEnumerable<Config> configs = ConfigHandler.GetConfigs(configFile);
				string directoryName = Path.GetDirectoryName(configFile);
				List<Config> list = new List<Config>();
				foreach (Config item in configs)
				{
					if (Path.Combine(directoryName, item.InputFile.Replace("/", "\\")).Equals(sourceFile, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(item);
					}
				}
				return list;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private CompilerResult ProcessConfig(string baseFolder, Config config)
		{
			CompilerResult compilerResult = CompilerService.GetCompiler(config).Compile(config);
			if (compilerResult.Errors.Any((CompilerError e) => !e.IsWarning))
			{
				return compilerResult;
			}
			if (Path.GetExtension(config.OutputFile).Equals(".css", StringComparison.OrdinalIgnoreCase) && AdjustRelativePaths(config))
			{
				compilerResult.CompiledContent = CssRelativePath.Adjust(compilerResult.CompiledContent, config);
			}
			config.Output = compilerResult.CompiledContent;
			FileInfo absoluteOutputFile = config.GetAbsoluteOutputFile();
			bool flag = FileHelpers.HasFileContentChanged(absoluteOutputFile.FullName, config.Output);
			OnBeforeProcess(config, baseFolder, flag);
			if (flag)
			{
				string directoryName = absoluteOutputFile.DirectoryName;
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.WriteAllText(absoluteOutputFile.FullName, config.Output, new UTF8Encoding(true));
			}
			OnAfterProcess(config, baseFolder, flag);
			FileMinifier.MinifyFile(config);
			if (!string.IsNullOrEmpty(compilerResult.SourceMap))
			{
				string fullName = config.GetAbsoluteOutputFile().FullName;
				string text = fullName + ".map";
				bool flag2 = FileHelpers.HasFileContentChanged(text, compilerResult.SourceMap);
				OnBeforeWritingSourceMap(fullName, text, flag2);
				if (flag2)
				{
					File.WriteAllText(text, compilerResult.SourceMap, new UTF8Encoding(true));
				}
				OnAfterWritingSourceMap(fullName, text, flag2);
			}
			return compilerResult;
		}

		private static bool AdjustRelativePaths(Config config)
		{
			if (!config.Options.ContainsKey("relativeUrls"))
			{
				return true;
			}
			return config.Options["relativeUrls"].ToString() == "True";
		}

		private void OnBeforeProcess(Config config, string baseFolder, bool containsChanges)
		{
			if (this.BeforeProcess != null)
			{
				this.BeforeProcess(this, new CompileFileEventArgs(config, baseFolder, containsChanges));
			}
		}

		private void OnConfigProcessed(Config config, int amountProcessed, int total)
		{
			if (this.ConfigProcessed != null)
			{
				this.ConfigProcessed(this, new ConfigProcessedEventArgs(config, amountProcessed, total));
			}
		}

		private void OnAfterProcess(Config config, string baseFolder, bool containsChanges)
		{
			if (this.AfterProcess != null)
			{
				this.AfterProcess(this, new CompileFileEventArgs(config, baseFolder, containsChanges));
			}
		}

		private void OnBeforeWritingSourceMap(string file, string mapFile, bool containsChanges)
		{
			if (this.BeforeWritingSourceMap != null)
			{
				this.BeforeWritingSourceMap(this, new SourceMapEventArgs(file, mapFile, containsChanges));
			}
		}

		private void OnAfterWritingSourceMap(string file, string mapFile, bool containsChanges)
		{
			if (this.AfterWritingSourceMap != null)
			{
				this.AfterWritingSourceMap(this, new SourceMapEventArgs(file, mapFile, containsChanges));
			}
		}
	}
}
