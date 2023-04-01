using System.Runtime.CompilerServices;

namespace WildberriesParser.Infastructure.Core
{
    public abstract class ViewModelBase : ObservableObject
    {
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string prop = "")
        {
            if (Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(prop);
            return true;
        }
    }
}