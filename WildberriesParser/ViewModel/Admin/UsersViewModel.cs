using System.Collections.Generic;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using WildberriesParser.Infastructure.Commands;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using WildberriesParser.Infastructure.Core;

namespace WildberriesParser.ViewModel.Admin
{
    public class UsersViewModel : ViewModelBase
    {
        public string Title { get; } = "Пользователи";

        private PagedList<User> _users;
        private PagedListCommands<User> _pagedCommands;
        private int _selectedIndex = 0;
        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };
        private bool _isExportWorking;
        private ExcelService _excelService;
        private string _searchText;
        private INavigationService _navigationService;

        private User _selectedUser;

        public User SelectedUser
        {
            get => _selectedUser;
            set => Set(ref _selectedUser, value);
        }

        public PagedList<User> Users
        {
            get => _users;
            set => Set(ref _users, value);
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                Set(ref _searchText, value);
            }
        }

        public UsersViewModel(INavigationService navigationService, ExcelService excelService)
        {
            NavigationService = navigationService;
            _excelService = excelService;
        }

        private void LoadUsers()
        {
            Users = new PagedList<User>(DBEntities.GetContext().User.OrderByDescending(u => u.ID), _pageSizes[_selectedIndex]);
            PagedCommands = new PagedListCommands<User>(Users);
        }

        private AsyncRelayCommand _RemoveCommand;

        public AsyncRelayCommand RemoveCommand
        {
            get
            {
                return _RemoveCommand ??
                    (_RemoveCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(async () =>
                       {
                           if (SelectedUser != null)
                           {
                               if (Helpers.MessageBoxHelper.Question($"Вы уверены, что хотите удалить пользователя {SelectedUser.Login}?") ==
                                   Helpers.MessageBoxHelperResult.YES)
                               {
                                   DBEntities.GetContext().User.Remove(SelectedUser);
                                   await DBEntities.GetContext().SaveChangesAsync();
                                   updateUsers();
                               }
                           }
                       }).Task;
                    }
                    ));
            }
        }

        private AsyncRelayCommand _EditCommand;

        public AsyncRelayCommand EditCommand
        {
            get
            {
                return _EditCommand ??
                    (_EditCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                       {
                           Helpers.MessageBoxHelper.Error("Еще не реализовано!");
                       }).Task;
                    }
                    ));
            }
        }

        private AsyncRelayCommand _HistoryCommand;

        public AsyncRelayCommand HistoryCommand
        {
            get
            {
                return _HistoryCommand ??
                    (_HistoryCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Helpers.MessageBoxHelper.Error("Еще не реализовано!");
                        }).Task;
                    }
                    ));
            }
        }

        private RelayCommand _AddUserCommand;

        public RelayCommand AddUserCommand
        {
            get
            {
                return _AddUserCommand ??
                    (_AddUserCommand = new RelayCommand
                    ((obj) =>
                    {
                        _navigationService.NavigateTo<UserAddViewModel>();
                    }
                    ));
            }
        }

        private AsyncRelayCommand _LoadedCommand;

        public AsyncRelayCommand LoadedCommand
        {
            get
            {
                return _LoadedCommand ??
                    (_LoadedCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            LoadUsers();
                        }).Task;
                    }
                    ));
            }
        }

        private AsyncRelayCommand _SearchTextChangedCommand;

        public AsyncRelayCommand SearchTextChangedCommand
        {
            get
            {
                return _SearchTextChangedCommand ??
                    (_SearchTextChangedCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                updateUsers();
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error(ex.Message);
                            }
                        }).Task;
                    }
                    ));
            }
        }

        private void updateUsers()
        {
            if (string.IsNullOrEmpty(_searchText))
            {
                Users = new PagedList<User>(DBEntities.GetContext()
                    .User.OrderByDescending(u => u.ID), _pageSizes[_selectedIndex]);
            }
            else
            {
                Users = new PagedList<User>(DBEntities.GetContext()
                    .User.Where(u => u.Login.Contains(_searchText))
                    .OrderByDescending(l => l.ID), _pageSizes[_selectedIndex]);
            }
            PagedCommands.Instance = Users;
        }

        private AsyncRelayCommand _exportCommand;

        public AsyncRelayCommand ExportCommand
        {
            get
            {
                return _exportCommand ??
                    (_exportCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            string path = _excelService.ShowSaveAsFileDialog();
                            if (path == null)
                            {
                                Helpers.MessageBoxHelper.Error("Вы не выбрали файл!");
                                return;
                            }

                            IsExportWorking = true;
                            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>();
                            data.Add("ID", new List<object>());
                            data.Add("Логин", new List<object>());
                            data.Add("Пароль", new List<object>());
                            data.Add("Роль", new List<object>());

                            foreach (var item in _users.ItemsSource)
                            {
                                data["ID"].Add(item.ID);
                                data["Логин"].Add(item.Login);
                                data["Пароль"].Add(item.Password);
                                data["Роль"].Add(item.Role.Name);
                            }
                            try
                            {
                                _excelService.Export(ExcelColumn.FromDictionary(data), path);
                                if (Helpers.MessageBoxHelper.Question("Экcпортировано успешно! Открыть файл?") == Helpers.MessageBoxHelperResult.YES)
                                {
                                    Process.Start(path);
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error($"Во время экспорта произошла ошибка:\n{ex.Message}");
                            }
                            finally
                            {
                                IsExportWorking = false;
                            }
                        }).Task;
                    },
                    (obj) => !IsExportWorking
                    ));
            }
        }

        public PagedListCommands<User> PagedCommands
        {
            get => _pagedCommands;
            set
            {
                Set(ref _pagedCommands, value);
            }
        }

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                Users.PageSize = _pageSizes[value];
            }
        }

        public int[] PageSizes
        {
            get => _pageSizes;
            set => _pageSizes = value;
        }
    }
}