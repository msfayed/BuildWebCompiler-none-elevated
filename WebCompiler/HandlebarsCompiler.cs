using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCompiler
{
	internal class HandlebarsCompiler : ICompiler
	{
		private static Regex _errorRx = new Regex("Error: (?<message>.+) on line (?<line>[0-9]+):", RegexOptions.Compiled);

		private string _mapPath;

		private string _path;

		private string _name = string.Empty;

		private string _extension = string.Empty;

		private string _output = string.Empty;

		private string _error = string.Empty;

		private bool _partial;

		public HandlebarsCompiler(string path)
		{
			_path = path;
		}

		public CompilerResult Compile(Config config)
		{
			string directoryName = Path.GetDirectoryName(config.FileName);
			string text = Path.Combine(directoryName, config.InputFile);
			FileInfo fileInfo = new FileInfo(text);
			string originalContent = File.ReadAllText(fileInfo.FullName);
			CompilerResult compilerResult = new CompilerResult
			{
				FileName = fileInfo.FullName,
				OriginalContent = originalContent
			};
			string extension = Path.GetExtension(text);
			if (!string.IsNullOrWhiteSpace(extension))
			{
				_extension = extension.Substring(1);
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			if (!string.IsNullOrWhiteSpace(fileNameWithoutExtension) && fileNameWithoutExtension.StartsWith("_"))
			{
				_name = fileNameWithoutExtension.Substring(1);
				_partial = true;
				string text2 = Path.Combine(Path.GetDirectoryName(text), _name + ".handlebarstemp");
				fileInfo.CopyTo(text2);
				fileInfo = new FileInfo(text2);
				_extension = "handlebarstemp";
			}
			_mapPath = Path.ChangeExtension(text, ".js.map.tmp");
			try
			{
				RunCompilerProcess(config, fileInfo);
				compilerResult.CompiledContent = _output;
				if ((BaseOptions<HandlebarsOptions>.FromConfig(config).SourceMap || config.SourceMap) && File.Exists(_mapPath))
				{
					compilerResult.SourceMap = File.ReadAllText(_mapPath);
				}
				if (_error.Length > 0)
				{
					CompilerError compilerError = new CompilerError
					{
						FileName = text,
						Message = _error.Replace(directoryName, string.Empty),
						IsWarning = !string.IsNullOrEmpty(_output)
					};
					Match match = _errorRx.Match(_error);
					if (match.Success)
					{
						compilerError.Message = match.Groups["message"].Value.Replace(directoryName, string.Empty);
						compilerError.LineNumber = int.Parse(match.Groups["line"].Value);
						compilerError.ColumnNumber = 0;
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
					FileName = text,
					Message = (string.IsNullOrEmpty(_error) ? ex.Message : _error),
					LineNumber = 0,
					ColumnNumber = 0
				};
				compilerResult.Errors.Add(item);
				return compilerResult;
			}
			finally
			{
				if (File.Exists(_mapPath))
				{
					File.Delete(_mapPath);
				}
				if (fileInfo.Extension == ".handlebarstemp")
				{
					fileInfo.Delete();
				}
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
			processStartInfo.Arguments = "/c \"\"" + Path.Combine(_path, "node_modules\\.bin\\handlebars.cmd") + "\" \"" + info.FullName + "\" " + text + "\"";
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

		private string ConstructArguments(Config config)
		{
			string text = "";
			HandlebarsOptions handlebarsOptions = BaseOptions<HandlebarsOptions>.FromConfig(config);
			if (handlebarsOptions.AMD)
			{
				text += " --amd";
			}
			else if (!string.IsNullOrEmpty(handlebarsOptions.CommonJS))
			{
				text = text + " --commonjs \"" + handlebarsOptions.CommonJS + "\"";
			}
			string[] knownHelpers = handlebarsOptions.KnownHelpers;
			foreach (string text2 in knownHelpers)
			{
				text = text + " --known \"" + text2 + "\"";
			}
			if (handlebarsOptions.KnownHelpersOnly)
			{
				text += " --knownOnly";
			}
			if (handlebarsOptions.ForcePartial || _partial)
			{
				text += " --partial";
			}
			if (handlebarsOptions.NoBOM)
			{
				text += " --bom";
			}
			if ((handlebarsOptions.SourceMap || config.SourceMap) && !string.IsNullOrWhiteSpace(_mapPath))
			{
				text = text + " --map \"" + _mapPath + "\"";
			}
			if (!string.IsNullOrEmpty(handlebarsOptions.TemplateNameSpace))
			{
				text = text + " --namespace \"" + handlebarsOptions.TemplateNameSpace + "\"";
			}
			if (!string.IsNullOrEmpty(handlebarsOptions.Root))
			{
				text = text + " --root \"" + handlebarsOptions.Root + "\"";
			}
			if (!string.IsNullOrEmpty(handlebarsOptions.Name))
			{
				text = text + " --name \"" + handlebarsOptions.Name + "\"";
			}
			else if (!string.IsNullOrEmpty(_name))
			{
				text = text + " --name \"" + _name + "\"";
			}
			if (!string.IsNullOrEmpty(_extension))
			{
				text = text + " --extension \"" + _extension + "\"";
			}
			return text;
		}
	}
}
