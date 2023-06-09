﻿using System.Collections.Generic;
using WildberriesParser.Infastructure.Core;
using DataLayer;
using WildberriesParser.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using WildberriesParser.Infastructure.Commands;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WildberriesParser.ViewModel.Admin
{
    public class HistoryViewModel : ViewModelBase
    {
        public string Title { get; } = "История";

        private PagedList<Log> _logs;
        private PagedListCommands<Log> _pagedCommands;
        private int _selectedIndex = 0;
        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };
        private bool _isExportWorking;
        private ExcelService _excelService;
        private string _searchText;
        private INavigationService _navigationService;

        public PagedList<Log> Logs
        {
            get => _logs;
            set => Set(ref _logs, value);
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

        public HistoryViewModel(INavigationService navigationService,
            ExcelService excelService)
        {
            NavigationService = navigationService;

            _excelService = excelService;

            Logs = new PagedList<Log>(DBEntities.GetContext()
                                    .Log.OrderByDescending(x => x.Date).ToList(), _pageSizes[_selectedIndex]);
            PagedCommands = new PagedListCommands<Log>(Logs);
        }

        private AsyncRelayCommand _SearchCommand;

        public AsyncRelayCommand SearchCommand
        {
            get
            {
                return _SearchCommand ??
                    (_SearchCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(_searchText))
                                {
                                    Logs = new PagedList<Log>(DBEntities.GetContext()
                                        .Log.OrderByDescending(u => u.ID).ToList(), 25);
                                }
                                else if (int.TryParse(SearchText, out int userID))
                                {
                                    Logs = new PagedList<Log>(DBEntities.GetContext()
                                        .Log.Where(u => u.UserID == userID)
                                        .OrderByDescending(l => l.Date).ToList(), 25);
                                }
                                else
                                {
                                    Logs = new PagedList<Log>(DBEntities.GetContext()
                                       .Log.Where(u => u.User.Login.ToLower().Contains(_searchText.ToLower()))
                                       .OrderByDescending(l => l.Date).ToList(), 25);
                                }
                                PagedCommands.Instance = Logs;
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
                            data.Add("Пользователь", new List<object>());
                            data.Add("Тип", new List<object>());
                            data.Add("Описание", new List<object>());
                            data.Add("Дата", new List<object>());

                            foreach (var item in _logs.ItemsSource)
                            {
                                data["ID"].Add(item.ID);
                                data["Пользователь"].Add(item.User?.Login);
                                data["Тип"].Add(item.LogType.Name);
                                data["Описание"].Add(item.Description);
                                data["Дата"].Add(item.Date.ToString("g"));
                            }
                            try
                            {
                                _excelService.Export(ExcelColumn.FromDictionary(data), path, "Логи");
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

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }

        public PagedListCommands<Log> PagedCommands
        {
            get => _pagedCommands;
            set
            {
                Set(ref _pagedCommands, value);
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                Logs.PageSize = _pageSizes[value];
            }
        }

        public int[] PageSizes
        {
            get => _pageSizes;
            set => _pageSizes = value;
        }
    }
}