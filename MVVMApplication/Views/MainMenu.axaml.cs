using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
            ulong workshopFileID = (ulong)(sender as Button).Tag;
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
    }
}