using System.ComponentModel;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;
public class MainWindowModel : ViewModelBase, INotifyPropertyChanged
{
    public virtual async Task ShowBusyPopup(string? message) { }
    public virtual async Task ShowNoBusyPopup() { }

}
