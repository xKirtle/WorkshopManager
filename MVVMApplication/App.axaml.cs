using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MVVMApplication.ViewModels;
using MVVMApplication.Views;
using SteamworksWorker;

namespace MVVMApplication
{
    public partial class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //Deploy Libs
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    ProcessStartInfo processInfo = new()
                    {
                        UseShellExecute = true,
                        FileName = "bash",
                        Arguments = "deployLibs.sh"
                    };  
                    Process.Start(processInfo).WaitForExit();
                }
                else if (OperatingSystem.IsWindows())
                {
                    ProcessStartInfo processInfo = new()
                    {
                        UseShellExecute = true,
                        FileName = "cmd.exe",
                        Arguments = "/c deployLibs.bat"
                    };
                    Process.Start(processInfo).WaitForExit();
                }
                
                Worker.InitializeSteamworks();
                
                Views.MainMenu mainMenu = new();
                MainMenuViewModel mainMenuViewModel = new();
                mainMenu.DataContext = mainMenuViewModel;
                desktop.MainWindow = mainMenu;

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    if (Worker.IsInitialized)
                        Worker.Exit();
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}