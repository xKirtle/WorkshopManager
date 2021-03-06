using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using MVVMApplication.Models;
using MVVMApplication.Utils;
using ReactiveUI;
using SteamworksWorker;
using SteamworksWorker.Modules;

namespace MVVMApplication.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public ObservableCollection<FilterItem> FilterItems { get; }
        public ObservableCollection<WorkshopItem> WorkshopVisibleItems { get; set; }
        public Dictionary<ulong, WorkshopItem> ItemsDictionary { get; private set; }
        private List<WorkshopItem> _fixedWorkshopItemsList;
        private List<WorkshopItem> _filteredWorkshopItemsList;
        private QueryInstance QueryInstance;
        private AutoResetEvent _evtSignalling;
        private string _prevText;

        private const int InitialzeNum = 2048;
        public MainMenuViewModel()
        {
            QueryInstance = new(AddItemToResultItems);
            WorkshopVisibleItems = new();
            FilterItems = new();
            ItemsDictionary = new(InitialzeNum);
            _fixedWorkshopItemsList = new(InitialzeNum);
            _filteredWorkshopItemsList = new(InitialzeNum);
            _evtSignalling = new(false);
            AddFilterItems();
            AsyncFetchWorkshopItems();
        }

        public Task AsyncFetchWorkshopItems() => Task.Run(() => QueryInstance.QueryNextPage());
        
        public void AsyncFilterItemsByKeyword(string text)
        {
            if (text == _prevText) return;

            Task.Run(() =>
            {
                List<WorkshopItem> filteredList = new();
                WorkshopVisibleItems.Clear();
                _prevText = text;
                if (!String.IsNullOrEmpty(text))
                {
                    _filteredWorkshopItemsList.ForEach(item =>
                    {
                        if (item.DisplayName.Contains(text, StringComparison.CurrentCultureIgnoreCase) 
                            || item.ShortDescription.Contains(text, StringComparison.CurrentCultureIgnoreCase) 
                            || item.Tags.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                            WorkshopVisibleItems.Add(item);
                    });
                }
                else WorkshopVisibleItems.AddRange(_filteredWorkshopItemsList); 
            });
        }

        public void ChangeFilter(int filterIndex)
        {
            //Perform operations to change _filteredWorkshopItemsList
        }
        
        private void AddItemToResultItems(WorkshopItem item)
        {
            DownloadImage(item);

            //Wait for the first 3 items to be fully ready since they'll be on display
            if (WorkshopVisibleItems.Count <= 3)
                _evtSignalling.WaitOne();
            
            WorkshopVisibleItems.Add(item);
            _fixedWorkshopItemsList.Add(item);
            _filteredWorkshopItemsList.Add(item);
            ItemsDictionary.Add(item.WorkshopFileID, item);
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

        public async void AsyncSubscribeAndDownloadWorkshopItem(Action action)
        {
            await Task.Run(() =>
            {
                //Actually do stuff here
                var sw = Stopwatch.StartNew();
                while (sw.Elapsed.Seconds <= 3) ;
            });
            
            action.Invoke();
        }

        public void AsyncUnsubscribeAndRemoveWorkshopItem(WorkshopItem item)
        {
            
        }
    }
}