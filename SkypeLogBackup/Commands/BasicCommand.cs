using System;
using System.Windows.Input;

namespace SkypeLogBackup.Commands
{
	public class BasicCommand : ICommand
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

		private readonly Action<object> _commandAction;

		public BasicCommand(Action<object> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			_commandAction = action;
		}

		public bool CanExecute(object parameter)
		{
			return _enabled;
		}

		public void Execute(object parameter)
		{
			_commandAction(parameter);
		}

		public event EventHandler CanExecuteChanged;
	}
}
