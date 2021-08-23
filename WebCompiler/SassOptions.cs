using Newtonsoft.Json;

namespace WebCompiler
{
	public class SassOptions : BaseOptions<SassOptions>
	{
		private const string trueStr = "true";

		protected override string CompilerFileName => "sass";

		[JsonProperty("autoPrefix")]
		public string AutoPrefix { get; set; } = "";


		[JsonProperty("includePath")]
		public string IncludePath { get; set; } = string.Empty;


		[JsonProperty("indentType")]
		public string IndentType { get; set; } = "space";


		[JsonProperty("indentWidth")]
		public int IndentWidth { get; set; } = 2;


		[JsonProperty("outputStyle")]
		public string OutputStyle { get; set; } = "nested";


		public int Precision { get; set; } = 5;


		[JsonProperty("relativeUrls")]
		public bool RelativeUrls { get; set; } = true;


		[JsonProperty("sourceMapRoot")]
		public string SourceMapRoot { get; set; } = string.Empty;


		[JsonProperty("lineFeed")]
		public string LineFeed { get; set; } = string.Empty;


		protected override void LoadSettings(Config config)
		{
			base.LoadSettings(config);
			string value = GetValue(config, "autoPrefix");
			if (value != null)
			{
				AutoPrefix = value;
			}
			if (config.Options.ContainsKey("outputStyle"))
			{
				OutputStyle = config.Options["outputStyle"].ToString();
			}
			if (config.Options.ContainsKey("indentType"))
			{
				IndentType = config.Options["indentType"].ToString();
			}
			int result = 5;
			if (int.TryParse(GetValue(config, "precision"), out result))
			{
				Precision = result;
			}
			int result2 = -1;
			if (int.TryParse(GetValue(config, "indentWidth"), out result2))
			{
				IndentWidth = result2;
			}
			string value2 = GetValue(config, "relativeUrls");
			if (value2 != null)
			{
				RelativeUrls = value2.ToLowerInvariant() == "true";
			}
			string value3 = GetValue(config, "includePath");
			if (value3 != null)
			{
				IncludePath = value3;
			}
			string value4 = GetValue(config, "sourceMapRoot");
			if (value4 != null)
			{
				SourceMapRoot = value4;
			}
			string value5 = GetValue(config, "lineFeed");
			if (value5 != null)
			{
				LineFeed = value5;
			}
		}
	}
}
