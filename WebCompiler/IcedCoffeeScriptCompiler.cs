using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCompiler
{
	internal class IcedCoffeeScriptCompiler : ICompiler
	{
		private static Regex _errorRx = new Regex(":(?<line>[0-9]+):(?<column>[0-9]+).*error: (?<message>.+)", RegexOptions.Compiled);

		private string _path;

		private string _error = string.Empty;

		private string _temp = Path.Combine(Path.GetTempPath(), ".iced-coffee-script");

		public IcedCoffeeScriptCompiler(string path)
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
			string text = Path.ChangeExtension(Path.Combine(_temp, fileInfo.Name), ".js");
			string path = text + ".map";
			try
			{
				RunCompilerProcess(config, fileInfo);
				if (File.Exists(text))
				{
					compilerResult.CompiledContent = File.ReadAllText(text);
					if ((BaseOptions<IcedCoffeeScriptOptions>.FromConfig(config).SourceMap || config.SourceMap) && File.Exists(path))
					{
						compilerResult.SourceMap = File.ReadAllText(path);
					}
				}
				if (_error.Length > 0)
				{
					CompilerError compilerError = new CompilerError
					{
						FileName = fileInfo.FullName,
						Message = _error.Replace(directoryName, string.Empty)
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
			finally
			{
				File.Delete(text);
				File.Delete(path);
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
			processStartInfo.Arguments = "/c \"\"" + Path.Combine(_path, "node_modules\\.bin\\iced.cmd") + "\" " + text + " \"" + info.FullName + "\"\"";
			processStartInfo.StandardErrorEncoding = Encoding.UTF8;
			processStartInfo.RedirectStandardError = true;
			ProcessStartInfo processStartInfo2 = processStartInfo;
			processStartInfo2.EnvironmentVariables["PATH"] = _path + ";" + processStartInfo2.EnvironmentVariables["PATH"];
			using (Process process = Process.Start(processStartInfo2))
			{
				Task<string> task = process.StandardError.ReadToEndAsync();
				process.WaitForExit();
				_error = task.Result;
			}
		}

		private string ConstructArguments(Config config)
		{
			string text = " --compile --output \"" + _temp + "\"";
			IcedCoffeeScriptOptions icedCoffeeScriptOptions = BaseOptions<IcedCoffeeScriptOptions>.FromConfig(config);
			if (icedCoffeeScriptOptions.SourceMap || config.SourceMap)
			{
				text += " --map";
			}
			if (icedCoffeeScriptOptions.Bare)
			{
				text += " --bare";
			}
			if (!string.IsNullOrEmpty(icedCoffeeScriptOptions.RuntimeMode))
			{
				text = text + " --runtime " + icedCoffeeScriptOptions.RuntimeMode;
			}
			return text;
		}
	}
}
