namespace WebCompiler
{
	internal class LessDependencyResolver : SassDependencyResolver
	{
		public override string[] SearchPatterns => new string[1] { "*.less" };

		public override string FileExtension => ".less";
	}
}
