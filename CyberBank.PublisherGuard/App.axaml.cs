using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CyberBank.PublisherGuard.Application.Delivery;
using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Infrastructure.Delivery;
using CyberBank.PublisherGuard.Infrastructure.Packages;
using CyberBank.PublisherGuard.ViewModels;
using CyberBank.PublisherGuard.Views;

namespace CyberBank.PublisherGuard;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splash = new SplashWindow();
            splash.Show();

            var startupTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1600) };
            startupTimer.Tick += (_, _) =>
            {
                startupTimer.Stop();
                var ignoreRuleStatePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CyberBank",
                    "Publisher Guard",
                    "ignore-rules.json");
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(
                        new PackageWorkflowCoordinator(new LocalPackageInventory(), new JsonLocalIgnoreRuleStore(ignoreRuleStatePath)),
                        new LocalCleanPackageBuilder())
                };
                desktop.MainWindow.Show();
                splash.Close();
            };
            startupTimer.Start();
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView { DataContext = new MainViewModel() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
