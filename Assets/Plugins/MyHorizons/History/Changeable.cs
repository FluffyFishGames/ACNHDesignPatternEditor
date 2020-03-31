using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyHorizons.History
{
    public abstract class Changeable : Cloneable, INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T obj, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(obj, value)) return false;

            obj = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}