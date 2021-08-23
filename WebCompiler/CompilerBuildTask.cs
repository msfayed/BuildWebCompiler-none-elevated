using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WebCompiler
{
	public class CompilerBuildTask : Task
	{
		public string FileName { get; set; }

		public override bool Execute()
		{
			FileInfo fileInfo = new FileInfo(FileName);
			if (!fileInfo.Exists)
			{
				((Task)this).Log.LogWarning(fileInfo.FullName + " does not exist");
				return true;
			}
			((Task)this).Log.LogMessage((MessageImportance)0, Environment.NewLine + "WebCompiler: Begin compiling " + fileInfo.Name);
			ConfigFileProcessor configFileProcessor = new ConfigFileProcessor();
			configFileProcessor.BeforeProcess += delegate(object s, CompileFileEventArgs e)
			{
				if (e.ContainsChanges)
				{
					FileHelpers.RemoveReadonlyFlagFromFile(e.Config.GetAbsoluteOutputFile());
				}
			};
			configFileProcessor.AfterProcess += Processor_AfterProcess;
			configFileProcessor.BeforeWritingSourceMap += delegate(object s, SourceMapEventArgs e)
			{
				FileHelpers.RemoveReadonlyFlagFromFile(e.ResultFile);
			};
			configFileProcessor.AfterWritingSourceMap += Processor_AfterWritingSourceMap;
			FileMinifier.BeforeWritingMinFile += delegate(object s, MinifyFileEventArgs e)
			{
				if (e.ContainsChanges)
				{
					FileHelpers.RemoveReadonlyFlagFromFile(e.ResultFile);
				}
			};
			FileMinifier.AfterWritingMinFile += FileMinifier_AfterWritingMinFile;
			FileMinifier.BeforeWritingGzipFile += delegate(object s, MinifyFileEventArgs e)
			{
				if (e.ContainsChanges)
				{
					FileHelpers.RemoveReadonlyFlagFromFile(e.ResultFile);
				}
			};
			FileMinifier.AfterWritingGzipFile += FileMinifier_AfterWritingGzipFile;
			CompilerService.Initializing += delegate
			{
				((Task)this).Log.LogMessage((MessageImportance)0, "WebCompiler installing updated versions of the compilers...");
			};
			try
			{
				IEnumerable<CompilerResult> enumerable = configFileProcessor.Process(fileInfo.FullName);
				bool result = true;
				foreach (CompilerResult item in enumerable)
				{
					if (!item.HasErrors)
					{
						continue;
					}
					result = false;
					foreach (CompilerError error in item.Errors)
					{
						((Task)this).Log.LogError("WebCompiler", "0", "", error.FileName, error.LineNumber, error.ColumnNumber, error.LineNumber, error.ColumnNumber, error.Message, (object[])null);
					}
				}
				((Task)this).Log.LogMessage((MessageImportance)0, "WebCompiler: Done compiling " + fileInfo.Name);
				return result;
			}
			catch (Exception ex)
			{
				((Task)this).Log.LogError(ex.Message);
				return false;
			}
		}

		private void FileMinifier_AfterWritingGzipFile(object sender, MinifyFileEventArgs e)
		{
			((Task)this).Log.LogMessage((MessageImportance)0, "\tGzipped  " + FileHelpers.MakeRelative(FileName, e.ResultFile));
		}

		private void Processor_AfterProcess(object sender, CompileFileEventArgs e)
		{
			((Task)this).Log.LogMessage((MessageImportance)0, "\tCompiled " + e.Config.OutputFile);
		}

		private void Processor_AfterWritingSourceMap(object sender, SourceMapEventArgs e)
		{
			((Task)this).Log.LogMessage((MessageImportance)0, "\tSourceMap " + FileHelpers.MakeRelative(FileName, e.ResultFile));
		}

		private void FileMinifier_AfterWritingMinFile(object sender, MinifyFileEventArgs e)
		{
			((Task)this).Log.LogMessage((MessageImportance)0, "\tMinified " + FileHelpers.MakeRelative(FileName, e.ResultFile));
		}

		public CompilerBuildTask()
			: base()
		{
		}
	}
}
