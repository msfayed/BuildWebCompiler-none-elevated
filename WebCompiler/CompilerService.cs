using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace WebCompiler
{
    public static class CompilerService
    {
        internal const string Version = "1.12.405";

        private static readonly string _path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "temp");

        // used Mutex for interprocess synchronization
        private static Mutex _syncRoot = new Mutex(false, "89A99E44-E217-4ED7-8003-7EA9C682EDD2");

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

            _syncRoot.WaitOne();
            try
            {
                if (!LongDirectory.Exists(path) || !LongFile.Exists(path2) || !LongFile.Exists(path3))
                {
                    OnInitializing();
                    if (LongDirectory.Exists(_path))
                    {
                         LongDirectory.Delete(_path, true);
                    }

                    LongDirectory.CreateDirectory(_path);
                    SaveResourceFile(_path, "Node.node_with_modules.zip", "node_with_modules.zip");

                    var zipFilePath = Path.Combine(_path, "node_with_modules.zip");

                    using (var archive = ZipFile.OpenRead(zipFilePath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var fullEntryPath = Path.Combine(_path, entry.FullName.Replace("/","\\"));

                            var dirPath = Path.GetDirectoryName(fullEntryPath);
                            if (!LongDirectory.Exists(dirPath))
                            {
                                LongDirectory.CreateDirectory(dirPath);
                            }

                            if (Path.GetFileName(entry.FullName).Length != 0)
                            {
                                using (var fileHandle = LongFile.CreateFileForWrite(fullEntryPath))
                                {
                                    using (var destination = new FileStream(fileHandle, FileAccess.Write))
                                    {
                                        using (Stream stream = entry.Open())
                                        {
                                            stream.CopyTo(destination);
                                        }
                                    }
                                }

                            }

                        }
                    }

                    File.WriteAllText(path3, DateTime.Now.ToLongDateString());

                    OnInitialized();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                _syncRoot.ReleaseMutex();
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
