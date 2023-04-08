using System.Collections.Generic;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using WildberriesParser.Infastructure.Commands;

using WildberriesParser.Infastructure.Core;

using System;
using System.Threading.Tasks;

namespace WildberriesParser.ViewModel.Admin
{
    public class HistoryViewModel : ViewModelBase
    {
        private PagedList<Log> _logs;
        private PagedListCommands<Log> _pagedCommands;
        private int _selectedIndex = 0;
        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };
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

        public HistoryViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            Logs = new PagedList<Log>(DBEntities.GetContext()
                                    .Log.OrderBy(u => u.ID), 25);
            PagedCommands = new PagedListCommands<Log>(Logs);
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
                        Task task2 = App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            if (string.IsNullOrEmpty(_searchText))
                            {
                                Logs = new PagedList<Log>(DBEntities.GetContext()
                                    .Log.OrderBy(u => u.ID), 25);
                            }
                            else
                            {
                                Logs = new PagedList<Log>(DBEntities.GetContext()
                                    .Log.Where(u => u.User.Login.Contains(_searchText))
                                    .OrderBy(l => l.Date), 25);
                            }
                            PagedCommands.Instance = Logs;
                        }).Task;
                        return Task.WhenAll(task2);
                    }
                    ));
            }
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