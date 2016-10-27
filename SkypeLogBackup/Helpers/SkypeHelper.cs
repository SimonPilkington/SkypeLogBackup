using System;
using System.Diagnostics;
using System.IO;

namespace SkypeLogBackup.Helpers
{
	public static class SkypeHelper
	{
		public static string AppDataPath { get; }
		public static string SkypeAppDataPath { get; }

		static SkypeHelper()
		{
			AppDataPath = Environment.GetEnvironmentVariable("appdata");
			SkypeAppDataPath = Path.Combine(AppDataPath, "Skype");
		}

		public static bool SkypeProcessExists => Process.GetProcessesByName("skype").Length != 0;
	}
}
