using System;
using System.Linq;
using System.Threading.Tasks;
using WildberriesParser.Helpers;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel
{
    public class SettingDatabaseServerViewModel : Infastructure.Core.ViewModelBase
    {
        private string _login = Properties.Settings.Default.login;
        private string _password = Properties.Settings.Default.password;
        private string _server = Properties.Settings.Default.server;
        private string _databaseName = Properties.Settings.Default.database;
        private string _checkState = "Проверить подключение";
        private bool _isConnected = false;
        private bool _canCheck = false;
        private INavigationService _navigationService;
        private ILoggerService _loggerService;

        public string Login
        {
            get => _login;
            set
            {
                if (Set(ref _login, value))
                {
                    IsConnected = false;
                    CheckState = "Проверить подключение";
                }
            }
        }

        public bool CanCheck
        {
            get => _canCheck;
            set
            {
                bool result = !string.IsNullOrEmpty(Server) && !string.IsNullOrEmpty(DatabaseName);
                Set(ref _canCheck, result);
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (Set(ref _password, value))
                {
                    IsConnected = false;
                    CheckState = "Проверить подключение";
                }
            }
        }

        public string Server
        {
            get => _server;
            set
            {
                if (Set(ref _server, value))
                {
                    IsConnected = false;
                    CheckState = "Проверить подключение";
                }
            }
        }

        public string DatabaseName
        {
            get => _databaseName;
            set
            {
                if (Set(ref _databaseName, value))
                {
                    IsConnected = false;
                    CheckState = "Проверить подключение";
                }
            }
        }

        public SettingDatabaseServerViewModel(INavigationService navigationService, ILoggerService loggerService)
        {
            NavigationService = navigationService;
            _loggerService = loggerService;
        }

        private bool HasUsers()
        {
            return DBEntities.GetContext().User.Count() > 0;
        }

        private AsyncRelayCommand _CompleteSettingCommand;

        public AsyncRelayCommand CompleteSettingCommand
        {
            get
            {
                return _CompleteSettingCommand ?? (_CompleteSettingCommand = new AsyncRelayCommand
                    (
                        (obj) =>
                        {
                            return App.Current.Dispatcher.InvokeAsync(async () =>
                                {
                                    if (_isConnected)
                                    {
                                        if (!HasUsers())
                                        {
                                            NavigationService.NavigateTo<AdminRegistrationViewModel>();
                                        }
                                        else
                                        {
                                            NavigationService.NavigateTo<AuthorizationViewModel>();
                                        }

                                        return;
                                    }

                                    CheckState = "Проверка подключения";

                                    string conn = DBHelper.CreateConnectionString(_server, _databaseName, _login, _password);
                                    string connWithoutDB = DBHelper.CreateConnectionString(_server, string.Empty, _login, _password);

                                    string efconn = DBHelper.CreateEFConnectionString(conn);

                                    CheckResult result = await DBHelper.ServerIsAvailableSql(connWithoutDB);

                                    if (result.Result)
                                    {
                                        DBEntities.SetContext(efconn);
                                        bool databaseExists = DBEntities.GetContext().Database.Exists();
                                        if (!databaseExists)
                                        {
                                            if (Helpers.MessageBoxHelperResult.YES == MessageBoxHelper.Question($"База данных \"{_databaseName}\" отсутствует на сервере.\n" +
                                                $"Без базы данных программа работать не может. Создать ее прямо сейчас?"))
                                            {
                                                DBEntities.GetContext().Database.Create();
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        CheckState = "Проверить подключение";
                                        MessageBoxHelper.Error(result.Exception.Message);
                                        return;
                                    }

                                    Properties.Settings.Default.ConnectionString = efconn;
                                    Properties.Settings.Default.login = _login;
                                    Properties.Settings.Default.password = _password;
                                    Properties.Settings.Default.server = _server;
                                    Properties.Settings.Default.database = _databaseName;
                                    Properties.Settings.Default.Save();
                                    IsConnected = true;
                                    CheckState = "Готово";

                                    _loggerService.AddLog($"Соеденение настроено с БД настроено:\n" +
                                        $"Логин: {_login}" +
                                        $"Пароль: {_password}\n" +
                                        $"Сервер: {_server}\n" +
                                        $"База данных: {_databaseName}", Model.LogTypeEnum.CHANGE_DB_SETTINGS);
                                }).Task;
                        },
                        (obj) =>
                                !string.IsNullOrEmpty(Server) && !string.IsNullOrEmpty(DatabaseName)
                    ));
            }
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public string CheckState
        {
            get => _checkState;
            set => Set(ref _checkState, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => Set(ref _isConnected, value);
        }
    }
}