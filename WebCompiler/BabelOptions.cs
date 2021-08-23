namespace WebCompiler
{
	public class BabelOptions : BaseOptions<BabelOptions>
	{
		protected override string CompilerFileName => "babel";

		protected override void LoadSettings(Config config)
		{
			base.LoadSettings(config);
		}
	}
}
