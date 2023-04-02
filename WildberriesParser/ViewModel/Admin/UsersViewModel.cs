using System.Collections.Generic;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using System.Linq;

namespace WildberriesParser.ViewModel.Admin
{
    public class UsersViewModel : ViewModelBase
    {
        private List<User> _users;
        private INavigationService _navigationService;

        public List<User> Users
        {
            get => _users;
            set => Set(ref _users, value);
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public UsersViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            _users = DBEntities.GetContext().User.ToList();
        }
    }
}