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
    public class BySearchViewModel : ViewModelBase
    {
        public string Title { get; } = "Поиск по запросу";

        private string _searchPattern;
        private bool _isExportWorking;
        private string _result = "Htess";
        private bool _isSearchWorking;
        private ExcelService _excelService;

        private int _selectedIndex = 0;
        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        private ObservableCollection<WbProduct> _originalProducts = new ObservableCollection<WbProduct>();

        private PagedList<WbProduct> _products;
        private PagedListCommands<WbProduct> _pagedCommands;

        public PagedList<WbProduct> Products
        {
            get => _products;
            set { Set(ref _products, value); }
        }

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public string SearchPattern
        {
            get => _searchPattern;
            set
            {
                Set(ref _searchPattern, value);
            }
        }

        public BySearchViewModel(INavigationService navigationService, WbParser wbParser,
            ExcelService excelService, WbRequesterService wbRequesterService)
        {
            NavigationService = navigationService;
            _wbRequesterService = wbRequesterService;
            _wbParser = wbParser;
            _excelService = excelService;

            _products = new PagedList<WbProduct>(_originalProducts, _pageSizes[_selectedIndex]);
            _pagedCommands = new PagedListCommands<WbProduct>(_products);
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;
        private readonly WbRequesterService _wbRequesterService;
        private readonly WbParser _wbParser;

        private async Task<List<WbProduct>> _Search()
        {
            List<WbProduct> products = new List<WbProduct>();
            var responsesJson = await _wbRequesterService.GetProductCardsBySearch(_searchPattern, 1);
            foreach (var responseJson in responsesJson)
            {
                var response = _wbParser.ParseResponse(responseJson);
                if (response.Data?.Products.Count > 0)
                {
                    foreach (var product in response.Data.Products)
                    {
                        products.Insert(0, product);
                    }
                }
            }

            return products;
        }

        private async Task AddProductsToDB(IEnumerable<WbProduct> wbProducts)
        {
            DateTime now = DateTime.Now.Date;

            foreach (var product in wbProducts)
            {
                if (await DBEntities.GetContext().WbBrand.FirstOrDefaultAsync(b => b.ID == product.WbBrandID) == null)
                {
                    DBEntities.GetContext().WbBrand.Add(new WbBrand
                    {
                        Name = product.brand,
                        ID = product.WbBrandID
                    });
                }

                Model.Data.WbProduct findProduct = await DBEntities.GetContext().WbProduct.FirstOrDefaultAsync(x => x.ID == product.id);
                if (findProduct == null)
                {
                    var newProduct = new Model.Data.WbProduct
                    {
                        ID = product.id,
                        Name = product.name,
                        WbBrandID = product.WbBrandID,
                        LastUpdate = now
                    };

                    DBEntities.GetContext().WbProduct.Add(newProduct);

                    DBEntities.GetContext().WbProductChanges.Add(new WbProductChanges
                    {
                        WbProduct = newProduct,
                        Date = now,
                        Discount = product.sale,
                        PriceWithDiscount = (int)product.salePriceU,
                        PriceWithoutDiscount = (int)product.priceU,
                        Quantity = product.Quantity ?? 0
                    });
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
                            Discount = product.sale,
                            PriceWithDiscount = (int)product.salePriceU,
                            PriceWithoutDiscount = (int)product.priceU,
                            Quantity = product.Quantity ?? 0
                        });
                    }
                }
            }

            await DBEntities.GetContext().SaveChangesAsync();
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
                           var products = await _Search();
                           if (products.Count > 0)
                           {
                               var responseJson = await _wbRequesterService.GetProductsByArticlesSite(products.Select(x => x.id).ToList());

                               var response = _wbParser.ParseResponse(responseJson);

                               foreach (var product in response.Data.Products)
                               {
                                   _originalProducts.Add(product);
                               }

                               Products = new PagedList<WbProduct>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                               _pagedCommands.Instance = Products;

                               await AddProductsToDB(response.Data.Products);
                           }
                           IsSearchWorking = false;
                       }).Task;
                    },
                    (obj) => !IsSearchWorking && !string.IsNullOrEmpty(_searchPattern)
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
                    }
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