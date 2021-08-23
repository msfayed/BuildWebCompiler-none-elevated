using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebCompiler
{
	public abstract class BaseOptions<T> where T : BaseOptions<T>, new()
	{
		protected abstract string CompilerFileName { get; }

		[JsonProperty("sourceMap")]
		public bool SourceMap { get; set; }

		public static T FromConfig(Config config)
		{
			string path = config.FileName + ".defaults";
			T val = new T();
			if (File.Exists(path))
			{
				JToken jToken = JObject.Parse(File.ReadAllText(path))["compilers"][val.CompilerFileName];
				if (jToken != null)
				{
					val = JsonConvert.DeserializeObject<T>(jToken.ToString());
				}
			}
			val.LoadSettings(config);
			return val;
		}

		protected virtual void LoadSettings(Config config)
		{
			string value = GetValue(config, "sourceMap");
			if (value != null)
			{
				SourceMap = value.ToLowerInvariant() == "true";
			}
		}

		protected string GetValue(Config config, string key)
		{
			if (config.Options.ContainsKey(key))
			{
				return config.Options[key].ToString();
			}
			return null;
		}
	}
}
