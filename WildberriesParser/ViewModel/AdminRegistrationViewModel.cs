using System.Windows;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel
{
    public class AdminRegistrationViewModel : Infastructure.Core.ViewModelBase
    {
        private string _login;
        private string _password;
        private bool _isWorking = false;
        private string _repeatPassword;
        private ILoggerService _loggerService;
        private INavigationService _navigationService;

        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }

        public string Login
        {
            get => _login;
            set => Set(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        public string RepeatPassword
        {
            get => _repeatPassword;
            set => Set(ref _repeatPassword, value);
        }

        public AdminRegistrationViewModel(INavigationService navigationService, ILoggerService loggerService)
        {
            NavigationService = navigationService;
            _loggerService = loggerService;
        }

        private AsyncRelayCommand _createAdminCommand;

        public AsyncRelayCommand CreateAdminCommand
        {
            get
            {
                return _createAdminCommand ?? (_createAdminCommand = new AsyncRelayCommand
                    (
                        (obj) =>
                        {
                            IsWorking = true;
                            return App.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    User user = new User
                                    {
                                        Login = _login,
                                        Password = _password,
                                        RoleID = 1,
                                    };
                                    DBEntities.GetContext().User.Add(user);
                                    DBEntities.GetContext().SaveChanges();

                                    App.CurrentUser = user;

                                    _loggerService.AddLog(
                                        $"Создание аккаунта администратора. Password: {_password}, Login: {_login}",
                                        Model.LogTypeEnum.CREATE_USER);

                                    Window curr = obj as Window;
                                    IsWorking = false;
                                    curr.Hide();
                                    (App.ServiceProvider.GetService(typeof(View.Admin.AdminMainView)) as Window).Show();
                                    curr.Close();
                                }).Task;
                        },
                        (obj) => !string.IsNullOrEmpty(_login) && !string.IsNullOrEmpty(_password) && !string.IsNullOrEmpty(_repeatPassword)
                    ));
            }
        }

        private RelayCommand _settingDatabaseServerCommand;

        public RelayCommand SettingDatabaseServerCommand
        {
            get
            {
                return _settingDatabaseServerCommand ?? (_settingDatabaseServerCommand = new RelayCommand
                    (
                        (obj) =>
                        {
                            NavigationService.NavigateTo<SettingDatabaseServerViewModel>();
                        }
                    ));
            }
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }
    }
}