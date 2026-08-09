using Avalonia.Controls;
using ReactiveUI;
using SWSMonitor.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SWSMonitor;

public class AppViewLocator : ReactiveUI.IViewLocator
{
    public IViewFor<TViewModel>? ResolveView<TViewModel>() where TViewModel : class
    {
        return ResolveView<TViewModel>(null);
    }

    public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract) where TViewModel : class
    {
        // For compile-time resolution without an instance, try to create a default instance
        var viewModelType = typeof(TViewModel);
        try
        {
            var instance = Activator.CreateInstance(viewModelType);
            var view = ResolveViewForInstance(instance, contract);
            return view as IViewFor<TViewModel>;
        }
        catch
        {
            // Cannot create instance without parameters, return null
            return null;
        }
    }

    [RequiresUnreferencedCode("This method uses reflection to determine the view model type at runtime, which may be incompatible with trimming.")]
    [RequiresDynamicCode("Trimming can't validate that the requirements of those annotations are met.")]
    public IViewFor? ResolveView(object? instance)
    {
        return ResolveView(instance, null);
    }

    [RequiresUnreferencedCode("This method uses reflection to determine the view model type at runtime, which may be incompatible with trimming.")]
    [RequiresDynamicCode("Trimming can't validate that the requirements of those annotations are met.")]
    public IViewFor? ResolveView(object? instance, string? contract)
    {
        if (instance is not ViewModelBase)
            throw new Exception("Must use ViewModelBase");

        return ResolveViewForInstance(instance, contract);
    }

    private IViewFor? ResolveViewForInstance(object? viewModel, string? contract)
    {
        if (viewModel is null)
            throw new ArgumentNullException(nameof(viewModel));

        var viewModelName = viewModel.GetType()!.FullName;
        if (string.IsNullOrEmpty(viewModelName) || !viewModelName.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("ViewModel must follow convention <viewname>ViewModel.", nameof(viewModel));
        var viewName = viewModelName.Replace("ViewModel", "View", StringComparison.OrdinalIgnoreCase);
        var viewType = Type.GetType(viewName);
        if (viewType is null)
        {
            // Might be at the root namespace
            viewName = viewName.Replace(".Views", "", StringComparison.OrdinalIgnoreCase);
            viewType = Type.GetType(viewName);
            if (viewType is null)
                throw new ArgumentException($"View not found for ViewModel: {viewModelName}", nameof(viewModel));
        }

        var view = (Control)Activator.CreateInstance(viewType);
        if (view == null)
            throw new InvalidOperationException($"Could not create view instance for type: {viewType.FullName}");
        view.DataContext = viewModel;
        return view as IViewFor;
    }
}