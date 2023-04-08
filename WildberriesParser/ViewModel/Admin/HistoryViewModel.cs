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
                        }).Task;
                        return Task.WhenAll(task2);
                    }
                    ));
            }
        }

        private RelayCommand _firstPageCommand;

        public RelayCommand FirstPageCommand
        {
            get
            {
                return _firstPageCommand ??
                    (_firstPageCommand = new RelayCommand
                    ((obj) =>
                    {
                        Logs.ToFirst();
                    },
                    (obj) => Logs.CanPrevious
                    ));
            }
        }

        private RelayCommand _lastPageCommand;

        public RelayCommand LastPageCommand
        {
            get
            {
                return _lastPageCommand ??
                    (_lastPageCommand = new RelayCommand
                    ((obj) =>
                    {
                        Logs.ToLast();
                    },
                    (obj) => Logs.CanNext
                    ));
            }
        }

        private RelayCommand _previousPageCommand;

        public RelayCommand PreviousPageCommand
        {
            get
            {
                return _previousPageCommand ??
                    (_previousPageCommand = new RelayCommand
                    ((obj) =>
                    {
                        Logs.Previous();
                    },
                    (obj) => Logs.CanPrevious
                    ));
            }
        }

        private RelayCommand _nextPageCommand;

        public RelayCommand NextPageCommand
        {
            get
            {
                return _nextPageCommand ??
                    (_nextPageCommand = new RelayCommand
                    ((obj) =>
                    {
                        Logs.Next();
                    },
                    (obj) => Logs.CanNext
                    ));
            }
        }
    }
}