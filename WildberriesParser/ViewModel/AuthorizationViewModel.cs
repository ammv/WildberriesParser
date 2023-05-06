using System.Linq;
using System;
using System.Windows;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using DataLayer;
using WildberriesParser.Services;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

namespace WildberriesParser.ViewModel
{
    internal class AuthorizationViewModel : ViewModelBase
    {
        private string _login;
        private string _password;
        private Window _curr;
        private bool _isWorking = false;
        private bool _rememberMe = Properties.Settings.Default.rememberMe;
        private INavigationService _navigationService;
        private ILoggerService _loggerService;

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

        public AuthorizationViewModel(INavigationService navigationService, ILoggerService loggerService)
        {
            _navigationService = navigationService;
            _loggerService = loggerService;

            if (RememberMe)
            {
                Login = Properties.Settings.Default.rememberLogin;
                Password = Properties.Settings.Default.rememberPassword;
            }
        }

        public User GetUser()
        {
            return DBEntities.GetContext().User.FirstOrDefault(x => x.Login == _login);
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }

        private string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            //0A:00:27:00:00:0A

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                sb.Append(macAddress.Substring(i * 2, 2));
                if (i != 5)
                {
                    sb.Append(":");
                }
            }

            return sb.ToString();
        }

        private void SettingNavigationAfterAuth(User user)
        {
            Window window;
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
            _loggerService.AddLog(
                $"Устройство: {System.Environment.MachineName}\n" +
                $"Имя пользователя: {System.Environment.UserName}\n" +
                $"Версия ОС: {System.Environment.OSVersion}\n" +
                $"Имя сетевого домена: {System.Environment.UserDomainName}\n" +
                $"IPv4: {GetLocalIPAddress()}\n" +
                $"MAC: {GetMacAddress()}",
                Model.LogTypeEnum.AUTH_USER);

            Properties.Settings.Default.Save();
            IsWorking = false;
            _curr.Hide();
            window.Show();
            _curr.Close();
            _curr = null;
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
                            if (_curr == null)
                            {
                                _curr = obj as Window;
                            }

                            IsWorking = true;

                            return App.Current.Dispatcher.InvokeAsync(() =>
                            {
                                try
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
                                            _loggerService.AddLog($"Неудачная попытка авторизации в аккаунт пользователя с логином {user.Login}", Model.LogTypeEnum.AUTH_USER_TRY);
                                            IsWorking = false;
                                            Helpers.MessageBoxHelper.Error("Неверный пароль");
                                        }
                                    }
                                    else
                                    {
                                        IsWorking = false;
                                        Helpers.MessageBoxHelper.Error("Пользователь с таким логином не найден");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Helpers.MessageBoxHelper.Error($"Возникла ошибка во время авторизации:\n{ex.Message}");
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