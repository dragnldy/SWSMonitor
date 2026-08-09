using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SWSMonitor.ViewModels;

public class ErrorsViewModel : INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _propertyErrors = new Dictionary<string, List<string>>();

    public bool HasErrors => _propertyErrors.Any();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public IEnumerable GetErrors(string propertyName)
    {
        return _propertyErrors.GetValueOrDefault(propertyName, null);
    }

    public void AddError(string propertyName, string errorMessage)
    {
        if (!_propertyErrors.ContainsKey(propertyName))
        {
            _propertyErrors.Add(propertyName, new List<string>());
        }

        _propertyErrors[propertyName].Add(errorMessage);
        OnErrorsChanged(propertyName);
    }

    public void ClearErrors(string propertyName)
    {
        if (_propertyErrors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }
    internal void ClearErrors()
    {
        // Clear all errors for all properties
        List<string> propertiesWithErrors = _propertyErrors.Keys.ToList();

        foreach (string key in propertiesWithErrors)
        {
            ClearErrors(key);
        }
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

}
