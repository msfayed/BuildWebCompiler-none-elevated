using System.Collections.Generic;

namespace WebCompiler
{
	public class Dependencies
	{
		public HashSet<string> DependentOn { get; set; } = new HashSet<string>();


		public HashSet<string> DependentFiles { get; set; } = new HashSet<string>();

	}
}
