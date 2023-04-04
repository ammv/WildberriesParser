using System.Collections.Generic;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using WildberriesParser.Infastructure.Commands;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;
using System.Windows.Documents;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WildberriesParser.ViewModel.Staff.SearchProducts
{
    public class ByArticleViewModel : ViewModelBase
    {
        private string _article;
        private string _result = "Htess";
        private bool _isWorking;

        private ObservableCollection<WbProduct> _products = new ObservableCollection<WbProduct>();

        public ObservableCollection<WbProduct> Products
        {
            get => _products;
            set { Set(ref _products, value); }
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public string Article
        {
            get => _article;
            set
            {
                Set(ref _article, value);
            }
        }

        public ByArticleViewModel(INavigationService navigationService, WbRequesterService wbRequesterService, WbParser wbParser)
        {
            NavigationService = navigationService;
            _wbRequesterService = wbRequesterService;
            _wbParser = wbParser;
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;
        private readonly WbRequesterService _wbRequesterService;
        private readonly WbParser _wbParser;

        private async Task<WbProduct> _Search()
        {
            string responseJson = await _wbRequesterService.GetProductByArticleBasket(Int32.Parse(_article));
            var response = _wbParser.ParseResponse(responseJson);
            WbProduct product = null;
            if (response.Data.Products.Count > 0)
            {
                product = response.Data.Products[0];
            }
            return product;
        }

        public AsyncRelayCommand SearchCommand
        {
            get
            {
                return _SearchCommand ??
                    (_SearchCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(async () =>
                       {
                           WbProduct product = await _Search();
                           if (product != null)
                           {
                               Products.Insert(0, product);
                           }
                       }).Task;
                    },
                    (obj) => !IsWorking && !string.IsNullOrEmpty(_article) && Int32.TryParse(_article, out int _)
                    ));
            }
        }

        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }

        public string Result
        {
            get => _result;
            set => Set(ref _result, value);
        }
    }
}