using System.Diagnostics;
using Steamworks;

namespace SteamworksWorker.Modules;

public sealed class QueryInstance
{
    private EResult _errorState;
    private List<dynamic> _items = new (Constants.k_unEnumeratePublishedFilesMaxResults);
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

    public QueryInstance(QueryType queryType = QueryType.MostVoted, uint playtimeStats = 30, string searchText = null)
    {
        _queryHook = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);

        _queryType = QueryHelper.queryTypeToUGCIndex[queryType];
        _playtimeStats = Math.Clamp(playtimeStats, 1, 365);
        _searchText = searchText ?? String.Empty;
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

            PublishedFileId_t[] itemDependencies = new PublishedFileId_t[32]; //No item should have more than 32 dependencies? lol
            SteamUGC.GetQueryUGCChildren(qHandle, i, itemDependencies, (uint) itemDependencies.Length);

            //TODO: Retrieve the data we want from the workshop item
            
            uint[] votesUpAndDown = new[] {pDetails.m_unVotesUp, pDetails.m_unVotesDown};
        }

        _totalItemsQueried += _ugcQueryResultNumItems;
        ReleaseQuery();
    }
}