using Newtonsoft.Json;

namespace WebCompiler
{
	public class LessOptions : BaseOptions<LessOptions>
	{
		private const string trueStr = "true";

		protected override string CompilerFileName => "less";

		[JsonProperty("autoPrefix")]
		public string AutoPrefix { get; set; } = "";


		[JsonProperty("cssComb")]
		public string CssComb { get; set; } = "none";


		[JsonProperty("ieCompat")]
		public bool IECompat { get; set; } = true;


		[JsonProperty("math")]
		public string Math { get; set; }

		[JsonProperty("strictMath")]
		public bool StrictMath { get; set; }

		[JsonProperty("strictUnits")]
		public bool StrictUnits { get; set; }

		[JsonProperty("relativeUrls")]
		public bool RelativeUrls { get; set; } = true;


		[JsonProperty("rootPath")]
		public string RootPath { get; set; } = "";


		[JsonProperty("sourceMapRoot")]
		public string SourceMapRoot { get; set; } = string.Empty;


		[JsonProperty("sourceMapBasePath")]
		public string SourceMapBasePath { get; set; } = string.Empty;


		protected override void LoadSettings(Config config)
		{
			base.LoadSettings(config);
			string value = GetValue(config, "autoPrefix");
			if (value != null)
			{
				AutoPrefix = value;
			}
			string value2 = GetValue(config, "cssComb");
			if (value2 != null)
			{
				CssComb = value2;
			}
			string value3 = GetValue(config, "ieCompat");
			if (value3 != null)
			{
				IECompat = value3.ToLowerInvariant() == "true";
			}
			string value4 = GetValue(config, "math");
			if (value4 != null)
			{
				Math = value4;
			}
			string value5 = GetValue(config, "strictMath");
			if (value5 != null)
			{
				StrictMath = value5.ToLowerInvariant() == "true";
			}
			string value6 = GetValue(config, "strictUnits");
			if (value6 != null)
			{
				StrictUnits = value6.ToLowerInvariant() == "true";
			}
			string value7 = GetValue(config, "rootPath");
			if (value7 != null)
			{
				RootPath = value7;
			}
			string value8 = GetValue(config, "relativeUrls");
			if (value8 != null)
			{
				RelativeUrls = value8.ToLowerInvariant() == "true";
			}
			string value9 = GetValue(config, "sourceMapRoot");
			if (value9 != null)
			{
				SourceMapRoot = value9;
			}
			string value10 = GetValue(config, "sourceMapBasePath");
			if (value10 != null)
			{
				SourceMapBasePath = value10;
			}
		}
	}
}
