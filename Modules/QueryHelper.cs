using Steamworks;

namespace SteamworksWorker.Modules;

public static class QueryHelper
{
    public static string GetCurrentSteamLangKey()
    {
        string[] supportedLangagues = new[]
            {"german", "italian", "french", "spanish", "russian", "schinese", "portuguese", "polish"};

        string lang = SteamApps.GetCurrentGameLanguage();
        return supportedLangagues.Contains(lang) ? lang : "english";
    }
}