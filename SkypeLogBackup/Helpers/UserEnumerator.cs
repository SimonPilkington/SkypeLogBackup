using System.Collections.Generic;
using System.IO;

namespace SkypeLogBackup.Helpers
{
	public class UserEnumerator
	{
		public UserEnumerator()
		{
			if (!Directory.Exists(SkypeHelper.SkypeAppDataPath))
				throw new SkypeLogBackupException("skype not installed (properly?)");
		}

		public IEnumerable<string> GetUsers()
		{
			var subDirectories = Directory.EnumerateDirectories(SkypeHelper.SkypeAppDataPath);

			foreach (var directory in subDirectories)
			{
				var mainDbPath = Path.Combine(directory, "main.db");

				if (File.Exists(mainDbPath))
					yield return Path.GetFileNameWithoutExtension(directory);
			}
		}
	}
}
