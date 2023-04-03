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

namespace WildberriesParser.ViewModel.Staff.SearchProducts
{
    public class ByArticleViewModel : ViewModelBase
    {
        private string _article;
        private string _result = "Htess";
        private bool _isWorking;

        private ObservableCollection<Run> _productProperties;

        public ObservableCollection<Run> ProductProperties
        {
            get => _productProperties;
            set { Set(ref _productProperties, value); }
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

        private async Task _Search()
        {
            IsWorking = true;
            string responseJson = await _wbRequesterService.GetProductByArticleBasket(Int32.Parse(_article));
            var response = _wbParser.ParseResponse(responseJson);
            if (response.Data.Products.Count > 0)
            {
                WbProduct wbProduct = response.Data.Products[0];
                SetProductProperties(wbProduct);
            }
            else
            {
                Result = $"Товара с таким артикулом не существует!\nResponse: {responseJson}";
            }

            IsWorking = false;
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
                           await _Search();
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

        public void SetProductProperties<T>(T instance)
        {
            ProductProperties = new ObservableCollection<Run>();
            var properties = TypeDescriptor.GetProperties(instance);
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor prop = properties[i];

                if (prop.Category == "Important")
                {
                    Run description = new Run(prop.Description + ": ");
                    description.FontWeight = FontWeights.Bold;

                    Run value = new Run(prop.GetValue(instance).ToString());

                    ProductProperties.Add(description);
                    ProductProperties.Add(value);
                }
            }
        }

        public string Result
        {
            get => _result;
            set => Set(ref _result, value);
        }
    }
}