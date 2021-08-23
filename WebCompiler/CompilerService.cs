using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace WebCompiler
{
	public static class CompilerService
	{
		internal const string Version = "1.12.405";

		private static readonly string _path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"temp");

		private static object _syncRoot = new object();

		public static readonly string[] AllowedExtensions = new string[11]
		{
			".LESS", ".SCSS", ".SASS", ".STYL", ".COFFEE", ".ICED", ".JS", ".JSX", ".ES6", ".HBS",
			".HANDLEBARS"
		};

		public static event EventHandler<EventArgs> Initializing;

		public static event EventHandler<EventArgs> Initialized;

		public static bool IsSupported(string inputFile)
		{
			string value = Path.GetExtension(inputFile).ToUpperInvariant();
			return AllowedExtensions.Contains(value);
		}

		internal static ICompiler GetCompiler(Config config)
		{
			string text = Path.GetExtension(config.InputFile).ToUpperInvariant();
			ICompiler result = null;
			Initialize();
			switch (text)
			{
			case ".LESS":
				result = new LessCompiler(_path);
				break;
			case ".HANDLEBARS":
			case ".HBS":
				result = new HandlebarsCompiler(_path);
				break;
			case ".SCSS":
			case ".SASS":
				result = new SassCompiler(_path);
				break;
			case ".STYL":
			case ".STYLUS":
				result = new StylusCompiler(_path);
				break;
			case ".COFFEE":
			case ".ICED":
				result = new IcedCoffeeScriptCompiler(_path);
				break;
			case ".JS":
			case ".JSX":
			case ".ES6":
				result = new BabelCompiler(_path);
				break;
			}
			return result;
		}

		public static void Initialize()
		{
			string path = Path.Combine(_path, "node_modules");
			string path2 = Path.Combine(_path, "node.exe");
			string path3 = Path.Combine(_path, "log.txt");
			lock (_syncRoot)
			{
				if (!Directory.Exists(path) || !File.Exists(path2) || !File.Exists(path3))
				{
					OnInitializing();
					if (Directory.Exists(_path))
					{
						Directory.Delete(_path, true);
					}
					Directory.CreateDirectory(_path);
					SaveResourceFile(_path, "Node.node_with_modules.zip", "node_with_modules.zip");
                    
                    ZipFile.ExtractToDirectory(Path.Combine(_path, "node_with_modules.zip"), _path);
                  
                    File.WriteAllText(path3, DateTime.Now.ToLongDateString());

                    OnInitialized();
				}
			}
		}

        private static void SaveResourceFile(string path, string resourceName, string fileName)
		{
			using (Stream stream = typeof(CompilerService).Assembly.GetManifestResourceStream(resourceName))
			{
				using (FileStream destination = new FileStream(Path.Combine(path, fileName), FileMode.Create))
				{
					stream.CopyTo(destination);
				}
			}
		}

		private static void OnInitializing()
		{
			if (CompilerService.Initializing != null)
			{
				CompilerService.Initializing(null, EventArgs.Empty);
			}
		}

		private static void OnInitialized()
		{
			if (CompilerService.Initialized != null)
			{
				CompilerService.Initialized(null, EventArgs.Empty);
			}
		}
	}
}
