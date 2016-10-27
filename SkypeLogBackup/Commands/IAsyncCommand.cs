using System.Threading.Tasks;
using System.Windows.Input;

namespace SkypeLogBackup.Commands
{
	public interface IAsyncCommand : ICommand
	{
		Task ExecuteAsync(object parameter);
	}
}
