using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using DataLibrary.Models;
using Models;

namespace DataLibrary.Crud;

public class ProfileCrud
{
    #region Read Methods - used by data entry for specific survey

    // Get all the ProfileEntries for all surveys- will be a lot of data (> 5 meg)
    //public static async Task<Profile> ReadSlimProfilesAsync(IDataSourceConfig config)
    //{
    //    // We signal we want just the slim profile entries by passing a surveyId of 0
    //    // The Profile returned will only have the List<ProfileEntry> populated and the ProfileDetails and ProfileSurfaceDetails will be empty
    //    return await ReadProfilesForSurveyAsync(config, 0);
    //}

    // Get all the ProfileEntries for a specific survey and decode the details and surface details into separate lists
    public static async Task<List<ProfileBase>> ReadProfilesForSurveyAsync(IDataSourceConfig config, long? surveyId = 0l)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (surveyId < 0)
            throw new ArgumentOutOfRangeException(nameof(surveyId), "Survey ID must be non-negative.");

        List<ProfileBase> profileEntries = new List<ProfileBase>();
        if (config is MySqlConfig mySqlConfig)
        {
            if (surveyId.Value > 0)
            {
                // First get all the profile entries 
                IEnumerable<ProfileBase> entries = await DataHelper.ReadEntries<ProfileBase>(config, ProfileEntry.TableName, surveyId.Value);
                profileEntries = entries.ToList();
            }
            else
            {
                // read all of the profileEntries but in the truncated format
                IEnumerable<ProfileBase> entries = await DataHelper.ReadEntries<ProfileBase>(config, ProfileEntry.TableName);
                profileEntries = entries.ToList();
            }

            return profileEntries;
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            ProfileApiClient apiClient = new ProfileApiClient(apiClientConfig);
            if (surveyId.Value > 0)
            {
                var entries = await apiClient.GetProfilesBySurveyIdAsync(surveyId.Value);
                profileEntries = entries.ToList();
            }
            else
            {
                var entries = await apiClient.GetProfilesAsync(0);
                profileEntries = entries.ToList();
            }    
            return profileEntries;
        }
        throw new NotImplementedException();
    }

    #endregion Read Methods - used by data entry for specific survey

    #region Create/Update Methods

    public static async Task<List<ProfileBase>> CreateOrUpdateProfilesAsync(IDataSourceConfig config, long surveyId,List<ProfileBase> profileEntries)
    {

        if (config == null || profileEntries == null)
            throw new ArgumentException("You must create the survey first and provide a valid surveyId");
        if (!profileEntries.Any())
            throw new ArgumentException("The list of profile entries cannot be empty.");
        if (surveyId <= 0)
            throw new ArgumentException("Survey ID must be greater than zero.");

        // Get a list of current ID's for the profile entries for the survey so we can determine which ones need to be removed
        List<long> currentIds = new();
        foreach (var entry in profileEntries)
        {
            entry.EntryDate = DateTime.Now;

            if (entry.ID > 0) currentIds.Add(entry.ID);
        }
        await DeleteFromProfileEntriesNotInListAsync(config, surveyId, currentIds);

        if (config is MySqlConfig mySqlConfig)
        {

            foreach (var profileEntry in profileEntries)
            {
                (long entryId, string guid) = await MySqlHelperUtils.InsertOrUpdateRecordAsync<ProfileBase>(
                    profileEntry,
                    action: profileEntry.ID > 0 ? Actions.Replace : Actions.Insert,
                    keyfield: "ID",
                    keytype: KeyTypes.Long,
                    currentId: profileEntry.ID);
                profileEntry.ID = entryId;
            }
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            ProfileApiClient apiClient = new ProfileApiClient(apiClientConfig);
            profileEntries = await apiClient!.CreateProfileEntries(surveyId, profileEntries);
        }

        return profileEntries ?? new List<ProfileBase>();
        throw new NotImplementedException();
    }

    public static async Task<bool> DeleteProfileBySurveyAsync(IDataSourceConfig config, long surveyId)
    {
        // This will delete all the profile entries for the survey, which will also delete the details and surface details since they are stored in the same table
            // Delete all profile entries for the survey- sending an empty list of IDs will delete all entries for the survey
        return await DeleteFromProfileEntriesNotInListAsync(config, surveyId, new List<long>());
    }

    public static async Task<bool> DeleteFromProfileEntriesNotInListAsync(IDataSourceConfig config, long surveyId, List<long> pelist)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            // Delete the profile entries for the survey that are NOT in the list. If the list is empty, it will delete all entries for the survey
            var result =  await MySqlHelperUtils.DeleteFromTableNotInIdList(mySqlConfig, "ProfileEntries", surveyId, pelist);
//            else              await MySqlHelperUtils.DeleteFromTableWithFilter(mySqlConfig, "ProfileEntries", $"SurveyID = {surveyId}");
            return true;
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            ProfileApiClient apiClient = new ProfileApiClient(apiClientConfig);
            return await apiClient.DeleteProfileEntriesAsync(surveyId,pelist);
        }
        throw new NotImplementedException("Delete operation is not implemented for this data source.");
    }

    #endregion Create/Update Methods
}

