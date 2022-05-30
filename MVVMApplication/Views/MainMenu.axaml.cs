using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using DynamicData.Kernel;
using MVVMApplication.ViewModels;
using SteamworksWorker;
using SteamworksWorker.Modules;

namespace MVVMApplication.Views
{
    public partial class MainMenu : Window
    {
        public MainMenu() => InitializeComponent();

        private void FiltersListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as MainMenuViewModel;
        }

        private void DownloadButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            ulong workshopFileID = (ulong)button.Tag;
            var workshopItem = ((WorkshopItem) button.Parent.Parent.Parent.DataContext);
        }

        private void VotingButton_OnClick(object? sender, RoutedEventArgs e)
        {
            string[] tags = (sender as Button).Tag.ToString().Split(", ");
            bool isVoteUp = tags[0] == "Up";
            ulong workshopFileID = ulong.Parse(tags[1]);
        }

        private void SearchTextBox_OnKeyUp(object? sender, KeyEventArgs e)
        {
            var viewModel = DataContext as MainMenuViewModel;
            viewModel.FilterItemsByName(SearchTextBox.Text);
        }

        private void ImageTemplate_OnInitialized(object? sender, EventArgs e)
        {
            //TODO: Having to go through every single item again to change subscription image sucks
            //Look into how to create my DataTemplate for the ListBoxItem programatically?
            // Image image = sender as Image;
            //
            // //Not really the item itself..?
            // WorkshopItem item = image.Parent.Parent.Parent.DataContext as WorkshopItem;
            //
            // if (item.IsSubscribed)
            // {
            //     Console.WriteLine(item.DisplayName);
            //     var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            //     // image.Source = new Bitmap(assets.Open(new Uri("avares://MVVMApplication/Assets/check-solid.png")));
            // }
        }

        //https://github.com/AvaloniaUI/Avalonia/blob/c1b93d4da2006b73ccc451cea95611a252fb5888/tests/Avalonia.Controls.UnitTests/Presenters/ItemsPresenterTests_Virtualization.cs#L302
        //Or if making a template in a different xaml window.. (var template = (DataTemplate)new AvaloniaXamlLoader().Load(xaml);)
        private void WorkshopItemsListBox_OnInitialized(object? sender, EventArgs e)
        {
            ListBox listBox = (sender as ListBox);

            FuncDataTemplate<WorkshopItem> template = new FuncDataTemplate<WorkshopItem>((item, scope) =>
            {
                TextBlock textBlock = new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("DisplayName")
                };
                return textBlock;
            });

            // (sender as ListBox).DataTemplates.Add(template);
            listBox.ItemTemplate = template;
        }
    }
}