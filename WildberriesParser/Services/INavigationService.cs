using WildberriesParser.Infastructure.Core;

namespace WildberriesParser.Services
{
    public interface INavigationService
    {
        ViewModelBase CurrentView { get; }

        void NavigateTo<T>() where T : ViewModelBase;
    }
}