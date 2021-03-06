using Steamworks;
using SteamworksWorker.Modules;

namespace SteamworksWorker;

public static class Worker
{
    public static bool IsInitialized { get; private set; }

    public static void InitializeSteamworks()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Exit();
        
        try
        {
            if (SteamAPI.Init())
                IsInitialized = true;
            else
            {
                Console.WriteLine("Could not initialize the Steamworks API!");
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
        if (IsInitialized)
            SteamAPI.Shutdown();
    }
}