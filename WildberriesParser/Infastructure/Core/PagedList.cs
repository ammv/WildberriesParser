using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WildberriesParser.Infastructure.Core
{
    public class PagedList<T> : ObservableObject
    {
        private IEnumerable<T> _itemsSource;
        private IEnumerable<T> _displayedItems;
        private int _pageSize;
        private int _currentPage;
        private int _totalPages;
        private bool _canNext;
        private bool _canPrevious;

        public IEnumerable<T> ItemsSource
        {
            get => _itemsSource;
            set
            {
                _itemsSource = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<T> DisplayedItems
        {
            get => _displayedItems;
            set
            {
                _displayedItems = value;
                OnPropertyChanged();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                CurrentPage = 1;
                Refresh();
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
            }
        }

        public bool CanNext
        {
            get => _canNext;
            set
            {
                _canNext = value;
                OnPropertyChanged();
            }
        }

        public bool CanPrevious
        {
            get => _canPrevious;
            set
            {
                _canPrevious = value;
                OnPropertyChanged();
            }
        }

        public void Next()
        {
            if (_canNext)
            {
                CurrentPage++;
                Refresh();
            }
        }

        public void Previous()
        {
            if (_canPrevious)
            {
                CurrentPage--;
                Refresh();
            }
        }

        public void ToFirst()
        {
            CurrentPage = 1;
            Refresh();
        }

        public void ToLast()
        {
            CurrentPage = _totalPages;
            Refresh();
        }

        public PagedList(IEnumerable<T> collection, int pageSize)
        {
            if (pageSize < 0)
            {
                throw new ArgumentException("PageSize can not equals to zero");
            }

            _pageSize = pageSize;

            CurrentPage = 1;
            ItemsSource = collection;
            Refresh();
        }

        public void Refresh()
        {
            if (_itemsSource != null)
            {
                int count = _itemsSource.Count();
                TotalPages = (int)Math.Ceiling(count / (double)_pageSize);
                if (_currentPage == 1)
                {
                    DisplayedItems = ItemsSource.Take(_pageSize);
                    CanNext = _totalPages != _currentPage;
                    CanPrevious = false;
                }
                else
                {
                    CanNext = _currentPage != _totalPages;
                    CanPrevious = true;
                    DisplayedItems = ItemsSource.Skip(_pageSize * (_currentPage - 1)).Take(_pageSize);
                }
            }
        }
    }
}