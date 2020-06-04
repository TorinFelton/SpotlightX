using System.Diagnostics;
using System.IO;
using System.Windows;
using CleanUI.Exceptions;
using CleanUI.Interfaces;
using CleanUI.Interfaces.Implementations;
using Unity;
using Unity.Injection;

namespace CleanUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string _settingsPath = Directory.GetCurrentDirectory() + @"\config\settings.json";
        
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                var container=new UnityContainer();
                container.RegisterSingleton<ISettingsLoader, FileSystemSettingsLoaderAndSaver>(new InjectionConstructor(_settingsPath));
                container.RegisterSingleton<ISettingsSaver, FileSystemSettingsLoaderAndSaver>(new InjectionConstructor(_settingsPath));
                container.RegisterSingleton<ICommandLoader, CommandLoader>();
                container.RegisterSingleton<IProgramListLoader, StartMenuProgramListLoader>();
                var window = container.Resolve<MainWindow>();
                Current.MainWindow = window;
                Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Debug.Assert(Current.MainWindow != null, "Current.MainWindow != null");
                Current.MainWindow.Show();
            }
            catch (SettingsLoadException ex)
            {
                MessageBox.Show(
                    "Couldn't load the config/settings.json file, is it valid JSON? Re-download it or fix any JSON formatting errors. Exception: " +
                    ex);
                Current.Shutdown();
            }
        }
    }
}