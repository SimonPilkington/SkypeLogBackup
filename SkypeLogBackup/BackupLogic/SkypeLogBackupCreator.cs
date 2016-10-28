using SkypeLogBackup.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace SkypeLogBackup.BackupLogic
{
	public class SkypeLogBackupCreator
	{
		private readonly string _targetDirectory;
		private readonly string _outputPath;

		private readonly Uri _targetDirectoryUri;

		private const string MAIN_DATABSE_PATH = "main.db";

		public SkypeLogBackupCreator(string username, string outputPath)
		{
			if (username == null)
				throw new ArgumentNullException(nameof(username));

			if (outputPath == null)
				throw new ArgumentNullException(nameof(outputPath));

			_targetDirectory = Path.Combine(SkypeHelper.SkypeAppDataPath, username);
			_targetDirectoryUri = new Uri(_targetDirectory, UriKind.Absolute);

			if (!File.Exists(Path.Combine(_targetDirectory, MAIN_DATABSE_PATH)))
				throw new ArgumentException("user does not exist", nameof(username));

			_outputPath = outputPath;
		}

		public async Task ExecuteAsync()
		{
			await ExecuteAsync(null);
		}

		public async Task ExecuteAsync(IProgress<uint> progressReport)
		{
			if (File.Exists(_outputPath))
				File.Delete(_outputPath);

			using (var backupZipArchive = ZipFile.Open(_outputPath, ZipArchiveMode.Create))
			{
				await AddAllFilesToArchive(backupZipArchive, _targetDirectory, progressReport).ConfigureAwait(false);

				string username = Path.GetFileName(_targetDirectory);
				backupZipArchive.CreateEntry($"{username}{Properties.Settings.Default.BackupCanaryFileExtension}");
			}
		}

		private async Task AddAllFilesToArchive(ZipArchive archive, string directoryFullPath, IProgress<uint> progressReport)
		{
			var subDirectories = new List<string>(Directory.EnumerateDirectories(directoryFullPath));

			for (int i = 0; i < subDirectories.Count; i++)
			{
				await AddAllFilesToArchive(archive, subDirectories[i], null).ConfigureAwait(false);

				progressReport?.Report(ProgressHelper.ComputeProgressPercentage((uint)i, (uint)subDirectories.Count + 1));
			}

			foreach (var file in Directory.EnumerateFiles(directoryFullPath))
			{
				if (IsDatabaseLock(file))
					continue;

				string entryPath = CreateRelativePath(file);
				var entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);

				using (var entryStream = entry.Open())
				{
					using (var fileStream = File.OpenRead(file))
						await fileStream.CopyToAsync(entryStream).ConfigureAwait(false);
				}
			}

			progressReport?.Report(100);
		}

		private string CreateRelativePath(string path)
		{
			var pathUri = new Uri(path, UriKind.Absolute);

			var result = _targetDirectoryUri.MakeRelativeUri(pathUri).ToString();
			return Uri.UnescapeDataString(result);
		}

		private bool IsDatabaseLock(string path) => Path.GetExtension(path) == ".lck";
	}
}
