using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using MVVMApplication.Models;
using SteamworksWorker;
using SteamworksWorker.Modules;

namespace MVVMApplication.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public ObservableCollection<FilterItem> FilterItems { get; }
        public ObservableCollection<WorkshopItem> ResultItems { get; }
        public bool IsAddingItems { get; private set; } = false;
        public QueryInstance QueryInstance;
        
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        
        public MainMenuViewModel()
        {
            _cancellationTokenSource = new();
            _cancellationToken = _cancellationTokenSource.Token;
            
            QueryInstance = new(_cancellationToken, onItemHandled: AddItemToResultItems);
            ResultItems = new();
            FilterItems = new();
            AddFilterItems();
        }

        public async Task AsyncAddItems()
        {
            //Necessary lock mechanism for if the user is scrolling too fast
            IsAddingItems = true;
            await Task.Run(() =>
            {
                QueryInstance.QueryNextPage();
                IsAddingItems = false;
            });
        }

        public void SetQueryFilter(QueryType type)
        {
            //Sending a cancellation request to any tasks that might be running
            //Necessary for if the user swaps between filters too fast and some
            //tasks, like downloading the mods icon might still be in progress
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new();
            _cancellationToken = _cancellationTokenSource.Token;

            ResultItems.Clear();
            QueryInstance = new QueryInstance(_cancellationToken, onItemHandled: AddItemToResultItems, queryType: type);
            AsyncAddItems();
        }

        private void AddItemToResultItems(WorkshopItem item)
        {
            ConvertStreamToBitmap(item);
            ResultItems.Add(item);
        }
        
        private void ConvertStreamToBitmap(WorkshopItem workshopItem) =>
            workshopItem.BitmapIcon = new Bitmap(workshopItem.BitmapIcon);

        private void AddFilterItems()
        {
            string[] filters = new[]
            {
                "Most Voted", "Most Recent", "Last Updated", "Most Subscribed", "Favorited by Friends", "Most Popular"
            };
            string[] icons = new[]
            {
                "arrow-up-solid.png", "calendar-solid.png", "clock-solid.png", "user-solid.png", "star-solid.png",
                "fire-solid.png"
            };
            for (int i = 0; i < filters.Length; i++)
            {
                FilterItem item = new FilterItem(filters[i], icons[i]);
                FilterItems.Add(item);
            }
        }
    }
}