using Avalonia.Controls;
using Avalonia.Interactivity;
using SWSMonitor.ViewModels;

namespace SWSMonitor
{
    public partial class MainView : UserControl
    {
        public static MainView ViewInstance { get; private set; }
        public static MainViewModel ViewModelInstance { get; private set; }

        public MainView()
        {
            InitializeComponent();
            ViewInstance = this;
        }

        protected override async void OnLoaded(RoutedEventArgs args)
        {
            // Always call the base method first to raise the Loaded event
            base.OnLoaded(args);

            SetPageSize();

            ViewModelInstance = this.DataContext as MainViewModel;
            if (ViewModelInstance != null)
            {
                await ViewModelInstance.InitializeAsync();
            }
        }
        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            var newWidth = e.NewSize.Width < 400? 400 : e.NewSize.Width;
            var newHeight = e.NewSize.Height < 400 ? 400 : e.NewSize.Height;

            // Update your responsive logic here
        }

        public void SetBusy(bool isBusy)
        {
            this.Cursor = isBusy
                ? new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Wait)
                : Avalonia.Input.Cursor.Default;
        }


        public void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mwvm)
            {
                mwvm.IsPopupOpen = false;
                //this.MyPopup.IsOpen = false;
            }
        }

        private void PaneToggle_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mwvm)
            {
                mwvm.TogglePane();
            }
        }

        private void TopListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
//            if (this.DataContext is MainViewModel mwvm)
//            {
//                if (sender is ListBox lb && lb.SelectedItem is BrowserItemViewModel mivm)
//                {
//                    mwvm.SelectedBottomMenuItem = null;
////                    mwvm.OnSelectedMenuItemChanged(mivm);
//                }
//            }

        }
        private void BottomListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
//            if (this.DataContext is MainViewModel mwvm)
//            {
//                if (sender is ListBox lb && lb.SelectedItem is BrowserItemViewModel mivm)
//                {
//                    mwvm.SelectedMenuItem = null;
////                    mwvm.OnSelectedBottomMenuItemChanged(mivm);
//                }
//            }

        }

        internal void SetPageSize()
        {
            var topLevel = TopLevel.GetTopLevel(this.OuterView);
            topLevel.SizeChanged += TopLevel_SizeChanged;
            if (topLevel != null)
            {
                var width = topLevel.ClientSize.Width >= 400 ? topLevel.ClientSize.Width : 400;
                var height = topLevel.ClientSize.Height >= 400 ? topLevel.ClientSize.Height : 400;
                this.Width = width * 0.9;
                this.Height = height * 0.9;
            }

        }

        private void TopLevel_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            this.Width = e.NewSize.Width * 0.9;
            this.Height = e.NewSize.Height * 0.9;

        }
    }
}