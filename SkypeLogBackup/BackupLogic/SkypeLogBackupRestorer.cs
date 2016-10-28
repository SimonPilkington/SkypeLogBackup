using SkypeLogBackup.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SkypeLogBackup.BackupLogic
{
	public class SkypeLogBackupRestorer : IDisposable
	{
		private readonly string _targetDirectory = SkypeHelper.SkypeAppDataPath;
		private readonly ZipArchive _backupFile;

		private const int PROGRES_REPORT_FILE_COUNT = 100;

		public string Username { get; }

		public SkypeLogBackupRestorer(string backupPath)
		{
			if (backupPath == null)
				throw new ArgumentNullException(nameof(backupPath));

			_backupFile = ZipFile.OpenRead(backupPath);
			Username = GetBackupUsername(_backupFile);

			if (Username == null)
				throw new SkypeLogBackupException("invalid backup file");
		}

		public async Task ExecuteAsync()
		{
			await ExecuteAsync(null);
		}

		public async Task ExecuteAsync(IProgress<uint> progressReport)
		{
			for (int i = 0; i < _backupFile.Entries.Count; i++)
			{
				var entry = _backupFile.Entries[i];

				if (IsCanary(entry.Name))
					continue;

				string entryFullPath = Path.Combine(_targetDirectory, entry.FullName);
				EnsureFileCanBeWritten(entryFullPath);

				using (var fileStream = File.Open(entryFullPath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					using (var entryStream = entry.Open())
						await entryStream.CopyToAsync(fileStream);
				}

				if (i % PROGRES_REPORT_FILE_COUNT == 0 || i == _backupFile.Entries.Count)
					progressReport?.Report(ProgressHelper.ComputeProgressPercentage((uint)i, (uint)_backupFile.Entries.Count));
			}
		}

		private static void EnsureFileCanBeWritten(string entryFullPath)
		{
			string entryDirectory = Path.GetDirectoryName(entryFullPath);

			if (File.Exists(entryFullPath))
				File.Delete(entryFullPath);
			else if (!Directory.Exists(entryDirectory))
				Directory.CreateDirectory(entryDirectory);
		}

		public void Dispose()
		{
			_backupFile.Dispose();
		}

		private static string GetBackupUsername(ZipArchive file)
		{
			var canaryEntry = file.Entries
				.Where(x => IsCanary(x.Name))
				.FirstOrDefault();

			if (canaryEntry == null)
				return null;

			return Path.GetFileNameWithoutExtension(canaryEntry.Name);
		}

		private static bool IsCanary(string file)
		{
			if (Path.GetExtension(file) == Properties.Settings.Default.BackupCanaryFileExtension)
				return true;

			return false;
		}
	}
}
