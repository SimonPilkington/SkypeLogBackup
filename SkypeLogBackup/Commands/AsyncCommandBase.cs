using System;
using System.Threading.Tasks;

namespace SkypeLogBackup.Commands
{
	public class BasicAsyncCommand : IAsyncCommand
	{
		private bool _enabled = true;
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					CanExecuteChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private Func<object, Task> _commandAction;
		private volatile bool _executing;

		public BasicAsyncCommand(Func<object, Task> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			_commandAction = action;
		}

		public bool CanExecute(object parameter)
		{
			return Enabled && !_executing;
		}

		public async void Execute(object parameter)
		{
			await ExecuteAsync(parameter);
		}

		public async Task ExecuteAsync(object parameter)
		{
			_executing = true;
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);

			await _commandAction(parameter);

			_executing = false;
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler CanExecuteChanged;
	}
}
