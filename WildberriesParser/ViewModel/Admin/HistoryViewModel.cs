using System.Collections.Generic;
using System.Linq;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Admin
{
    public class HistoryViewModel : ViewModelBase
    {
        private List<Log> _users;
        private string _searchText;
        private INavigationService _navigationService;

        public List<Log> Users
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
                if (Set(ref _searchText, value))
                {
                }
            }
        }

        public HistoryViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            _users = DBEntities.GetContext().Log.ToList();
        }
    }
}