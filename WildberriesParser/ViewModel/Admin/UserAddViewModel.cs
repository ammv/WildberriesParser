using System;
using System.Linq;
using System.Windows;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Admin
{
    public class UserAddViewModel : ViewModelBase
    {
        public string Title { get; } = "Добавление пользователей";

        private string _login;
        private string _password;
        private int _selectedRoleId = 2;
        private bool _isWorking = false;
        private INavigationService _navigationService;
        private ILoggerService _loggerService;

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

        public UserAddViewModel(INavigationService navigationService, ILoggerService loggerService)
        {
            _navigationService = navigationService;
            _loggerService = loggerService;
        }

        public User GetUser()
        {
            return DBEntities.GetContext().User.FirstOrDefault(x => x.Login == _login);
        }

        private void _addUser()
        {
            User user = new User
            {
                Login = _login,
                Password = _password,
                RoleID = _selectedRoleId
            };

            DBEntities.GetContext().User.Add(user);

            _loggerService.AddLog(
                $"Создание пользователя\nЛогин: {_login}\nПароль: {_password}\nРоль: {DBEntities.GetContext().Role.FirstOrDefault(r => r.ID == _selectedRoleId).Name}",
                Model.LogTypeEnum.CREATE_USER);
        }

        private AsyncRelayCommand _addUserCommand;

        public AsyncRelayCommand AddUserCommand
        {
            get
            {
                return _addUserCommand ?? (_addUserCommand = new AsyncRelayCommand
                    (
                        (obj) =>
                        {
                            IsWorking = true;

                            return App.Current.Dispatcher.InvokeAsync(() =>
                            {
                                try
                                {
                                    if (GetUser() == null)
                                    {
                                        _addUser();
                                        IsWorking = false;
                                        Helpers.MessageBoxHelper.Information("Пользватель добавлен");
                                        _navigationService.NavigateTo<UsersViewModel>();
                                    }
                                    else
                                    {
                                        IsWorking = false;
                                        Helpers.MessageBoxHelper.Error("Пользователь с таким логином уже существует");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Helpers.MessageBoxHelper.Error(ex.Message);
                                }
                            }).Task;
                        },
                        (obj) => !string.IsNullOrEmpty(_login) && !string.IsNullOrEmpty(_password)
                    ));
            }
        }

        private RelayCommand _SelectRoleCommand;

        public RelayCommand SelectRoleCommand
        {
            get
            {
                return _SelectRoleCommand ??
                    (_SelectRoleCommand = new RelayCommand
                    ((obj) =>
                    {
                        _selectedRoleId = (int)obj;
                    }
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