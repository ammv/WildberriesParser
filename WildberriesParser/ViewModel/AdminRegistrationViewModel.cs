using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
                            return Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                IsWorking = true;
                                try
                                {
                                    RegistrationAdmin(obj);
                                }
                                catch (System.Exception ex)
                                {
                                    Helpers.MessageBoxHelper.Error(ex.Message);
                                    Helpers.MessageBoxHelper.Error(ex.InnerException.Message);
                                    WriteEfErrosAndOpen(ex);
                                }
                                IsWorking = false;
                            }).Task;
                        },
                        (obj) => !string.IsNullOrEmpty(_login) && !string.IsNullOrEmpty(_password) && !string.IsNullOrEmpty(_repeatPassword) && !IsWorking
                    ));
            }
        }

        private static void WriteEfErrosAndOpen(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            var errorsEntities = DBEntities.GetContext().GetValidationErrors().ToList();
            foreach (var errorEntity in errorsEntities)
            {
                sb.Append($"Сущность: {errorEntity.Entry.Entity.GetType().Name}\n");
                sb.Append($"Ошибки: \n");
                foreach (var item in errorEntity.ValidationErrors)
                {
                    sb.Append($"\t - {item.ErrorMessage}\n");
                }
            }
            //List<string>
            File.WriteAllText("ef_erros.txt",
                sb.ToString());

            Helpers.MessageBoxHelper.Error(ex.Message);
            if (Helpers.MessageBoxHelper.Question("Ошибки записаны в ef_erros.txt. Открыть?") == Helpers.MessageBoxHelperResult.YES)
            {
                Process.Start("ef_erros.txt");
            }
        }

        private void RegistrationAdmin(object obj)
        {
            User user = new User
            {
                Login = _login,
                Password = _password,
                RoleID = 1,
                CanAuth = true
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