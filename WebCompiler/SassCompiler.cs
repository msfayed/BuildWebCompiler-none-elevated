using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebCompiler
{
	internal class SassCompiler : ICompiler
	{
		private static Regex _errorRx = new Regex("(?<message>.+) on line (?<line>[0-9]+), column (?<column>[0-9]+)", RegexOptions.Compiled);

		private string _path;

		private string _output = string.Empty;

		private string _error = string.Empty;

		public SassCompiler(string path)
		{
			_path = path;
		}

		public CompilerResult Compile(Config config)
		{
			FileInfo fileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(config.FileName), config.InputFile));
			string originalContent = File.ReadAllText(fileInfo.FullName);
			CompilerResult compilerResult = new CompilerResult
			{
				FileName = fileInfo.FullName,
				OriginalContent = originalContent
			};
			try
			{
				RunCompilerProcess(config, fileInfo);
				int num = _output.LastIndexOf("*/");
				if (num > -1 && _output.Contains("sourceMappingURL=data:"))
				{
					_output = _output.Substring(0, num + 2);
				}
				compilerResult.CompiledContent = _output;
				if (_error.Length > 0)
				{
					JObject jObject = JObject.Parse(_error);
					CompilerError item = new CompilerError
					{
						FileName = fileInfo.FullName,
						Message = jObject["message"].ToString(),
						ColumnNumber = int.Parse(jObject["column"].ToString()),
						LineNumber = int.Parse(jObject["line"].ToString()),
						IsWarning = !string.IsNullOrEmpty(_output)
					};
					compilerResult.Errors.Add(item);
					return compilerResult;
				}
				return compilerResult;
			}
			catch (Exception ex)
			{
				CompilerError item2 = new CompilerError
				{
					FileName = fileInfo.FullName,
					Message = (string.IsNullOrEmpty(_error) ? ex.Message : _error),
					LineNumber = 0,
					ColumnNumber = 0
				};
				compilerResult.Errors.Add(item2);
				return compilerResult;
			}
		}

		private void RunCompilerProcess(Config config, FileInfo info)
		{
			string text = ConstructArguments(config);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.WorkingDirectory = new FileInfo(config.FileName).DirectoryName;
			processStartInfo.UseShellExecute = false;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.FileName = "cmd.exe";
			processStartInfo.Arguments = "/c \"\"" + Path.Combine(_path, "node_modules\\.bin\\node-sass.cmd") + "\" " + text + " \"" + info.FullName + "\" \"";
			processStartInfo.StandardOutputEncoding = Encoding.UTF8;
			processStartInfo.StandardErrorEncoding = Encoding.UTF8;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			ProcessStartInfo processStartInfo2 = processStartInfo;
			SassOptions sassOptions = BaseOptions<SassOptions>.FromConfig(config);
			if (!string.IsNullOrEmpty(sassOptions.AutoPrefix))
			{
				string text2 = "--use autoprefixer";
				if (!sassOptions.SourceMap && !config.SourceMap)
				{
					text2 += " --no-map";
				}
				processStartInfo2.Arguments = processStartInfo2.Arguments.TrimEnd('"') + " | \"" + Path.Combine(_path, "node_modules\\.bin\\postcss.cmd") + "\" " + text2 + "\"";
				processStartInfo2.EnvironmentVariables.Add("BROWSERSLIST", sassOptions.AutoPrefix);
			}
			processStartInfo2.EnvironmentVariables["PATH"] = _path + ";" + processStartInfo2.EnvironmentVariables["PATH"];
			using (Process process = Process.Start(processStartInfo2))
			{
				Task<string> task = process.StandardOutput.ReadToEndAsync();
				Task<string> task2 = process.StandardError.ReadToEndAsync();
				process.WaitForExit();
				_output = task.Result;
				if (!task2.Result.StartsWith("√"))
				{
					_error = task2.Result;
				}
			}
		}

		private static string ConstructArguments(Config config)
		{
			string text = "";
			SassOptions sassOptions = BaseOptions<SassOptions>.FromConfig(config);
			if (sassOptions.SourceMap || config.SourceMap)
			{
				text += " --source-map-embed=true";
			}
			text = text + " --precision=" + sassOptions.Precision;
			if (!string.IsNullOrEmpty(sassOptions.OutputStyle))
			{
				text = text + " --output-style=" + sassOptions.OutputStyle;
			}
			if (!string.IsNullOrEmpty(sassOptions.IndentType))
			{
				text = text + " --indent-type=" + sassOptions.IndentType;
			}
			if (sassOptions.IndentWidth > -1)
			{
				text = text + " --indent-width=" + sassOptions.IndentWidth;
			}
			if (!string.IsNullOrEmpty(sassOptions.IncludePath))
			{
				text = text + " --include-path=" + sassOptions.IncludePath;
			}
			if (!string.IsNullOrEmpty(sassOptions.SourceMapRoot))
			{
				text = text + " --source-map-root=" + sassOptions.SourceMapRoot;
			}
			if (!string.IsNullOrEmpty(sassOptions.LineFeed))
			{
				text = text + " --linefeed=" + sassOptions.LineFeed;
			}
			return text;
		}
	}
}
