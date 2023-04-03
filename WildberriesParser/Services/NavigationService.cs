using System;
using WildberriesParser.Infastructure.Core;

namespace WildberriesParser.Services
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private ViewModelBase _currentView;
        private readonly Func<Type, ViewModelBase> _viewModelFactory;

        public ViewModelBase CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModelBase>() where TViewModelBase : ViewModelBase
        {
            var vm = _viewModelFactory.Invoke(typeof(TViewModelBase));
            CurrentView = vm;
        }
    }
}