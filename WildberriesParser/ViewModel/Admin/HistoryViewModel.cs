﻿using System.Collections.Generic;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using WildberriesParser.Infastructure.Commands;
using System;

namespace WildberriesParser.ViewModel.Admin
{
    public class HistoryViewModel : ViewModelBase
    {
        private ObservableCollection<Log> _logs;
        private string _searchText;
        private INavigationService _navigationService;

        public ObservableCollection<Log> Logs
        {
            get => _logs;
            set => Set(ref _logs, value);
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                Set(ref _searchText, value);
            }
        }

        public HistoryViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            DBEntities.GetContext().Log.Load();
            _logs = DBEntities.GetContext().Log.Local;
        }
    }
}