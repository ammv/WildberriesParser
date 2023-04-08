using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Infastructure.Commands;

namespace WildberriesParser.Infastructure.Core
{
    public class PagedListCommands<T> : ObservableObject
    {
        private PagedList<T> _instance;

        public PagedList<T> Instance
        {
            get => _instance;
            set
            {
                _instance = value;
                OnPropertyChanged();
            }
        }

        public PagedListCommands(PagedList<T> instance)
        {
            Instance = instance;
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
                        _instance.ToFirst();
                    },
                    (obj) => _instance.CanPrevious
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
                        _instance.ToLast();
                    },
                    (obj) => _instance.CanNext
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
                        _instance.Previous();
                    },
                    (obj) => _instance.CanPrevious
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
                        _instance.Next();
                    },
                    (obj) => _instance.CanNext
                    ));
            }
        }
    }
}