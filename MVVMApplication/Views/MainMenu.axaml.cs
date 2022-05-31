using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
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

        private void SubscribeButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var image = button.Content as Image;
            var workshopItem = ((WorkshopItem) button.Parent.Parent.Parent.DataContext);
            workshopItem.IsSubscribed = !workshopItem.IsSubscribed;
            
            //TODO: Proper animation handling? Need a system for this..
            if (workshopItem.IsSubscribed)
                AsyncSubscribeAndDownloadMod(image, workshopItem);
            else
                AsyncUnsubscribeAndDeleteMod(image, workshopItem);
        }

        public async void AsyncSubscribeAndDownloadMod(Image image, WorkshopItem item)
        {
            // var assetsLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            // string uriPath = "avares://MVVMApplication/Assets/";
            //
            //
            // image.Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + "loading-icon.png")));
            // image.Classes.Add("downloading");
            //
            // await Task.Run(() =>
            // {
            //     //Actually subscribe/download
            //     var sw = Stopwatch.StartNew();
            //     while (sw.Elapsed.Seconds < 30) ;
            // });
            //
            // image.Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + "check-solid.png")));
            // image.Classes.Remove("downloading");
            // image.Classes.Add("subscribed");
        }

        public async void AsyncUnsubscribeAndDeleteMod(Image image, WorkshopItem item)
        {
            // var assetsLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            // string uriPath = "avares://MVVMApplication/Assets/";
            //
            // image.Classes.Remove("subscribed");
            // image.Classes.Add("removed");
            // //Waiting for the animation to play..
            // await Task.Run(() => Thread.Sleep(1000));
            // image.Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + "arrow-down-solid.png")));
            // image.Classes.Remove("removed");
            //
            // //Actually unsubscribe/remove
            // Task.Run(() =>
            // {
            //     
            // });
        }
        
        private void SearchTextBox_OnKeyUp(object? sender, KeyEventArgs e)
        {
            var viewModel = DataContext as MainMenuViewModel;
            viewModel.AsyncFilterItemsByKeyword(SearchTextBox.Text);
        }

        private void WorkshopItemsListBox_OnInitialized(object? sender, EventArgs e)
        {
            ListBox listBox = (sender as ListBox);
            var assetsLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string uriPath = "avares://MVVMApplication/Assets/";

            FuncDataTemplate<WorkshopItem> template = new FuncDataTemplate<WorkshopItem>((item, scope) =>
            {
                Grid grid = new Grid
                {
                    Height = 93,
                    Background = Brushes.Transparent
                    // ShowGridLines = true
                };
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("50")));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("92")));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                StackPanel ratingPanel = new StackPanel()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, -4, 0, 0)
                };
                
                Grid.SetRowSpan(ratingPanel, 3);
                grid.Children.Add(ratingPanel);

                Button voteUpButton = new Button()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(5, 3, 5, 0)
                };
                
                Image voteUpImage = new Image()
                {
                    Width = Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + "votingUp.png")))
                };

                voteUpButton.Content = voteUpImage;
                ratingPanel.Children.Add(voteUpButton);

                TextBlock votingRatio = new TextBlock()
                {
                    [!TextBlock.TextProperty] = new Binding("VotesRatio"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5)
                };
                
                ratingPanel.Children.Add(votingRatio);
                
                Button voteDownButton = new Button()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(5, 3, 5, 0)
                };
                
                Image voteDownImage = new Image()
                {
                    Width = Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + "votingDown.png")))
                };
                
                voteDownButton.Content = voteDownImage;
                ratingPanel.Children.Add(voteDownButton);
                
                Image icon = new Image()
                {
                    Width = Height = 80,
                    [!Image.SourceProperty] = new Binding("BitmapIcon"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 3, 0, 0)
                };
                
                Grid.SetRow(icon, 0);
                Grid.SetRowSpan(icon, 3);
                Grid.SetColumn(icon, 1);
                grid.Children.Add(icon);

                TextBlock displayName = new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("DisplayName"),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize =  16,
                    Margin = new Thickness(5, 0, 0, 2),
                };
                
                Grid.SetColumn(displayName, 2);
                grid.Children.Add(displayName);
                
                TextBlock displayNameUnderline = new TextBlock()
                {
                    Height = 2,
                    Background = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                
                Grid.SetColumn(displayNameUnderline, 2);
                grid.Children.Add(displayNameUnderline);

                DockPanel secondRowDockPanel = new DockPanel();
                Grid.SetRow(secondRowDockPanel, 1);
                Grid.SetColumn(secondRowDockPanel, 2);
                grid.Children.Add(secondRowDockPanel);
                
                Button authors = new Button()
                {
                    Height = 20,
                    Content = "Show Authors",
                    FontSize = 12,
                    Focusable = false,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                
                ToolTip toolTipAuthors = new ToolTip()
                {
                    [!ToolTip.ContentProperty] = new Binding("Authors"),
                };
                
                ToolTip.SetTip(authors, toolTipAuthors);
                ToolTip.SetPlacement(authors, PlacementMode.Pointer);
                
                secondRowDockPanel.Children.Add(authors);

                Button tags = new Button()
                {
                    Height = 20,
                    Content = "Show Tags",
                    FontSize = 12,
                    Focusable = false,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                
                ToolTip toolTipTags = new ToolTip()
                {
                    [!ToolTip.ContentProperty] = new Binding("Tags"),
                };
                
                ToolTip.SetTip(tags, toolTipTags);
                ToolTip.SetPlacement(tags, PlacementMode.Pointer);
                
                secondRowDockPanel.Children.Add(tags);

                Binding modSideBinding = new Binding("ModSide");
                modSideBinding.StringFormat = "Mod Side: {0}";
                Button modSide = new Button()
                {
                    Height = 20,
                    [!Button.ContentProperty] = modSideBinding,
                    FontSize = 12,
                    Focusable = false,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                
                secondRowDockPanel.Children.Add(modSide);
                
                Button subscribeButton = new Button()
                {
                    Width = 40,
                    Height = 40,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 30, 0, 0)
                };

                Image subscribeImage = new Image()
                {
                    Width = Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                string iconName = item.IsSubscribed ? "check-solid.png" : "arrow-down-solid.png";
                subscribeImage.Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + iconName)));

                subscribeButton.Content = subscribeImage;
                subscribeButton.Click += SubscribeButton_OnClick;
                secondRowDockPanel.Children.Add(subscribeButton);
                
                DockPanel thirdRowDockPanel = new DockPanel();
                thirdRowDockPanel.Margin = new Thickness(0, -8, 0, 0);
                Grid.SetRow(thirdRowDockPanel, 2);
                Grid.SetColumn(thirdRowDockPanel, 2);
                grid.Children.Add(thirdRowDockPanel);

                Image subsImage = new Image()
                {
                    Width = Height = 20,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + "user-solid.png"))),
                    Margin = new Thickness(5, 0, 0, 0)
                };
                
                thirdRowDockPanel.Children.Add(subsImage);

                TextBlock subsText = new TextBlock()
                {
                    [!TextBlock.TextProperty] = new Binding("Subscriptions"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                
                thirdRowDockPanel.Children.Add(subsText);
                
                Image favImage = new Image()
                {
                    Width = Height = 20,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Source = new Bitmap(assetsLoader.Open(new Uri(uriPath + "star-solid.png"))),
                    Margin = new Thickness(10, 0, 0, 0)
                };
                
                thirdRowDockPanel.Children.Add(favImage);

                TextBlock favText = new TextBlock()
                {
                    [!TextBlock.TextProperty] = new Binding("Favorites"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                
                thirdRowDockPanel.Children.Add(favText);

                return grid;
            });

            listBox.ItemTemplate = template;
        }
    }
}