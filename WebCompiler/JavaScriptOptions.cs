using System;
using NUglify;
using NUglify.JavaScript;

namespace WebCompiler
{
	public class JavaScriptOptions : BaseMinifyOptions
	{
		public static CodeSettings GetSettings(Config config)
		{
			BaseMinifyOptions.LoadDefaultSettings(config, "javascript");
			CodeSettings codeSettings = new CodeSettings();
			codeSettings.PreserveImportantComments = BaseMinifyOptions.GetValue(config, "preserveImportantComments", true) == "True";
			codeSettings.TermSemicolons = BaseMinifyOptions.GetValue(config, "termSemicolons", true) == "True";
			if (BaseMinifyOptions.GetValue(config, "renameLocals", true) == "False")
			{
				codeSettings.LocalRenaming = LocalRenaming.KeepAll;
			}
			string value = BaseMinifyOptions.GetValue(config, "evalTreatment", "ignore");
			if (value.Equals("ignore", StringComparison.OrdinalIgnoreCase))
			{
				codeSettings.EvalTreatment = EvalTreatment.Ignore;
			}
			else if (value.Equals("makeAllSafe", StringComparison.OrdinalIgnoreCase))
			{
				codeSettings.EvalTreatment = EvalTreatment.MakeAllSafe;
			}
			else if (value.Equals("makeImmediateSafe", StringComparison.OrdinalIgnoreCase))
			{
				codeSettings.EvalTreatment = EvalTreatment.MakeImmediateSafe;
			}
			string value2 = BaseMinifyOptions.GetValue(config, "outputMode", "singleLine");
			if (value2.Equals("multipleLines", StringComparison.OrdinalIgnoreCase))
			{
				codeSettings.OutputMode = OutputMode.MultipleLines;
			}
			else if (value2.Equals("singleLine", StringComparison.OrdinalIgnoreCase))
			{
				codeSettings.OutputMode = OutputMode.SingleLine;
			}
			else if (value2.Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				codeSettings.OutputMode = OutputMode.None;
			}
			int result;
			if (int.TryParse(BaseMinifyOptions.GetValue(config, "indentSize", 2), out result))
			{
				codeSettings.IndentSize = result;
			}
			return codeSettings;
		}
	}
}
