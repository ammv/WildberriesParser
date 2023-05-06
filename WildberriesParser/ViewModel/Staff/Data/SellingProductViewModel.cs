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
    public class SellingProduct
    {
        public SellingProduct(WbProductChanges changes, int sales)
        {
            Changes = changes;
            Sales = sales;
        }

        public WbProductChanges Changes { get; set; }
        public int Sales { get; set; }

        public decimal Cash
        {
            get => Sales * Changes.PriceWithDiscount;
        }
    }

    internal class SellingProductViewModel : ViewModelBase
    {
        public string Title { get; } = "Отчет по изменению товара";

        private string _article;
        private string _tracedValue;
        private bool _isExportWorking;
        private bool _isShowWorking;

        private ExcelService _excelService;
        private readonly INavigationService navigationService;

        // Для пагинации
        private int _selectedIndex = 0;

        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        private List<SellingProduct> _originalProducts;

        private PagedList<SellingProduct> _productPosChanges;
        private PagedListCommands<SellingProduct> _pagedCommands;

        public PagedList<SellingProduct> ProductPosChanges
        {
            get => _productPosChanges;
            set { Set(ref _productPosChanges, value); }
        }

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }

        public string Article
        {
            get => _article;
            set
            {
                Set(ref _article, value);
            }
        }

        public SellingProductViewModel(
            ExcelService excelService, INavigationService navigationService)
        {
            _excelService = excelService;
            this.navigationService = navigationService;
            _originalProducts = null;

            ProductPosChanges = new PagedList<SellingProduct>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands = new PagedListCommands<SellingProduct>(ProductPosChanges);
            //PagedCommands.Instance = ProductPosChanges;
        }

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

        public void updateData()
        {
            int article = int.Parse(_article);
            var data = DBEntities.GetContext().WbProductChanges
                .Where(x => x.WbProductID == article).OrderBy(x => x.Date).ToList();

            if (data.Count <= 1)
            {
                Helpers.MessageBoxHelper.Error("Нехватает данных о товаре для расчета продаж");
                return;
            }

            List<SellingProduct> sellingProducts = new List<SellingProduct>();

            for (int i = 0; i < data.Count - 1; i++)
            {
                int sales = data[i].Quantity - data[i + 1].Quantity;
                if (sales < 0)
                {
                    sales = 0;
                }
                sellingProducts.Add(new SellingProduct(data[i], sales));
            }

            // Подготовка данных для расчета

            //List<List<WbProductChanges>> changesWithGoodDateDiff = new List<List<WbProductChanges>>();

            //if (_originalProducts.Count == 2)
            //{
            //    if((_originalProducts[0].Date - _originalProducts[1].Date).Days == -1)
            //    {
            //        changesWithGoodDateDiff.Add(new List<WbProductChanges>());
            //        changesWithGoodDateDiff[0].Add(_originalProducts[0]);
            //        changesWithGoodDateDiff[0].Add(_originalProducts[1]);
            //    }
            //}
            //else
            //{
            //    int k = 0;

            //    for (int i = 0; i < _originalProducts.Count - 1; i++)
            //    {
            //        // если разница в 1 день
            //        if ((_originalProducts[i].Date - _originalProducts[i + 1].Date).Days == -1)
            //        {
            //            // если группа дат не создана, тогда создаем
            //            if (changesWithGoodDateDiff.Count == k)
            //            {
            //                // тогда создаем
            //                changesWithGoodDateDiff.Add(new List<WbProductChanges>());
            //            }
            //            if (_originalProducts.Count == i + 1)
            //                changesWithGoodDateDiff[k].Add(_originalProducts[i]);
            //        }
            //        else
            //        {
            //            k++;
            //        }
            //    }
            //}

            ProductPosChanges = new PagedList<SellingProduct>(sellingProducts, _pageSizes[_selectedIndex]);
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
                            //data.Add("Артикул", new List<object>());

                            foreach (var product in _originalProducts)
                            {
                                // data["Артикул"].Add(product.ID);
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

        private SellingProduct _selectedEntity;

        public SellingProduct SelectedEntity
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
                                    vm.Article = SelectedEntity.Changes.WbProductID.ToString();
                                    vm.updateData();
                                    navigationService.NavigateTo<Data.DataProductChangesViewModel>();
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
                                    vm.Article = SelectedEntity.Changes.WbProductID.ToString();
                                    vm.updateData();
                                    navigationService.NavigateTo<Data.SellingProductViewModel>();
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
                                    vm.Article = SelectedEntity.Changes.WbProductID.ToString();
                                    vm.updateData();
                                    navigationService.NavigateTo<Data.DataProductPosChangesViewModel>();
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

        public PagedListCommands<SellingProduct> PagedCommands
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