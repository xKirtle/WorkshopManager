using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DynamicData;
using SteamworksWorker;
using SteamworksWorker.Modules;

namespace MVVMApplication.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public ObservableCollection<TextBlock> FilterItems { get; }
        public ObservableCollection<WorkshopItem> ResultItems { get; }
        public bool IsAddingItems { get; set; } = false;
        public QueryInstance QueryInstance;
        
        public MainMenuViewModel()
        {
            QueryInstance = new(onItemHandled: AddItemToResultItems);
            ResultItems = new();
            FilterItems = new();
            AddFilterItems();
        }

        public async Task AsyncAddItems()
        {
            //Necessary lock mechanism for if the user is scrolling too fast
            IsAddingItems = true;
            Task task = Task.Run(() =>
            {
                QueryInstance.QueryNextPage();
                IsAddingItems = false;
                Console.WriteLine($"New Items Count: {ResultItems.Count}");
            });
        }

        private void AddItemToResultItems(WorkshopItem item)
        {
            //Bitmap might not have been downloaded yet! Need to wait for it!
            while (item.BitmapIcon is not Stream);
            
            ConvertStreamToBitmap(item);
            ResultItems.Add(item);
        }
        
        private void ConvertStreamToBitmap(WorkshopItem workshopItem) =>
            workshopItem.BitmapIcon = new Bitmap(workshopItem.BitmapIcon);

        private void AddFilterItems()
        {
            string[] filters = new[] {"Most Voted", "Most Recent", "Last Updated", "Most Subscribed"};
            for (int i = 0; i < filters.Length; i++)
            {
                TextBlock textBlock = new();
                textBlock.Text = filters[i];
                FilterItems.Add(textBlock);
            }
        }
    }
}