
using System.Diagnostics;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CyberBank.PublisherGuard.ViewModels;

namespace CyberBank.PublisherGuard.Views;

public partial class MainView : UserControl
{
    private CancellationTokenSource? buildCancellation;

    public MainView()
    {
        InitializeComponent();
    }

    private async void ChooseLocalPackageAsync(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var sourceFolders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose the local source package folder",
            AllowMultiple = false
        });

        if (sourceFolders.Count == 0)
        {
            return;
        }

        var outputFolders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose a separate local delivery output parent",
            AllowMultiple = false
        });

        if (outputFolders.Count == 0)
        {
            return;
        }

        viewModel.Prepare(sourceFolders[0].Path.LocalPath, outputFolders[0].Path.LocalPath);
    }

    private async void BuildCleanDelivery(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || string.IsNullOrWhiteSpace(viewModel.SourceRootDisplay)
            || viewModel.SourceRootDisplay == "No package selected")
        {
            return;
        }

        var sourceName = Path.GetFileName(Path.TrimEndingDirectorySeparator(viewModel.SourceRootDisplay));
        buildCancellation?.Cancel();
        buildCancellation?.Dispose();
        buildCancellation = new CancellationTokenSource();

        try
        {
            await viewModel.TryBuildCleanDeliveryAsync($"{sourceName}-clean", buildCancellation.Token);
        }
        finally
        {
            buildCancellation.Dispose();
            buildCancellation = null;
        }
    }

    private void CancelCurrentBuild(object? sender, RoutedEventArgs e)
    {
        buildCancellation?.Cancel();
    }

    private static void OpenDeliveryFolder(MainViewModel viewModel)
    {
        if (viewModel.HasDeliveryOutput && Directory.Exists(viewModel.DeliveryOutputRoot))
        {
            Process.Start(new ProcessStartInfo(viewModel.DeliveryOutputRoot) { UseShellExecute = true });
        }
    }

    private void ExcludeIssue(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: IssueRow issue } || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.Exclude(issue.RelativePath, "Creator excluded this item from the local delivery package.");
    }

    private void RememberIssue(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: IssueRow issue } || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.RememberIgnoreRule(issue.RelativePath, "Creator keeps this item out of future local deliveries.");
    }

    private void ViewIssueDetails(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: IssueRow issue } && DataContext is MainViewModel viewModel)
        {
            viewModel.SelectIssue(issue);
        }
    }

    private void RunPrimaryAction(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        if (viewModel.ActiveStage == "CHOOSE PACKAGE")
        {
            ChooseLocalPackageAsync(sender, e);
        }
        else if (viewModel.ActiveStage == "CREATE DELIVERY")
        {
            BuildCleanDelivery(sender, e);
        }
        else if (viewModel.ActiveStage == "DELIVERY COMPLETE")
        {
            OpenDeliveryFolder(viewModel);
        }
        else if (viewModel.Issues.Count > 0)
        {
            viewModel.SelectIssue(viewModel.Issues[0]);
        }
    }
}
