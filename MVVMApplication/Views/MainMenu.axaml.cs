using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
using AvaloniaAccordion;
using DynamicData;
using DynamicData.Kernel;
using MVVMApplication.Utils;
using MVVMApplication.ViewModels;
using SteamworksWorker;
using SteamworksWorker.Modules;

namespace MVVMApplication.Views
{
    public partial class MainMenu : Window
    {
        private Animation _downloadAnimation;
        public MainMenu()
        {
            _downloadAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(1.5),
                IterationCount = new IterationCount(3),
                Easing = new SineEaseInOut(),
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0),
                        Setters =
                        {
                            new Setter(RotateTransform.AngleProperty, 0)
                        }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(1.5),
                        Setters =
                        {
                            new Setter(RotateTransform.AngleProperty, 360)
                        }
                    }
                }
            };
            
            InitializeComponent();
        }

        private void FiltersListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as MainMenuViewModel;
        }

        private void SubscribeButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainMenuViewModel;
            var button = sender as Button;
            var image = button.Content as Image;
            var item = ((WorkshopItem) button.Parent.Parent.Parent.DataContext);
            item.IsSubscribed = !item.IsSubscribed;

            if (item.IsSubscribed)
            {
                //Unsubscribe from event until it's finished (avoid spam)
                button.Click -= SubscribeButton_OnClick;

                //Set download animation
                item.IsDownloading = true;
                image.Classes.Add("downloading");
                image.Source = Helpers.ImageNameToBitmap("loading-icon");

                //Request download and send callback to remove animation and set it as completed
                viewModel.AsyncSubscribeAndDownloadWorkshopItem(FinishAsyncDownload);
            }
            else
            {
                viewModel.AsyncUnsubscribeAndRemoveWorkshopItem(item);
            }

            async void FinishAsyncDownload()
            {
                item.IsDownloading = false;
                image.Classes.Remove("downloading");
                image.Source = Helpers.ImageNameToBitmap("check-solid");
                
                //Play little animation
                image.Classes.Add("finishedDownload");
                await Task.Run(() =>
                {
                    //Animation lasts .5 seconds
                    var sw = Stopwatch.StartNew();
                    while (sw.Elapsed.Seconds < 1) ;
                    image.Classes.Remove("finishedDownload");
                });
                button.Click += SubscribeButton_OnClick;
            }
        }

        private void SearchTextBox_OnKeyUp(object? sender, KeyEventArgs e)
        {
            var viewModel = DataContext as MainMenuViewModel;
            viewModel.AsyncFilterItemsByKeyword(SearchTextBox.Text);
        }

        private void WorkshopItemsListBox_OnInitialized(object? sender, EventArgs e)
        {
            ListBox listBox = (sender as ListBox);
            
            //Template runs everytime a new item comes on screen due to Data Virtualization for performance
            FuncDataTemplate<WorkshopItem> template = new FuncDataTemplate<WorkshopItem>((item, scope) =>
            {
                Grid grid = new Grid
                {
                    Height = 93,
                    Background = Brushes.Transparent
                    // ShowGridLines = true
                };
                
                #region Main Grid Definition
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("50")));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("92")));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                #endregion
                
                #region Ratings
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
                    Source = Helpers.ImageNameToBitmap("votingUp")
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
                    Source = Helpers.ImageNameToBitmap("votingDown")
                };
                
                voteDownButton.Content = voteDownImage;
                ratingPanel.Children.Add(voteDownButton);
                #endregion

                #region Icon and Display Name
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
                #endregion
                
                #region Second Row Grid
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
                
                //TODO: Due to virtualization, animations break because the ListBoxItem simply is removed..
                //Either keep track of the animation states or.. don't animate at all?
                //Item could be mid download..
                if (item.IsDownloading)
                {
                    subscribeImage.Source = Helpers.ImageNameToBitmap("loading-icon");
                    subscribeImage.Classes.Add("downloading");
                }
                else
                {
                    string iconName = item.IsSubscribed ? "check-solid" : "arrow-down-solid";
                    subscribeImage.Source = Helpers.ImageNameToBitmap(iconName);   
                }

                subscribeButton.Content = subscribeImage;
                subscribeButton.Click += SubscribeButton_OnClick;
                secondRowDockPanel.Children.Add(subscribeButton);
                #endregion
                
                #region Third Row Grid
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
                    Source = Helpers.ImageNameToBitmap("user-solid"),
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
                    Source = Helpers.ImageNameToBitmap("star-solid"),
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
                #endregion
                
                return grid;
            });
            
            //TODO: Implement my own ScrollViewer to set an amount for each scroll..
            //https://stackoverflow.com/questions/1639505/wpf-scrollviewer-scroll-amount
            //https://stackoverflow.com/a/42621035
            listBox.ItemContainerGenerator.ItemTemplate = template;
            listBox.VirtualizationMode = ItemVirtualizationMode.Simple;
            // listBox.ItemTemplate = template;
        }
    }
}