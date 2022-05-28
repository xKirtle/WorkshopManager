using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using SteamworksWorker;
using SteamworksWorker.Modules;

namespace MVVMApplication.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public ObservableCollection<WorkshopItem> ResultItems { get; }
        public bool IsAddingItems { get; set; } = false;
        public QueryInstance QueryInstance;
        
        public MainMenuViewModel()
        {
            QueryInstance = new();
            ResultItems = new();
            // AsyncAddItems();
        }

        public async Task AsyncAddItems()
        {
            Console.WriteLine("Request received");
        }
    }
}