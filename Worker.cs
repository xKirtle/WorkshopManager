using System.Diagnostics;
using Steamworks;
using SteamworksWorker.Modules;

namespace SteamworksWorker;

public class Worker
{
    private static bool _isInitialized;
    private static void Main(string[] args)
    {
        InitializeSteamworks();

        QueryInstance instance = new();
        instance.QueryAllPages();
    }

    public static void InitializeSteamworks()
    {
        //Deploy Libs
        ProcessStartInfo processInfo = new()
        {
            UseShellExecute = true,
            FileName = "bash",
            Arguments = "deployLibs.sh"
        };  
        Process.Start(processInfo).WaitForExit();
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Exit();
        
        try
        {
            if (SteamAPI.Init())
                _isInitialized = true;
            else
            {
                Debug.Print("Could not initialize the Steamworks API!");
                throw new Exception("Could not initialize the Steamworks API!");
            }
        }
        catch (DllNotFoundException e)
        {
            throw new DllNotFoundException(e.Message);
        }
    }

    public static void Exit()
    {
        if (_isInitialized)
            SteamAPI.Shutdown();
    }
}