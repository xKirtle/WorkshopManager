using System.Net;

namespace SteamworksWorker;

public class WorkshopItem
{
    public ulong WorkshopFileID { get; private set; }
    public string DisplayName { get; private set; }
    public string Authors { get; set; }
    public ulong[] WorkshopDependencies { get; private set; }
    public uint[] VotesUpAndDown { get; private set; }
    public DateTime LastUpdate { get; private set; }
    public string ShortDescription { get; private set; }
    public dynamic BitmapIcon { get; set; }
    public string Tags { get; private set; }
    public ulong Subscriptions { get; private set; }
    public ulong Favorites { get; private set; }
    public bool IsSubscribed { get; private set; }
    public string ModLoaderVersion { get; private set; }
    public ModSide ModSide { get; private set; }

    public WorkshopItem(ulong workshopFileId, string displayName, string authors, ulong[] workshopDependencies, 
        uint[] votesUpAndDown, DateTime lastUpdate, string shortDescription, dynamic bitmapIcon, string tags, 
        ulong subscriptions, ulong favorites, bool isSubscribed, string modLoaderVersion, ModSide modSide)
    {
        WorkshopFileID = workshopFileId;
        DisplayName = displayName;
        Authors = authors;
        WorkshopDependencies = workshopDependencies;
        VotesUpAndDown = votesUpAndDown;
        LastUpdate = lastUpdate;
        ShortDescription = shortDescription;
        BitmapIcon = bitmapIcon;
        Tags = tags;
        Subscriptions = subscriptions;
        Favorites = favorites;
        IsSubscribed = isSubscribed;
        ModLoaderVersion = modLoaderVersion;
        ModSide = modSide;
        
        //So we can later convert it into bitmap and render it
        DownloadImage();
    }
    
    private async void DownloadImage()
    {
        using (WebClient client = new WebClient())
        {
            client.DownloadDataAsync(new Uri(BitmapIcon));
            client.DownloadDataCompleted += DownloadComplete;
        }
    }
    
    private void DownloadComplete(object sender, DownloadDataCompletedEventArgs e)
    {
        try
        {
            byte[] bytes = e.Result;
            Stream stream = new MemoryStream(bytes);
            BitmapIcon = stream;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

public enum ModSide
{
    Both,
    Client, 
    Server,
    NoSync
}