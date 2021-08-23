using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebCompiler
{
	public abstract class BaseMinifyOptions
	{
		protected static void LoadDefaultSettings(Config config, string minifierType)
		{
			string path = config.FileName + ".defaults";
			if (!File.Exists(path))
			{
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			JToken jToken = JObject.Parse(File.ReadAllText(path))["minifiers"]?[minifierType];
			if (jToken != null)
			{
				dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jToken.ToString());
			}
			if (dictionary == null)
			{
				return;
			}
			foreach (string key in dictionary.Keys)
			{
				if (!config.Minify.ContainsKey(key))
				{
					config.Minify[key] = dictionary[key];
				}
			}
		}

		protected static string GetValue(Config config, string key, object defaultValue = null)
		{
			if (config.Minify.ContainsKey(key))
			{
				return config.Minify[key].ToString();
			}
			if (defaultValue != null)
			{
				return defaultValue.ToString();
			}
			return string.Empty;
		}
	}
}
