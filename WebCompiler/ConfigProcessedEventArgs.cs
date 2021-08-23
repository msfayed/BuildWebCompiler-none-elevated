using System;

namespace WebCompiler
{
	public class ConfigProcessedEventArgs : EventArgs
	{
		public Config Config { get; set; }

		public int AmountProcessed { get; set; }

		public int Total { get; set; }

		public ConfigProcessedEventArgs(Config config, int amountProcessed, int total)
		{
			Config = config;
			AmountProcessed = amountProcessed;
			Total = total;
		}
	}
}
