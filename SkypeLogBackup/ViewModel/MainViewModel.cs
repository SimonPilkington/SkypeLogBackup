using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using SkypeLogBackup.Properties;
using SkypeLogBackup.Commands;
using SkypeLogBackup.Helpers;
using SkypeLogBackup.BackupLogic;

namespace SkypeLogBackup.ViewModel
{
	public sealed class MainViewModel : ViewModelBase
    {
		#region Properties
		private CollectionView _skypeUsers;
        public CollectionView SkypeUsers => _skypeUsers;

        private string _selectedUser;
        public string SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                }
            }
        }

		private bool _operationInProgress;
		public Visibility ShowProgressBar
		{
			get
			{
				return _operationInProgress ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public Visibility ShowUserSelectionComboBox
		{
			get
			{
				return _operationInProgress ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		private uint _operationProgress;
		public uint OperationProgress
		{
			get { return _operationProgress; }
			set
			{
				if (value != _operationProgress)
				{
					if (value > 100)
						throw new ArgumentOutOfRangeException(nameof(OperationProgress));

					_operationProgress = value;
					OnPropertyChanged();
				}
			}
		}
		#endregion

		#region Commands
		public BasicAsyncCommand BackupCommand { get; }
        public BasicAsyncCommand RestoreCommand { get; }
		#endregion

		public MainViewModel()
        {
			BackupCommand = new BasicAsyncCommand(BackupCommandAsyncFunc);
            RestoreCommand = new BasicAsyncCommand(RestoreCommandAsyncFunc);

            var userEnumerator = new UserEnumerator();
            _skypeUsers = new CollectionView(userEnumerator.GetUsers());
        }

		private async Task BackupCommandAsyncFunc(object _)
		{
			if (CheckSkypeRunning())
				return;

			var saveDialog = new SaveFileDialog()
			{
				Filter = Settings.Default.ZipFileFilter,
				FileName = SelectedUser
			};

			if(saveDialog.ShowDialog().Value)
			{
				SetOperationInProgress(true);

				try
				{
					var backupCreator = new SkypeLogBackupCreator(SelectedUser, saveDialog.FileName);
					var progressReport = new Progress<uint>(i => OperationProgress = i);

					await backupCreator.ExecuteAsync(progressReport);
				}
				catch (SkypeLogBackupException)
				{
					MessageBox.Show(Resources.LogsCorrupted, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
				}
				finally
				{
					SetOperationInProgress(false);
				}
			}
		}

        private async Task RestoreCommandAsyncFunc(object _)
        {
			if (CheckSkypeRunning())
				return;

			var openDialog = new OpenFileDialog()
			{
				Filter = Settings.Default.ZipFileFilter
			};

			if (openDialog.ShowDialog().Value)
			{
				SetOperationInProgress(true);

				try
				{
					using (var backupRestorer = new SkypeLogBackupRestorer(openDialog.FileName))
					{
						if (!EnsureUserAgreesToRestore(backupRestorer.Username))
							return;

						var progressReport = new Progress<uint>(i => OperationProgress = i);
						await backupRestorer.ExecuteAsync(progressReport);
					}
				}
				catch (SkypeLogBackupException)
				{
					MessageBox.Show(Resources.InvalidBackup, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
				}
				finally
				{
					SetOperationInProgress(false);
				}
			}
		}

		private bool CheckSkypeRunning()
		{
			if (SkypeHelper.SkypeProcessExists)
			{
				MessageBox.Show(Resources.TurnSkypeOff, Resources.ThisIsAMessageBox, MessageBoxButton.OK, MessageBoxImage.Asterisk);
				return true;
			}

			return false;
		}

		private bool EnsureUserAgreesToRestore(string usernameBeingRestored)
		{
			var result = MessageBox.Show(string.Format(Resources.RestoreWarning, usernameBeingRestored), Resources.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (result == MessageBoxResult.Yes)
				return true;

			return false;
		}

		private void SetOperationInProgress(bool value)
		{
			if (!value)
				OperationProgress = 0;

			_operationInProgress = value;
			OnPropertyChanged(nameof(ShowProgressBar));
			OnPropertyChanged(nameof(ShowUserSelectionComboBox));
		}
    }
}
