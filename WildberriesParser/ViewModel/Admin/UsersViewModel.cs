﻿using System.Collections.Generic;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using WildberriesParser.Infastructure.Commands;
using System;
using System.Threading.Tasks;

namespace WildberriesParser.ViewModel.Admin
{
    public class UsersViewModel : ViewModelBase
    {
        private ObservableCollection<User> _users;
        private string _searchText;
        private INavigationService _navigationService;

        public ObservableCollection<User> Users
        {
            get => _users;
            set => Set(ref _users, value);
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

        public UsersViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            DBEntities.GetContext().User.Load();
            _users = DBEntities.GetContext().User.Local;
        }

        private AsyncRelayCommand _AddUserCommand;

        public AsyncRelayCommand AddUserCommand
        {
            get
            {
                return _AddUserCommand ??
                    (_AddUserCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        DBEntities.GetContext().User.Add(new User
                        {
                            Login = Guid.NewGuid().ToString().Substring(0, 15),
                            Password = Guid.NewGuid().ToString().Substring(0, 15),
                            Role = DBEntities.GetContext().Role.First(x => x.Name == "staff")
                        });
                        return DBEntities.GetContext().SaveChangesAsync();
                    }
                    ));
            }
        }

        private AsyncRelayCommand _SearchTextChangedCommand;

        public AsyncRelayCommand SearchTextChangedCommand
        {
            get
            {
                return _SearchTextChangedCommand ??
                    (_SearchTextChangedCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        Task task2 = App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Users.Clear();

                            if (string.IsNullOrEmpty(_searchText))
                            {
                                Users = new ObservableCollection<User>(DBEntities.GetContext()
                                    .User.OrderBy(u => u.ID));
                            }
                            else
                            {
                                Users = new ObservableCollection<User>(DBEntities.GetContext()
                                    .User.Where(u => u.Login.Contains(_searchText)).OrderBy(u => u.ID));
                            }
                        }).Task;
                        return Task.WhenAll(task2);
                    }
                    ));
            }
        }
    }
}