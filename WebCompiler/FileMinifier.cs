using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using NUglify;
using NUglify.Css;
using NUglify.JavaScript;

namespace WebCompiler
{
	public class FileMinifier
	{
		public static event EventHandler<MinifyFileEventArgs> BeforeWritingMinFile;

		public static event EventHandler<MinifyFileEventArgs> AfterWritingMinFile;

		public static event EventHandler<MinifyFileEventArgs> BeforeWritingGzipFile;

		public static event EventHandler<MinifyFileEventArgs> AfterWritingGzipFile;

		internal static MinificationResult MinifyFile(Config config)
		{
			FileInfo absoluteOutputFile = config.GetAbsoluteOutputFile();
			switch (absoluteOutputFile.Extension.ToUpperInvariant())
			{
			case ".JS":
				return MinifyJavaScript(config, absoluteOutputFile.FullName);
			case ".CSS":
				return MinifyCss(config, absoluteOutputFile.FullName);
			default:
				return null;
			}
		}

		private static MinificationResult MinifyJavaScript(Config config, string file)
		{
			string source = File.ReadAllText(file);
			CodeSettings settings = JavaScriptOptions.GetSettings(config);
			if (config.Minify.ContainsKey("enabled") && config.Minify["enabled"].ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			string minFileName = GetMinFileName(file);
			string code = Uglify.Js(source, settings).Code;
			bool flag = FileHelpers.HasFileContentChanged(minFileName, code);
			if (!string.IsNullOrEmpty(code))
			{
				OnBeforeWritingMinFile(file, minFileName, flag);
				if (flag)
				{
					File.WriteAllText(minFileName, code, new UTF8Encoding(true));
				}
				OnAfterWritingMinFile(file, minFileName, flag);
				GzipFile(config, minFileName, flag);
			}
			return new MinificationResult(code, null);
		}

		private static MinificationResult MinifyCss(Config config, string file)
		{
			string input = File.ReadAllText(file);
			CssSettings settings = CssOptions.GetSettings(config);
			if (config.Minify.ContainsKey("enabled") && config.Minify["enabled"].ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			input = Regex.Replace(input, "[\\u0000-\\u0009\\u000B-\\u000C\\u000E-\\u001F]", string.Empty);
			string code = Uglify.Css(input, settings).Code;
			string minFileName = GetMinFileName(file);
			bool flag = FileHelpers.HasFileContentChanged(minFileName, code);
			OnBeforeWritingMinFile(file, minFileName, flag);
			if (flag)
			{
				File.WriteAllText(minFileName, code, new UTF8Encoding(true));
			}
			OnAfterWritingMinFile(file, minFileName, flag);
			GzipFile(config, minFileName, flag);
			return new MinificationResult(code, null);
		}

		private static string GetMinFileName(string file)
		{
			string extension = Path.GetExtension(file);
			string text = file.Substring(0, file.LastIndexOf(extension));
			if (!text.EndsWith(".min"))
			{
				text += ".min";
			}
			return text + extension;
		}

		private static void GzipFile(Config config, string sourceFile, bool containsChanges)
		{
			if (!config.Minify.ContainsKey("gzip") || !config.Minify["gzip"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			string text = sourceFile + ".gz";
			OnBeforeWritingGzipFile(sourceFile, text, containsChanges);
			if (containsChanges)
			{
				using (FileStream fileStream = File.OpenRead(sourceFile))
				{
					using (FileStream stream = File.OpenWrite(text))
					{
						using (GZipStream destination = new GZipStream(stream, CompressionMode.Compress))
						{
							fileStream.CopyTo(destination);
						}
					}
				}
			}
			OnAfterWritingGzipFile(sourceFile, text, containsChanges);
		}

		private static void OnBeforeWritingMinFile(string file, string minFile, bool containsChanges)
		{
			if (FileMinifier.BeforeWritingMinFile != null)
			{
				FileMinifier.BeforeWritingMinFile(null, new MinifyFileEventArgs(file, minFile, containsChanges));
			}
		}

		private static void OnAfterWritingMinFile(string file, string minFile, bool containsChanges)
		{
			if (FileMinifier.AfterWritingMinFile != null)
			{
				FileMinifier.AfterWritingMinFile(null, new MinifyFileEventArgs(file, minFile, containsChanges));
			}
		}

		private static void OnBeforeWritingGzipFile(string minFile, string gzipFile, bool containsChanges)
		{
			if (FileMinifier.BeforeWritingGzipFile != null)
			{
				FileMinifier.BeforeWritingGzipFile(null, new MinifyFileEventArgs(minFile, gzipFile, containsChanges));
			}
		}

		private static void OnAfterWritingGzipFile(string minFile, string gzipFile, bool containsChanges)
		{
			if (FileMinifier.AfterWritingGzipFile != null)
			{
				FileMinifier.AfterWritingGzipFile(null, new MinifyFileEventArgs(minFile, gzipFile, containsChanges));
			}
		}
	}
}
