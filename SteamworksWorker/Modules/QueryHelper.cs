using System.Diagnostics;
using Steamworks;

namespace SteamworksWorker.Modules;

internal static class QueryHelper
{
    internal const uint TMLAppID = 1281930;
    internal static AppId_t TMLAppID_t = new(TMLAppID);
    
    internal static string GetCurrentSteamLangKey()
    {
        string[] supportedLangagues = new[]
            {"german", "italian", "french", "spanish", "russian", "schinese", "portuguese", "polish"};

        string lang = SteamApps.GetCurrentGameLanguage();
        return supportedLangagues.Contains(lang) ? lang : "english";
    }
    
    internal static string[] MetadataKeys = new string[7]
    {
        "name", "author", "modside", "homepage", "modloaderversion", "version", "modreferences"
    };
    
    internal static bool HandleError(EResult eResult)
    {
        if (eResult == EResult.k_EResultOK || eResult == EResult.k_EResultNone) return true;

        var error = $"Error: Unable to access Steam Workshop. {eResult}";

        switch (eResult)
        {
            case EResult.k_EResultAccessDenied:
                error = "Error: Access to Steam Workshop was denied.";
                break;
            case EResult.k_EResultTimeout:
                error = "Error: Operation Timed Out. No callback received from Steam Servers.";
                break;
        }

        Console.WriteLine(error);
        return false;
    }

    internal static Dictionary<QueryType, EUGCQuery> queryTypeToUGCIndex = new ()
    {
        {QueryType.MostVoted, EUGCQuery.k_EUGCQuery_RankedByVote},
        {QueryType.MostRecent, EUGCQuery.k_EUGCQuery_RankedByPublicationDate},
        {QueryType.LastUpdated, EUGCQuery.k_EUGCQuery_RankedByLastUpdatedDate},
        {QueryType.MostSubscribed, EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions},
        {QueryType.FavoritedByFriends, EUGCQuery.k_EUGCQuery_FavoritedByFriendsRankedByPublicationDate},
        {QueryType.MostPopular, EUGCQuery.k_EUGCQuery_RankedByTrend}
    };
}