using NUglify;
using NUglify.Css;

namespace WebCompiler
{
	public class CssOptions : BaseMinifyOptions
	{
		public static CssSettings GetSettings(Config config)
		{
			BaseMinifyOptions.LoadDefaultSettings(config, "css");
			CssSettings cssSettings = new CssSettings();
			cssSettings.TermSemicolons = BaseMinifyOptions.GetValue(config, "termSemicolons") == "True";
			switch (BaseMinifyOptions.GetValue(config, "commentMode"))
			{
			case "hacks":
				cssSettings.CommentMode = CssComment.Hacks;
				break;
			case "important":
				cssSettings.CommentMode = CssComment.Important;
				break;
			case "none":
				cssSettings.CommentMode = CssComment.None;
				break;
			case "all":
				cssSettings.CommentMode = CssComment.All;
				break;
			}
			switch (BaseMinifyOptions.GetValue(config, "colorNames"))
			{
			case "hex":
				cssSettings.ColorNames = CssColor.Hex;
				break;
			case "major":
				cssSettings.ColorNames = CssColor.Major;
				break;
			case "noSwap":
				cssSettings.ColorNames = CssColor.NoSwap;
				break;
			case "strict":
				cssSettings.ColorNames = CssColor.Strict;
				break;
			}
			switch (BaseMinifyOptions.GetValue(config, "outputMode", "singleLine"))
			{
			case "multipleLines":
				cssSettings.OutputMode = OutputMode.MultipleLines;
				break;
			case "singleLine":
				cssSettings.OutputMode = OutputMode.SingleLine;
				break;
			case "none":
				cssSettings.OutputMode = OutputMode.None;
				break;
			}
			int result;
			if (int.TryParse(BaseMinifyOptions.GetValue(config, "indentSize", 2), out result))
			{
				cssSettings.IndentSize = result;
			}
			return cssSettings;
		}
	}
}
