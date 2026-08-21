using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SWSMonitor.ViewModels;
using System;
using System.Runtime.InteropServices.JavaScript;

namespace SWSMonitor;
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
            NativeMethods.HideSpinner();
            // Always call the base method first to raise the Loaded event
            base.OnLoaded(args);

            SetPageSize();

            ViewModelInstance = this.DataContext as MainViewModel;
            if (ViewModelInstance != null)
            {
                await ViewModelInstance.InitializeAsync();
            }
            NativeMethods.HideSpinner();
    }
    protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            var newWidth = e.NewSize.Width < 400? 400 : e.NewSize.Width;
            var newHeight = e.NewSize.Height < 400 ? 400 : e.NewSize.Height;

            SetMaxSize(newWidth,newHeight);

            //PixelPoint topLeftCorner = this.Position;

            (double xCoord, double yCoord) = GetBrowserCoordinates();

            // Update your responsive logic here
        }

        private void SetMaxSize(double newWidth, double newHeight)
        {
            var topLevel = TopLevel.GetTopLevel(this.OuterView);
            // Translates the control's top-left corner (0,0) relative to the window/toplevel root
            Point? relativePoint = this.TranslatePoint(new Point(0, 0), topLevel);

            if (relativePoint.HasValue)
            {
                MainViewModel.XOffsetMainView = relativePoint.Value.X;
                MainViewModel.YOffsetMainView = relativePoint.Value.Y;
            }

            if (topLevel != null)
            {
                this.Width = topLevel.ClientSize.Width >= 1200 ? 1200 : topLevel.ClientSize.Width;
                this.Height = topLevel.ClientSize.Height >= 800 ? 800 : topLevel.ClientSize.Height;
            }

        }

    public void SetBusy(bool isBusy)
    {
        if (isBusy)
        {
            NativeMethods.ShowSpinner();
        }
        else
        {
            NativeMethods.HideSpinner();
        }    
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
            if (topLevel != null)
            {
                topLevel.SizeChanged += TopLevel_SizeChanged;

                // Get screen dimensions
                var screen = topLevel!.Screens!.Primary;
                if (screen != null)
                {
                    var screenWidth = screen.Bounds.Width;
                    var screenHeight = screen.Bounds.Height;
                    SetMaxSize(screenWidth, screenHeight);

                }
            }
            ////var width = topLevel.ClientSize.Width >= 1200 ? topLevel.ClientSize.Width : 1200;
            ////var height = topLevel.ClientSize.Height >= 900 ? topLevel.ClientSize.Height : 900;
            //this.Width = topLevel.ClientSize.Width >= 1200 ? 1200 : topLevel.ClientSize.Width;
            //this.Height = topLevel.ClientSize.Height >= 900 ? 900 : topLevel.ClientSize.Height;
            //var width = 1100;
            //var height = 900;
            //this.Width = width * .9;
            //this.Height = height * .9;
        }

    private void TopLevel_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        SetMaxSize(e.NewSize.Width, e.NewSize.Height);

    }
    public static (double, double) GetBrowserCoordinates()
        {
            // Ensure this only executes when running on WebAssembly
            if (OperatingSystem.IsBrowser())
            {
            // Canvas always appears to be at (0,0) in the browser, so we will get the position of the native host control

            // Assuming your canvas container ID is 'out' (the default template name)
            
            //var coords = NativeMethods.GetCanvasPosition("avalonia-native-host");

            //    double xCoord = coords.GetPropertyAsDouble("x");
            //    double yCoord = coords.GetPropertyAsDouble("y");
            //    return(xCoord, yCoord);
            }
            return (0, 0);
        }
}
// Partial class handling the JSImport setup (.NET 7+)
internal static partial class NativeMethods
{
    [JSImport("globalThis.getCanvasCoordinates")]
    internal static partial JSObject GetCanvasPosition(string elementId);

    [JSImport("globalThis.repositionDivRelative")]
    internal static partial JSObject RepositionDivRelative(string elementId, int deltaX, int deltaY);

    [JSImport("globalThis.hideSpinner")]
    internal static partial JSObject HideSpinner();

    [JSImport("globalThis.showSpinner")]
    internal static partial JSObject ShowSpinner();
}