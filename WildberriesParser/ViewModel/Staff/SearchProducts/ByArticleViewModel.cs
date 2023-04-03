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

namespace WildberriesParser.ViewModel.Staff.SearchProducts
{
    public class ByArticleViewModel : ViewModelBase
    {
        private string _article;
        private string _result = "Htess";
        private bool _isWorking;

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

        public ByArticleViewModel(INavigationService navigationService, WbRequesterService wbRequesterService)
        {
            NavigationService = navigationService;
            _wbRequesterService = wbRequesterService;
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;
        private readonly WbRequesterService _wbRequesterService;

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
                            IsWorking = true;
                            Result = await _wbRequesterService.GetProductByArticleBasket(Int32.Parse(_article));
                            IsWorking = false;
                        }).Task;
                    },
                    (obj) => !string.IsNullOrEmpty(_article) && Int32.TryParse(_article, out int _)
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