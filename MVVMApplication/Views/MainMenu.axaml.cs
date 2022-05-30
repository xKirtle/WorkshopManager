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
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
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
                Grid grid = new Grid
                {
                    Height = 93,
                    Background = Brushes.Transparent,
                    ShowGridLines = true,
                };
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("50")));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("92")));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                Image icon = new Image()
                {
                    Width = Height = 80,
                    [!Image.SourceProperty] = new Binding("BitmapIcon"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
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

                Button authors = new Button()
                {
                    Height = 20,
                    Content = "Show Authors",
                    FontSize = 12,
                    Focusable = false,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                
                authors.Click += (sender, args) =>
                {
                    //TODO: Show tooltip
                    ToolTip toolTip = new ToolTip()
                    {
                        [!ToolTip.ContentProperty] = new Binding("Authors"),
                        
                    };
                    ToolTip.SetTip(authors, toolTip.Content);
                    ToolTip.SetIsOpen(toolTip, true);
                };

                Grid.SetRow(authors, 1);
                Grid.SetColumn(authors, 2);
                grid.Children.Add(authors);
                
                return grid;
            });

            // (sender as ListBox).DataTemplates.Add(template);
            listBox.ItemTemplate = template;
        }
    }
}