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
            List<FilterItem> filterItems = new()
            {
                new FilterItem("Most Subscribed", "user-solid.png"),
                new FilterItem("Most Voted", "arrow-up-solid.png"),
                new FilterItem("Most Recent", "calendar-solid.png"),
                new FilterItem("Most Last Updated", "clock-solid.png"),
                new FilterItem("Favorited by Friends", "star-solid.png"),
                new FilterItem("Most Popular", "fire-solid.png")
            };
            
            filterItems.ForEach(item => FilterItems.Add(item));
        }
    }
}