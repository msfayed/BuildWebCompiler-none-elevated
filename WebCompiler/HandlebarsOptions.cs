using System.Linq;
using Newtonsoft.Json;

namespace WebCompiler
{
	public class HandlebarsOptions : BaseOptions<HandlebarsOptions>
	{
		private const string trueStr = "true";

		protected override string CompilerFileName => "hbs";

		[JsonProperty("root")]
		public string Root { get; set; } = "";


		[JsonProperty("noBOM")]
		public bool NoBOM { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; } = "";


		[JsonProperty("namespace")]
		public string TemplateNameSpace { get; set; } = "";


		[JsonProperty("knownHelpersOnly")]
		public bool KnownHelpersOnly { get; set; }

		[JsonProperty("forcePartial")]
		public bool ForcePartial { get; set; }

		[JsonProperty("knownHelpers")]
		public string[] KnownHelpers { get; set; } = new string[0];


		[JsonProperty("commonjs")]
		public string CommonJS { get; set; } = "";


		[JsonProperty("amd")]
		public bool AMD { get; set; }

		protected override void LoadSettings(Config config)
		{
			base.LoadSettings(config);
			string value = GetValue(config, "name");
			if (value != null)
			{
				Name = value;
			}
			string value2 = GetValue(config, "namespace");
			if (value2 != null)
			{
				TemplateNameSpace = value2;
			}
			string value3 = GetValue(config, "root");
			if (value3 != null)
			{
				Root = value3;
			}
			string value4 = GetValue(config, "commonjs");
			if (value4 != null)
			{
				CommonJS = value4;
			}
			string value5 = GetValue(config, "amd");
			if (value5 != null)
			{
				AMD = value5.ToLowerInvariant() == "true";
			}
			string value6 = GetValue(config, "forcePartial");
			if (value6 != null)
			{
				ForcePartial = value6.ToLowerInvariant() == "true";
			}
			string value7 = GetValue(config, "noBOM");
			if (value7 != null)
			{
				NoBOM = value7.ToLowerInvariant() == "true";
			}
			string value8 = GetValue(config, "knownHelpersOnly");
			if (value8 != null)
			{
				KnownHelpersOnly = value8.ToLowerInvariant() == "true";
			}
			string value9 = GetValue(config, "knownHelpers");
			if (value9 != null)
			{
				KnownHelpers = (from s in value9.Split(',')
					where !string.IsNullOrWhiteSpace(s)
					select s.Trim()).ToArray();
			}
		}
	}
}
