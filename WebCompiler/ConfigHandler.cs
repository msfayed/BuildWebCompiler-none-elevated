using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace WebCompiler
{
	public class ConfigHandler
	{
		public void AddConfig(string fileName, Config config)
		{
			IEnumerable<Config> configs = GetConfigs(fileName);
			List<Config> list = new List<Config>();
			list.AddRange(configs);
			list.Add(config);
			config.FileName = fileName;
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};
			string contents = JsonConvert.SerializeObject(list, settings);
			File.WriteAllText(fileName, contents, new UTF8Encoding(true));
		}

		public void RemoveConfig(Config configToRemove)
		{
			IEnumerable<Config> configs = GetConfigs(configToRemove.FileName);
			List<Config> list = new List<Config>();
			if (configs.Contains(configToRemove))
			{
				list.AddRange(configs.Where((Config b) => !b.Equals(configToRemove)));
				string contents = JsonConvert.SerializeObject(list, Formatting.Indented);
				File.WriteAllText(configToRemove.FileName, contents);
			}
		}

		public void CreateDefaultsFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				string contents = JsonConvert.SerializeObject(new
				{
					compilers = new
					{
						less = new LessOptions(),
						sass = new SassOptions(),
						stylus = new StylusOptions(),
						babel = new BabelOptions(),
						coffeescript = new IcedCoffeeScriptOptions(),
						handlebars = new HandlebarsOptions()
					},
					minifiers = new
					{
						css = new
						{
							enabled = true,
							termSemicolons = true,
							gzip = false
						},
						javascript = new
						{
							enabled = true,
							termSemicolons = true,
							gzip = false
						}
					}
				}, Formatting.Indented);
				File.WriteAllText(fileName, contents);
			}
		}

		public static IEnumerable<Config> GetConfigs(string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			if (!fileInfo.Exists)
			{
				return Enumerable.Empty<Config>();
			}
			IEnumerable<Config> enumerable = JsonConvert.DeserializeObject<IEnumerable<Config>>(File.ReadAllText(fileName));
			Path.GetDirectoryName(fileInfo.FullName);
			foreach (Config item in enumerable)
			{
				item.FileName = fileName;
			}
			return enumerable;
		}
	}
}
