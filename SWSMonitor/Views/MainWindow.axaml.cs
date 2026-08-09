using Avalonia.Controls;
using SWSMonitor.ViewModels;
using System;

namespace SWSMonitor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel mwvm)
            {
                mwvm.IsPopupOpen = false;
                //this.MyPopup.IsOpen = false;
            }
        }

        private void TopListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel mwvm)
            {
                if (sender is ListBox lb && lb.SelectedItem is MenuItemViewModel mivm)
                {
                    mwvm.SelectedBottomMenuItem = null;
                    mwvm.OnSelectedMenuItemChanged(mivm);
                }
            }

        }
        private void BottomListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel mwvm)
            {
                if (sender is ListBox lb && lb.SelectedItem is MenuItemViewModel mivm)
                {
                    mwvm.SelectedMenuItem = null;
                    mwvm.OnSelectedBottomMenuItemChanged(mivm);
                }
            }

        }
    }
}