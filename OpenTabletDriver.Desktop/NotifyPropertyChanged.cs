using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenTabletDriver.Desktop
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaiseAndSetIfChanged<T>(ref T obj, T newValue, [CallerMemberName] string propertyName = "")
        {
            obj = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
