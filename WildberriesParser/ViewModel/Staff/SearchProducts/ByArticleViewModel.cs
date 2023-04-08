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
using System.Diagnostics;

namespace WildberriesParser.ViewModel.Staff.SearchProducts
{
    public class ByArticleViewModel : ViewModelBase
    {
        private string _article;
        private string _result = "Htess";
        private bool _isSearchWorking;
        private bool _isExportWorking;
        private ExcelService _excelService;
        private ILoggerService _loggerService;

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

        public ByArticleViewModel(INavigationService navigationService,
            WbRequesterService wbRequesterService, WbParser wbParser,
            ExcelService excelService, ILoggerService loggerService)
        {
            NavigationService = navigationService;
            _wbRequesterService = wbRequesterService;
            _wbParser = wbParser;
            _excelService = excelService;
            _loggerService = loggerService;
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
                           IsSearchWorking = true;
                           WbProduct product = await _Search();
                           if (product != null)
                           {
                               Products.Insert(0, product);
                           }
                           IsSearchWorking = false;
                       }).Task;
                    },
                    (obj) => !IsSearchWorking && !string.IsNullOrEmpty(_article) && Int32.TryParse(_article, out int _)
                    ));
            }
        }

        private RelayCommand _clearCommand;

        public RelayCommand ClearCommand
        {
            get
            {
                return _clearCommand ??
                    (_clearCommand = new RelayCommand
                    ((obj) =>
                    {
                        Products.Clear();
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
                            data.Add("Артикул", new List<object>());
                            data.Add("Название", new List<object>());
                            data.Add("Бренд", new List<object>());
                            data.Add("Скидка", new List<object>());
                            data.Add("Цена без скидки", new List<object>());
                            data.Add("Цена со скидкой", new List<object>());
                            data.Add("Рейтинг", new List<object>());
                            data.Add("Отзывы", new List<object>());
                            data.Add("Промо текст", new List<object>());

                            foreach (var product in _products)
                            {
                                data["Артикул"].Add(product.id);
                                data["Название"].Add(product.name);
                                data["Бренд"].Add(product.brand);
                                data["Скидка"].Add(product.sale);
                                data["Цена без скидки"].Add(product.priceU);
                                data["Цена со скидкой"].Add(product.salePriceU);
                                data["Рейтинг"].Add(product.rating);
                                data["Отзывы"].Add(product.feedbacks);
                                data["Промо текст"].Add(product.promoTextCat);
                            }
                            try
                            {
                                _excelService.Export(data, path);
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

        public bool IsSearchWorking
        {
            get => _isSearchWorking;
            set => Set(ref _isSearchWorking, value);
        }

        public string Result
        {
            get => _result;
            set => Set(ref _result, value);
        }

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }
    }
}