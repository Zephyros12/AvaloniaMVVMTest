using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaMVVMTest.ViewModels;
using AvaloniaMVVMTest.Views;
using Autofac;
using System;
using System.Threading.Tasks;

namespace AvaloniaMVVMTest
{
    public sealed class App : Application
    {
        public static IContainer Container { get; private set; } = default!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine("UnhandledException:");
                Console.WriteLine(e.ExceptionObject?.ToString());
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Console.WriteLine("UnobservedTaskException:");
                Console.WriteLine(e.Exception?.ToString());
                e.SetObserved();
            };

            Container = AppBootstrapper.Build();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                using var scope = Container.BeginLifetimeScope();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = scope.Resolve<MainWindowViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
