using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WebCompiler
{
	public class CompilerCleanTask : Task
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
			((Task)this).Log.LogMessage((MessageImportance)0, Environment.NewLine + "WebCompiler: Begin cleaning output of " + fileInfo.Name);
			try
			{
				new ConfigFileProcessor().DeleteOutputFiles(fileInfo.FullName);
				((Task)this).Log.LogMessage((MessageImportance)0, "WebCompiler: Done cleaning output of " + fileInfo.Name);
				return true;
			}
			catch (Exception ex)
			{
				((Task)this).Log.LogError(ex.Message);
				return false;
			}
		}

		public CompilerCleanTask()
			: base()
		{
		}
	}
}
