using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebCompiler
{
	internal class Program
	{
		private static int Main(params string[] args)
		{
			string text = args[0];
			string file = ((args.Length > 1) ? args[1] : null);
			IEnumerable<Config> configs = GetConfigs(text, file);
			if (configs == null)
			{
				Console.WriteLine("\u001b[33mNo configurations matched");
				return 0;
			}
			ConfigFileProcessor configFileProcessor = new ConfigFileProcessor();
			EventHookups(configFileProcessor, text);
			IEnumerable<CompilerResult> enumerable = from r in configFileProcessor.Process(text, configs)
				where r.HasErrors
				select r;
			foreach (CompilerResult item in enumerable)
			{
				foreach (CompilerError error in item.Errors)
				{
					Console.Write("\u001b[31m" + error.Message);
				}
			}
			if (!enumerable.Any())
			{
				return 0;
			}
			return 1;
		}

		private static void EventHookups(ConfigFileProcessor processor, string configPath)
		{
			processor.BeforeProcess += delegate(object s, CompileFileEventArgs e)
			{
				Console.WriteLine("Processing \u001b[36m" + e.Config.InputFile);
				if (e.ContainsChanges)
				{
					FileHelpers.RemoveReadonlyFlagFromFile(e.Config.GetAbsoluteOutputFile());
				}
			};
			processor.AfterProcess += delegate
			{
				Console.WriteLine("  \u001b[32mCompiled");
			};
			processor.BeforeWritingSourceMap += delegate(object s, SourceMapEventArgs e)
			{
				if (e.ContainsChanges)
				{
					FileHelpers.RemoveReadonlyFlagFromFile(e.ResultFile);
				}
			};
			processor.AfterWritingSourceMap += delegate
			{
				Console.WriteLine("  \u001b[32mSourcemap");
			};
			processor.ConfigProcessed += delegate
			{
				Console.WriteLine("\t");
			};
			FileMinifier.BeforeWritingMinFile += delegate(object s, MinifyFileEventArgs e)
			{
				FileHelpers.RemoveReadonlyFlagFromFile(e.ResultFile);
			};
			FileMinifier.AfterWritingMinFile += delegate
			{
				Console.WriteLine("  \u001b[32mMinified");
			};
			FileMinifier.BeforeWritingGzipFile += delegate(object s, MinifyFileEventArgs e)
			{
				FileHelpers.RemoveReadonlyFlagFromFile(e.ResultFile);
			};
			FileMinifier.AfterWritingGzipFile += delegate
			{
				Console.WriteLine("  \u001b[32mGZipped");
			};
		}

		private static IEnumerable<Config> GetConfigs(string configPath, string file)
		{
			IEnumerable<Config> enumerable = ConfigHandler.GetConfigs(configPath);
			if (enumerable == null || !enumerable.Any())
			{
				return null;
			}
			if (file != null)
			{
				enumerable = ((!file.StartsWith("*")) ? enumerable.Where((Config c) => c.InputFile.Equals(file, StringComparison.OrdinalIgnoreCase)) : enumerable.Where((Config c) => Path.GetExtension(c.InputFile).Equals(file.Substring(1), StringComparison.OrdinalIgnoreCase)));
			}
			return enumerable;
		}
	}
}
