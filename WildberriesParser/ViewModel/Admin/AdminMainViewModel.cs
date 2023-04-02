using System.Windows;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Admin
{
    internal class AdminMainViewModel : ViewModelWithWindowButtonsBase
    {
        public User CurrentUser
        {
            get => App.CurrentUser;
        }

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

        private RelayCommand _exitAccountCommand;

        public RelayCommand ExitAccountCommand
        {
            get
            {
                return _exitAccountCommand ??
                    (_exitAccountCommand = new RelayCommand
                    ((obj) =>
                    {
                        Window window = obj as Window;
                        window.Hide();
                        NavigationService.NavigateTo<AuthorizationViewModel>();
                        (App.ServiceProvider.GetService(typeof(View.StartView)) as Window).Show();
                        window.Close();
                    }
                    ));
            }
        }
    }
}