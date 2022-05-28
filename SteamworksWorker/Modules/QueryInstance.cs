using System.Collections.Specialized;
using System.Diagnostics;
using Steamworks;

namespace SteamworksWorker.Modules;

public sealed class QueryInstance
{
    private EResult _errorState;
    private List<WorkshopItem> _items = new (Constants.k_unEnumeratePublishedFilesMaxResults);
    private uint _totalItemsMatchingQuery;
    private uint _totalItemsQueried;
    private UGCQueryHandle_t _ugcQueryHandle;
    private EResult _ugcQueryResultState;
    private uint _ugcQueryResultNumItems;
    private string _nextCursor;
    private readonly CallResult<SteamUGCQueryCompleted_t> _queryHook;
    private const EUGCMatchingUGCType QueryResultType = EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items;

    //Fields that can be changed
    private EUGCQuery _queryType;
    private uint _playtimeStats;
    private string _searchText;
    private Action<WorkshopItem> _onItemHandled;

    public QueryInstance(QueryType queryType = QueryType.MostVoted, uint playtimeStats = 30, 
        string searchText = null, Action<WorkshopItem> onItemHandled = default)
    {
        _queryHook = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);

        _queryType = QueryHelper.queryTypeToUGCIndex[queryType];
        _playtimeStats = Math.Clamp(playtimeStats, 1, 365);
        _searchText = searchText ?? String.Empty;
        _onItemHandled = onItemHandled;
    } 
    
    private void OnQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
    {
        _ugcQueryHandle = pCallback.m_handle;
        _ugcQueryResultState = pCallback.m_eResult;
        _ugcQueryResultNumItems = pCallback.m_unNumResultsReturned;
        _nextCursor = pCallback.m_rgchNextCursor;

        if (_totalItemsMatchingQuery == 0 && pCallback.m_unTotalMatchingResults > 0)
            _totalItemsMatchingQuery = pCallback.m_unTotalMatchingResults;
    }

    public List<WorkshopItem> RetrieveItemsList() => _items;
    
    private void ReleaseQuery() => SteamUGC.ReleaseQueryUGCRequest(_ugcQueryHandle);

    public void QueryAllPages()
    {
        do
        {
            QueryNextPage();

            if (!QueryHelper.HandleError(_errorState))
                return;
        }
        while (_totalItemsMatchingQuery != _totalItemsQueried);
    }
    
    public void QueryNextPage()
    {
        if (_totalItemsMatchingQuery != 0 && _totalItemsMatchingQuery == _totalItemsQueried) return;

        _ugcQueryResultState = EResult.k_EResultNone;
        UGCQueryHandle_t qHandle =
            SteamUGC.CreateQueryAllUGCRequest(_queryType, QueryResultType, 
                QueryHelper.TMLAppID_t, QueryHelper.TMLAppID_t, _nextCursor);

        SteamUGC.SetLanguage(qHandle, QueryHelper.GetCurrentSteamLangKey());
        SteamUGC.SetReturnKeyValueTags(qHandle, true); //Workshop tags
        SteamUGC.SetAllowCachedResponse(qHandle, 0);
        SteamUGC.SetReturnChildren(qHandle, true);
        SteamUGC.SetSearchText(qHandle, _searchText);
        
        //TODO: Add other api calls that we'd like the qHandle to retrieve
        
        _queryHook.Set(SteamUGC.SendQueryUGCRequest(qHandle));

        //Validate connection to workshop
        var sw = Stopwatch.StartNew();

        do
        {
            if (sw.Elapsed.TotalSeconds >= 10)
                _ugcQueryResultState = EResult.k_EResultTimeout;

            SteamAPI.RunCallbacks();
        } while (_ugcQueryResultState == EResult.k_EResultNone);

        if (_ugcQueryResultState != EResult.k_EResultOK)
        {
            _errorState = _ugcQueryResultState;
            Debug.Print("Error: No connection to the workshop?");
            ReleaseQuery();
            return;
        }

        for (uint i = 0; i < _ugcQueryResultNumItems; i++)
        {
            //Item result call data
            SteamUGCDetails_t pDetails;
            SteamUGC.GetQueryUGCResult(_ugcQueryHandle, i, out pDetails);

            //Skip over any non visible or failed queries
            if (pDetails.m_eVisibility !=
                ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic
                || pDetails.m_eResult != EResult.k_EResultOK)
                continue;
            

            //Retrieve the data we want from the workshop item
            ulong workshopFileId = pDetails.m_nPublishedFileId.m_PublishedFileId;
            string displayName = pDetails.m_rgchTitle;
            string[] authors;
            ulong[] workshopDependencies;
            uint[] votesUpAndDown = new[] {pDetails.m_unVotesUp, pDetails.m_unVotesDown};
            DateTime lastUpdate = DateTimeOffset.FromUnixTimeSeconds(pDetails.m_rtimeUpdated).UtcDateTime;
            string shortDescription = pDetails.m_rgchDescription;
            string modIconURL;
            string[] tags = pDetails.m_rgchTags.Split(",");
            ulong subscriptions;
            ulong favorites;
            bool isSubscribed = (SteamUGC.GetItemState(pDetails.m_nPublishedFileId) & (ulong)EItemState.k_EItemStateSubscribed) == 1;
            string modloaderVersion;
            
            PublishedFileId_t[] itemDependencies = new PublishedFileId_t[32]; //No item should have more than 32 dependencies? lol
            if (!SteamUGC.GetQueryUGCChildren(qHandle, i, itemDependencies, (uint) itemDependencies.Length))
                Debug.Print($"Error: Could not retrieve mod dependencies for Mod {displayName}");
            workshopDependencies = Array.ConvertAll(itemDependencies, input => input.m_PublishedFileId);

            if (!SteamUGC.GetQueryUGCPreviewURL(_ugcQueryHandle, i, out modIconURL, 1000))
                Debug.Print($"Error: Could not retrieve icon preview for Mod {displayName}");
            
            if (!SteamUGC.GetQueryUGCStatistic(qHandle, i,
                    EItemStatistic.k_EItemStatistic_NumSubscriptions, out subscriptions))
                Debug.Print($"Error: Could not retrieve number of subscriptions for Mod {displayName}");
            
            if (!SteamUGC.GetQueryUGCStatistic(qHandle, i,
                    EItemStatistic.k_EItemStatistic_NumFavorites, out favorites))
                Debug.Print($"Error: Could not retrieve number of favorites for Mod {displayName}");
            
            //Metadata stuff
            NameValueCollection metadata = new();
            uint tagsKeyCount = SteamUGC.GetQueryUGCNumKeyValueTags(_ugcQueryHandle, i);
            
            //if (keyCount < MetadataKeys.Length) -> error?
            for (uint j = 0; j < tagsKeyCount; j++)
            {
                string key, value;
                SteamUGC.GetQueryUGCKeyValueTag(qHandle, i, j, 
                    out key, Constants.k_cubUFSTagTypeMax, out value, Constants.k_cubUFSTagValueMax);

                metadata[key] = value;
            }
            //Should I check if these keys are the correct ones? They might end up changing in tModLoader..

            authors = metadata["author"].Split(", ");
            modloaderVersion = metadata["modloaderversion"];
            
            ModSide modSide = ModSide.Both;
            switch (metadata["modside"])
            {
                case "Client":
                    modSide = ModSide.Client;
                    break;
                case "Server":
                    modSide = ModSide.Server;
                    break;
                case "NoSync":
                    modSide = ModSide.NoSync;
                    break;
            }

            WorkshopItem item = new WorkshopItem(workshopFileId, displayName, authors, workshopDependencies,
                votesUpAndDown, lastUpdate, shortDescription, modIconURL, tags, subscriptions,
                favorites, isSubscribed, modloaderVersion, modSide);
            _items.Add(item);
            
            _onItemHandled?.Invoke(item);
        }

        _totalItemsQueried += _ugcQueryResultNumItems;
        ReleaseQuery();
    }
}