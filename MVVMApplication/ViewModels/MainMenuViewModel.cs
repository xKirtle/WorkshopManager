using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        public ObservableCollection<FilterItem> FilterItems { get; }
        public ObservableCollection<WorkshopItem> WorkshopVisibleItems { get; }
        public QueryInstance QueryInstance;
        
        private Dictionary<ulong, WorkshopItem> _itemsDictionary;
        private AutoResetEvent _evtSignalling;

        public MainMenuViewModel()
        {
            QueryInstance = new(AddItemToResultItems);
            WorkshopVisibleItems = new();
            FilterItems = new();
            _itemsDictionary = new();
            _evtSignalling = new(false);
            AddFilterItems();
            AsyncFetchWorkshopItems();
        }
        
        public Task AsyncFetchWorkshopItems() => Task.Run(() => QueryInstance.QueryAllPages());

        private void AddItemToResultItems(WorkshopItem item)
        {
            DownloadImage(item);
            
            //Wait for the first 3 items to be fully ready since they'll be on display
            if (WorkshopVisibleItems.Count <= 3)
                _evtSignalling.WaitOne();
            
            WorkshopVisibleItems.Add(item);
            _itemsDictionary.Add(item.WorkshopFileID, item);
        }
        
        private void DownloadImage(WorkshopItem item)
        {
            using WebClient client = new WebClient();
            client.DownloadDataAsync(new Uri(item.IconUri));
            client.DownloadDataCompleted += DownloadComplete;
            
            void DownloadComplete(object sender, DownloadDataCompletedEventArgs e)
            {
                byte[] bytes = e.Result;
                Stream stream = new MemoryStream(bytes);
                item.BitmapIcon = new Bitmap(stream);
                _evtSignalling.Set();
            }
        }
        
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