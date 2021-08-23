namespace WebCompiler
{
	public class MinificationResult
	{
		public string MinifiedContent { get; set; }

		public string SourceMap { get; set; }

		public MinificationResult(string content, string sourceMap)
		{
			MinifiedContent = content;
			SourceMap = sourceMap;
		}
	}
}
