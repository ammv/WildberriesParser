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
        public string Title { get; } = "Поиск по артикулу";

        private string _article;

        private string _result = "Htess";
        private bool _isSearchWorking;
        private bool _isExportWorking;
        private int _selectedIndex = 0;
        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };
        private ExcelService _excelService;
        private ILoggerService _loggerService;

        private ObservableCollection<WbProduct> _originalProducts = new ObservableCollection<WbProduct>();

        private PagedList<WbProduct> _products;
        private PagedListCommands<WbProduct> _pagedCommands;

        public PagedList<WbProduct> Products
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

            _products = new PagedList<WbProduct>(_originalProducts, _pageSizes[_selectedIndex]);
            _pagedCommands = new PagedListCommands<WbProduct>(_products);
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;
        private readonly WbRequesterService _wbRequesterService;
        private readonly WbParser _wbParser;

        private async Task<WbProduct> _Search()
        {
            string responseJson = await _wbRequesterService.GetProductByArticleSite(Int32.Parse(_article));
            var response = _wbParser.ParseResponse(responseJson);
            WbProduct product = null;
            if (response.Data.Products.Count > 0)
            {
                product = response.Data.Products[0];
            }
            return product;
        }

        private void AddProductToDB(WbProduct wbProduct)
        {
            if (DBEntities.GetContext().WbBrand.FirstOrDefault(b => b.ID == wbProduct.WbBrandID) == null)
            {
                DBEntities.GetContext().WbBrand.Add(new WbBrand
                {
                    Name = wbProduct.brand,
                    ID = wbProduct.WbBrandID
                });

                DBEntities.GetContext().SaveChanges();
            }

            Model.Data.WbProduct findProduct = DBEntities.GetContext().WbProduct.FirstOrDefault(x => x.ID == wbProduct.id);
            DateTime now = DateTime.Now.Date;

            if (findProduct == null)
            {
                var newProduct = new Model.Data.WbProduct
                {
                    ID = wbProduct.id,
                    Name = wbProduct.name,
                    WbBrandID = wbProduct.WbBrandID,
                    LastUpdate = now
                };

                DBEntities.GetContext().WbProduct.Add(newProduct);

                DBEntities.GetContext().WbProductChanges.Add(new WbProductChanges
                {
                    WbProduct = newProduct,
                    Date = now,
                    Discount = wbProduct.sale,
                    PriceWithDiscount = (int)wbProduct.salePriceU,
                    PriceWithoutDiscount = (int)wbProduct.priceU,
                    Quantity = wbProduct.Quantity ?? 0
                });

                DBEntities.GetContext().SaveChanges();
            }
            else
            {
                if (findProduct.LastUpdate.Date < now)
                {
                    findProduct.LastUpdate = now;

                    DBEntities.GetContext().WbProductChanges.Add(new WbProductChanges
                    {
                        WbProduct = findProduct,
                        Date = now,
                        Discount = wbProduct.sale,
                        PriceWithDiscount = (int)wbProduct.salePriceU,
                        PriceWithoutDiscount = (int)wbProduct.priceU,
                        Quantity = wbProduct.Quantity ?? 0
                    });

                    DBEntities.GetContext().SaveChanges();
                }
            }
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
                                _originalProducts.Add(product);

                                Products = new PagedList<WbProduct>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                                _pagedCommands.Instance = Products;

                                AddProductToDB(product);
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
                        _originalProducts.Clear();
                        Products = new PagedList<WbProduct>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                        PagedCommands.Instance = Products;
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
                            data.Add("Остатки", new List<object>());
                            data.Add("Скидка", new List<object>());
                            data.Add("Цена без скидки", new List<object>());
                            data.Add("Цена со скидкой", new List<object>());
                            data.Add("Рейтинг", new List<object>());
                            data.Add("Отзывы", new List<object>());
                            data.Add("Промо текст", new List<object>());

                            foreach (var product in _originalProducts)
                            {
                                data["Артикул"].Add(product.id);
                                data["Название"].Add(product.name);
                                data["Бренд"].Add(product.brand);
                                data["Остатки"].Add(product.Quantity);
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

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                Products.PageSize = _pageSizes[value];
            }
        }

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }

        public PagedListCommands<WbProduct> PagedCommands
        {
            get => _pagedCommands;
            set
            {
                Set(ref _pagedCommands, value);
            }
        }

        public int[] PageSizes
        {
            get => _pageSizes;
            set => _pageSizes = value;
        }
    }
}