using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Admin
{
    internal class AdminMainViewModel : ViewModelWithWindowButtonsBase
    {
        private INavigationService _navigationService;

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public AdminMainViewModel(INavigationService navigationService)
        {
            WindowState = System.Windows.WindowState.Normal;
            NavigationService = navigationService;
            NavigationService.NavigateTo<UsersViewModel>();
        }

        private RelayCommand _usersCommand;

        public RelayCommand UsersCommand
        {
            get
            {
                return _usersCommand ??
                    (_usersCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<UsersViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _SettingsCommand;

        public RelayCommand SettingsCommand
        {
            get
            {
                return _SettingsCommand ??
                    (_SettingsCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<SettingsViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _HistoryCommand;

        public RelayCommand HistoryCommand
        {
            get
            {
                return _HistoryCommand ??
                    (_HistoryCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<HistoryViewModel>();
                    }
                    ));
            }
        }
    }
}