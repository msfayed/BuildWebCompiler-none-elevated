namespace WebCompiler
{
	public class StylusOptions : BaseOptions<StylusOptions>
	{
		protected override string CompilerFileName => "stylus";

		protected override void LoadSettings(Config config)
		{
			base.LoadSettings(config);
		}
	}
}
