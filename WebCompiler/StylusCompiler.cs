using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCompiler
{
	internal class StylusCompiler : ICompiler
	{
		private static Regex _errorRx = new Regex("(?<severity>Error|Warning): (?<file>.+):(?<line>[0-9]+):(?<column>[0-9]+).+^(?=[^\\s])(?<message>([^\\r\\n]+))", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline);

		private string _path;

		private string _output = string.Empty;

		private string _error = string.Empty;

		public StylusCompiler(string path)
		{
			_path = path;
		}

		public CompilerResult Compile(Config config)
		{
			string directoryName = Path.GetDirectoryName(config.FileName);
			FileInfo fileInfo = new FileInfo(Path.Combine(directoryName, config.InputFile));
			string originalContent = File.ReadAllText(fileInfo.FullName);
			CompilerResult compilerResult = new CompilerResult
			{
				FileName = fileInfo.FullName,
				OriginalContent = originalContent
			};
			try
			{
				RunCompilerProcess(config, fileInfo);
				compilerResult.CompiledContent = _output;
				if (_error.Length > 0)
				{
					CompilerError compilerError = new CompilerError
					{
						FileName = fileInfo.FullName,
						Message = _error.Replace(directoryName, string.Empty),
						IsWarning = !string.IsNullOrEmpty(_output)
					};
					Match match = _errorRx.Match(_error);
					if (match.Success)
					{
						compilerError.Message = match.Groups["message"].Value.Replace(directoryName, string.Empty);
						compilerError.LineNumber = int.Parse(match.Groups["line"].Value);
						compilerError.ColumnNumber = int.Parse(match.Groups["column"].Value);
					}
					compilerResult.Errors.Add(compilerError);
					return compilerResult;
				}
				return compilerResult;
			}
			catch (Exception ex)
			{
				CompilerError item = new CompilerError
				{
					FileName = fileInfo.FullName,
					Message = (string.IsNullOrEmpty(_error) ? ex.Message : _error),
					LineNumber = 0,
					ColumnNumber = 0
				};
				compilerResult.Errors.Add(item);
				return compilerResult;
			}
		}

		private void RunCompilerProcess(Config config, FileInfo info)
		{
			string text = ConstructArguments(config);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.WorkingDirectory = info.Directory.FullName;
			processStartInfo.UseShellExecute = false;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.FileName = "cmd.exe";
			processStartInfo.Arguments = "/c \"\"" + Path.Combine(_path, "node_modules\\.bin\\stylus.cmd") + "\" " + text + " \"" + info.FullName + "\"\"";
			processStartInfo.StandardOutputEncoding = Encoding.UTF8;
			processStartInfo.StandardErrorEncoding = Encoding.UTF8;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			ProcessStartInfo processStartInfo2 = processStartInfo;
			processStartInfo2.EnvironmentVariables["PATH"] = _path + ";" + processStartInfo2.EnvironmentVariables["PATH"];
			using (Process process = Process.Start(processStartInfo2))
			{
				Task<string> task = process.StandardOutput.ReadToEndAsync();
				Task<string> task2 = process.StandardError.ReadToEndAsync();
				process.WaitForExit();
				_output = task.Result.Trim();
				_error = task2.Result.Trim();
			}
		}

		private static string ConstructArguments(Config config)
		{
			string text = " --print";
			if (BaseOptions<StylusOptions>.FromConfig(config).SourceMap || config.SourceMap)
			{
				text += " --sourcemap-inline";
			}
			return text;
		}
	}
}
