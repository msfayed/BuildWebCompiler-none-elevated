using Newtonsoft.Json;

namespace WebCompiler
{
	public class IcedCoffeeScriptOptions : BaseOptions<IcedCoffeeScriptOptions>
	{
		private const string trueStr = "true";

		protected override string CompilerFileName => "coffeescript";

		[JsonProperty("bare")]
		public bool Bare { get; set; }

		[JsonProperty("runtimeMode")]
		public string RuntimeMode { get; set; } = "node";


		protected override void LoadSettings(Config config)
		{
			base.LoadSettings(config);
			string value = GetValue(config, "bare");
			if (value != null)
			{
				Bare = value.ToLowerInvariant() == "true";
			}
			string value2 = GetValue(config, "runtimeMode");
			if (value2 != null)
			{
				RuntimeMode = value2.ToLowerInvariant();
			}
		}
	}
}
