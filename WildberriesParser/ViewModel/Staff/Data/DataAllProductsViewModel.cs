using SimpleWbApi;
using SimpleWbApi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using DataLayer;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Staff.Data
{
    public class DataAllProductsViewModel : ViewModelBase
    {
        public string Title { get; } = "Отчет по изменению товара";

        private string _article;
        private string _tracedValue;
        private bool _isExportWorking;
        private bool _isShowWorking;

        private ExcelService _excelService;

        // Для пагинации
        private int _selectedIndex = 0;

        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        private List<WbProduct> _originalProducts;

        private PagedList<WbProduct> _productPosChanges;
        private PagedListCommands<WbProduct> _pagedCommands;

        public PagedList<WbProduct> ProductPosChanges
        {
            get => _productPosChanges;
            set { Set(ref _productPosChanges, value); }
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

        public string Article
        {
            get => _article;
            set
            {
                Set(ref _article, value);
            }
        }

        public DataAllProductsViewModel(INavigationService navigationService,
            ExcelService excelService, WbAPI wbAPI)
        {
            NavigationService = navigationService;
            _excelService = excelService;

            _originalProducts = DBEntities.GetContext().WbProduct
                                .OrderByDescending(x => x.ID)
                                .ToList();

            ProductPosChanges = new PagedList<WbProduct>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands = new PagedListCommands<WbProduct>(ProductPosChanges);
            //PagedCommands.Instance = ProductPosChanges;
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _ShowCommand;

        public AsyncRelayCommand ShowCommand
        {
            get
            {
                return _ShowCommand ??
                    (_ShowCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                IsShowWorking = true;

                                try
                                {
                                    updateData();
                                }
                                catch (Exception ex)
                                {
                                    Helpers.MessageBoxHelper.Error(ex.Message);
                                }

                                IsShowWorking = false;

                                //await AddPosChangesToDB(result);
                            }).Task;
                    }));
            }
        }

        private void updateData()
        {
            if (string.IsNullOrEmpty(_article))
            {
                _originalProducts = DBEntities.GetContext().WbProduct
                .OrderBy(x => x.Name).ToList();
            }
            else
            {
                if (int.TryParse(_article, out int article))
                {
                    _originalProducts = DBEntities.GetContext().WbProduct
                    .Where(x => x.ID == article).OrderBy(x => x.Name).ToList();
                }
                else
                {
                    _originalProducts = DBEntities.GetContext().WbProduct
                    .Where(x => x.Name.ToLower().Contains(_article.ToLower()))
                    .OrderBy(x => x.Name).ToList();
                }
            }

            ProductPosChanges = new PagedList<WbProduct>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands.Instance = ProductPosChanges;
        }

        private DateTime? _startDate;
        private DateTime? _endDate;

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
                            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>();
                            data.Add("Артикул", new List<object>());
                            data.Add("Ссылка", new List<object>());
                            data.Add("Название", new List<object>());
                            data.Add("Бренд", new List<object>());
                            data.Add("ID Бренда", new List<object>());

                            foreach (var product in _originalProducts)
                            {
                                data["Артикул"].Add(product.ID);
                                data["Ссылка"].Add($@"https://www.wildberries.ru/catalog/{product.ID}/detail.aspx");
                                data["Название"].Add(product.Name);
                                data["Бренд"].Add(product.WbBrand.Name);
                                data["ID Бренда"].Add(product.WbBrandID);
                            }
                            try
                            {
                                var columns = ExcelColumn.FromDictionary(data);
                                columns[1].CellFormatType = ExcelCellFormatType.Hyperlink;
                                _excelService.Export(columns, path, "Карточки");
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

        //<DataGrid.ContextMenu>
        //            <ContextMenu>
        //                <MenuItem Icon = "{materialDesign:PackIcon Kind=ChartLineVariant}"
        //                          Header="Динамика цены"
        //                          Command="{Binding PriceDynamicCommand}" />
        //                <MenuItem Icon = "{materialDesign:PackIcon Kind=Sale}"
        //                          Header="Продажи"
        //                          Command="{Binding SalesCommand}" />
        //                <MenuItem Icon = "{materialDesign:PackIcon Kind=ShoePrint}"
        //                          Header="Позиции в поиске"
        //                          Command="{Binding TraceCommand}" />
        //            </ContextMenu>
        //        </DataGrid.ContextMenu>

        private WbProduct _selectedEntity;

        public WbProduct SelectedEntity
        {
            get => _selectedEntity;
            set => Set(ref _selectedEntity, value);
        }

        private AsyncRelayCommand _PriceDynamicCommand;

        public AsyncRelayCommand PriceDynamicCommand
        {
            get
            {
                return _PriceDynamicCommand ??
                    (_PriceDynamicCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (SelectedEntity != null)
                                {
                                    var vm = App.ServiceProvider.GetService(typeof(DataProductChangesViewModel)) as DataProductChangesViewModel;
                                    vm.Article = SelectedEntity.ID.ToString();
                                    vm.updateData();
                                    NavigationService.NavigateTo<Data.DataProductChangesViewModel>();
                                }
                                else
                                {
                                    Helpers.MessageBoxHelper.Error("Вы не выбрали данные!");
                                }
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

        private AsyncRelayCommand _SalesCommand;

        public AsyncRelayCommand SalesCommand
        {
            get
            {
                return _SalesCommand ??
                    (_SalesCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (SelectedEntity != null)
                                {
                                    var vm = App.ServiceProvider.GetService(typeof(SellingProductViewModel)) as SellingProductViewModel;
                                    vm.Article = SelectedEntity.ID.ToString();
                                    vm.updateData();
                                    NavigationService.NavigateTo<Data.SellingProductViewModel>();
                                }
                                else
                                {
                                    Helpers.MessageBoxHelper.Error("Вы не выбрали данные!");
                                }
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

        private AsyncRelayCommand _TracecxCommand;

        public AsyncRelayCommand TraceCxCommand
        {
            get
            {
                return _TracecxCommand ??
                    (_TracecxCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (SelectedEntity != null)
                                {
                                    var vm = App.ServiceProvider.GetService(typeof(DataProductPosChangesViewModel)) as DataProductPosChangesViewModel;
                                    vm.Article = SelectedEntity.ID.ToString();
                                    vm.updateData();
                                    NavigationService.NavigateTo<Data.DataProductPosChangesViewModel>();
                                }
                                else
                                {
                                    Helpers.MessageBoxHelper.Error("Вы не выбрали данные!");
                                }
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

        public bool IsShowWorking
        {
            get => _isShowWorking;
            set => Set(ref _isShowWorking, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                ProductPosChanges.PageSize = _pageSizes[value];
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

        public DateTime? StartDate
        {
            get => _startDate;
            set => Set(ref _startDate, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => Set(ref _endDate, value);
        }
    }
}