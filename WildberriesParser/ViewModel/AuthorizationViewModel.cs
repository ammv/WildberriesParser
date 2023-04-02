using System.Linq;
using System.Windows;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel
{
    internal class AuthorizationViewModel : ViewModelBase
    {
        private string _login;
        private string _password;
        private bool _isWorking = false;
        private bool _rememberMe = Properties.Settings.Default.rememberMe;
        private INavigationService _navigationService;

        public string Login
        {
            get => _login;
            set => Set(ref _login, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => Set(ref _rememberMe, value);
        }

        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        public AuthorizationViewModel(INavigationService navigationService)
        {
            if (RememberMe)
            {
                Login = Properties.Settings.Default.rememberLogin;
                Password = Properties.Settings.Default.rememberPassword;
            }
            _navigationService = navigationService;
        }

        public User GetUser()
        {
            return DBEntities.GetContext().User.FirstOrDefault(x => x.Login == _login);
        }

        private void SettingNavigationAfterAuth(User user)
        {
            Window window, curr = App.ServiceProvider.GetService(typeof(View.StartView)) as Window;
            if (user.Role.Name == "admin")
            {
                window = App.ServiceProvider.GetService(typeof(View.Admin.AdminMainView)) as Window;
            }
            else if (user.Role.Name == "staff")
            {
                window = App.ServiceProvider.GetService(typeof(View.Staff.StaffMainView)) as Window;
            }
            else
            {
                Helpers.MessageBoxHelper.Error("Функционал для пользователя такого типа ещё не реализован");
                IsWorking = false;
                return;
            }
            Properties.Settings.Default.rememberMe = _rememberMe;
            if (_rememberMe)
            {
                Properties.Settings.Default.rememberLogin = _login;
                Properties.Settings.Default.rememberPassword = _password;
            }
            App.CurrentUser = user;
            Properties.Settings.Default.Save();
            IsWorking = false;
            curr.Hide();
            window.Show();
            curr.Close();
        }

        private AsyncRelayCommand _authCommand;

        public AsyncRelayCommand AuthCommand
        {
            get
            {
                return _authCommand ?? (_authCommand = new AsyncRelayCommand
                    (
                        (obj) =>
                        {
                            IsWorking = true;

                            return App.Current.Dispatcher.InvokeAsync(() =>
                            {
                                User user = GetUser();
                                if (user != null && user.Login == _login)
                                {
                                    if (user.Password == _password)
                                    {
                                        SettingNavigationAfterAuth(user);
                                    }
                                    else
                                    {
                                        IsWorking = false;
                                        Helpers.MessageBoxHelper.Error("Неверный пароль");
                                    }
                                }
                                else
                                {
                                    IsWorking = false;
                                    Helpers.MessageBoxHelper.Error("Пользователь с таким логином не найден");
                                }
                            }).Task;
                        },
                        (obj) => !string.IsNullOrEmpty(_login) && !string.IsNullOrEmpty(_password)
                    ));
            }
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }
    }
}