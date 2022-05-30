using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using MVVMApplication.Models;
using ReactiveUI;
using SteamworksWorker;
using SteamworksWorker.Modules;

namespace MVVMApplication.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public static MainMenuViewModel Instance;
        public ObservableCollection<FilterItem> FilterItems { get; }
        public ObservableCollection<WorkshopItem> WorkshopVisibleItems { get; }
        public bool IsAddingItems { get; private set; } = false;
        public QueryInstance QueryInstance;

        private List<WorkshopItem> _items;

        public MainMenuViewModel()
        {
            Instance = this;
            
            QueryInstance = new(AddItemToResultItems);
            //To avoid resizes
            WorkshopVisibleItems = new();
            FilterItems = new();
            _items = new();
            AddFilterItems();

            AsyncFetchWorkshopItems();
            //TODO: Stop abusing steam's api and lazy load all mods to later filter locally?
        }

        public async Task AsyncFetchWorkshopItems()
        {
            //Necessary lock mechanism for if the user is scrolling too fast
            IsAddingItems = true;
            await Task.Run(() =>
            {
                QueryInstance.QueryNextPage();
                IsAddingItems = false;
            });
        }

        private void AddItemToResultItems(WorkshopItem item)
        {
            ConvertStreamToBitmap(item);
            WorkshopVisibleItems.Add(item);
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