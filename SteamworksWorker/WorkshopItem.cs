using System.Net;
using SteamworksWorker.Modules;

namespace SteamworksWorker;

public class WorkshopItem
{
    public ulong WorkshopFileID { get; private set; }
    public string DisplayName { get; private set; }
    public string Authors { get; private set; }
    public ulong[] WorkshopDependencies { get; private set; }
    public int VotesRatio { get; private set; }
    public DateTime LastUpdate { get; private set; }
    public string ShortDescription { get; private set; }
    public string IconUri { get; set; }
    public dynamic BitmapIcon { get; set; }
    public string Tags { get; private set; }
    public ulong Subscriptions { get; private set; }
    public ulong Favorites { get; private set; }
    public bool IsSubscribed { get; set; }
    public string ModLoaderVersion { get; private set; }
    public ModSide ModSide { get; private set; }
    public bool IsDownloading { get; set; }

    public WorkshopItem(ulong workshopFileId, string displayName, string authors, ulong[] workshopDependencies, 
        int votesRatio, DateTime lastUpdate, string shortDescription, string iconUri, string tags, 
        ulong subscriptions, ulong favorites, bool isSubscribed, string modLoaderVersion, ModSide modSide)
    {
        WorkshopFileID = workshopFileId;
        DisplayName = displayName;
        Authors = authors;
        WorkshopDependencies = workshopDependencies;
        VotesRatio = votesRatio;
        LastUpdate = lastUpdate;
        ShortDescription = shortDescription;
        IconUri = iconUri;
        Tags = tags != "" ? tags : "No tags";
        Subscriptions = subscriptions;
        Favorites = favorites;
        IsSubscribed = isSubscribed;
        ModLoaderVersion = modLoaderVersion;
        ModSide = modSide;
    }
}