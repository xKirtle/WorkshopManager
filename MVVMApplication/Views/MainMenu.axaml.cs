using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using MVVMApplication.ViewModels;

namespace MVVMApplication.Views
{
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
            LazyScrolling();
        }

        private readonly CompositeDisposable _disposables = new();
        private CompositeDisposable _scrollViewerDisposables;
        private double _verticalHeightMax;
        private void LazyScrolling()
        {
            var listBox = this.FindControl<ListBox>("WorkshopItemsListBox");
            listBox.GetObservable(ListBox.ScrollProperty)
                .OfType<ScrollViewer>().Take(1).Subscribe(sv =>
                {
                    _scrollViewerDisposables?.Dispose();
                    _scrollViewerDisposables = new CompositeDisposable();

                    sv.GetObservable(ScrollViewer.VerticalScrollBarMaximumProperty)
                        .Subscribe(newMax => _verticalHeightMax = newMax)
                        .DisposeWith(_scrollViewerDisposables);


                    sv.GetObservable(ScrollViewer.OffsetProperty).Subscribe(offset =>
                    {
                        var delta = Math.Abs(_verticalHeightMax - offset.Y);
                        //At 85% of the scrollbar, ask for more items
                        if (delta <= WorkshopItemsListBox.ItemCount * 0.15)
                        {
                            //Setting the data context of this window as if
                            //it was our view model and adding more items
                            var viewModel = DataContext as MainMenuViewModel; 
                            //Check if viewModel == null? Initialized in App code behind..
                            if (!viewModel.IsAddingItems)
                            {
                                Debug.Print($"Requesting Items! Current: {viewModel?.ResultItems.Count}");
                                viewModel.AsyncAddItems();
                            }
                        }
                    }).DisposeWith(_disposables);
                }).DisposeWith(_disposables);
        }
    }
}