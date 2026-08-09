using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace SWSMonitor.ViewModels;

public class DynamicViewModel : ViewModelBase
{
    public static DynamicViewModel? Instance;
    public event PropertyChangedEventHandler? PropertyChanged;

    // The collection holding your dynamic rows
    public ObservableCollection<IDictionary<string, object>> DataRecords { get; set; } = new();

    // Track headers so the View knows which columns to build
    public List<string> DataHeaders { get; set; } = new() {"Column 1", "Column 2", "Column 3" };
    public DynamicViewModel()
    {
        Instance = this;
    }
}
