using System;
using System.Diagnostics;
using System.Windows;
using CleanUI.Interfaces;
using CleanUI.Interfaces.Implementations;
using Unity;
using Unity.Injection;

namespace CleanUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private readonly string _configPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                                              @"\AppData\Roaming\Microsoft\Windows\Start Menu\SpotlightXConfig\config\";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var settingsPath = _configPath + "settings.json";
            var container = new UnityContainer();
            container.RegisterSingleton<ISettingsLoader, FileSystemSettingsLoaderAndSaver>(
                new InjectionConstructor(settingsPath));
            container.RegisterSingleton<ISettingsSaver, FileSystemSettingsLoaderAndSaver>(
                new InjectionConstructor(settingsPath));
            container.RegisterSingleton<ICommandLoader, CommandLoader>();
            container.RegisterSingleton<IProgramListLoader, StartMenuProgramListLoader>();
            var window = container.Resolve<MainWindow>();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            MainWindow = window;
            Debug.Assert(MainWindow != null, nameof(MainWindow) + " != null");
            MainWindow.Show();
        }
    }
}
