using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using Models;

namespace DataLibrary.Crud;

public static class VolunteersCrud
{
    #region Read All Methods- used for populating dropdowns, lists, etc.
    public static async Task<List<Volunteer>> ReadAllVolunteersAsync(IDataSourceConfig? config)
    {
        if (config is ApiClientConfig apiClientConfig)
        {
            VolunteerApiClient volunteerClient = new VolunteerApiClient(apiClientConfig);
            List<Volunteer> results = await volunteerClient.GetAllVolunteersPublicAsync();
            return results ?? new List<Volunteer>();
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Volunteer> volunteers = await DataHelper.ReadAllEntriesAsync<Volunteer>(config, Volunteer.TableName);
            return volunteers.ToList();
        }

        throw new NotImplementedException("Unsupported data source configuration");
    }

    public static async Task<IEnumerable<Volunteer>> ReadActiveVolunteersAsync(IDataSourceConfig? config)
    {
        IEnumerable<Volunteer> volunteers = await ReadAllVolunteersAsync(config);
        return volunteers.Where(v => v.IsActive);
    }

    public static async Task<List<Volunteer>> ReadAllVolunteersByIslandAsync(IDataSourceConfig? config, string island)
    {
        if (config is ApiClientConfig apiClientConfig)
        {
            VolunteerApiClient volunteerClient = new VolunteerApiClient(apiClientConfig);
            List<Volunteer>? results = await volunteerClient!.GetAllVolunteersPublicAsync();

            if (results != null)
            {
                results = results.Where(v => v.Island == island).ToList();
            }
            return results ?? new List<Volunteer>();
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Volunteer> volunteers = await DataHelper.ReadAllEntriesAsync<Volunteer>(config, Volunteer.TableName);
            return volunteers.ToList();
        }

        throw new NotImplementedException("Unsupported data source configuration");
    }

    public static async Task<Volunteer> ReadVolunteerByIdAsync(IDataSourceConfig config, int id)
    {
        if (config is ApiClientConfig apiClientConfig)
        {
            VolunteerApiClient volunteerClient = new VolunteerApiClient(apiClientConfig);
            Volunteer volunteer = await volunteerClient.GetVolunteerByIdAsync(id);
            return volunteer;
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Volunteer> volunteers = await DataHelper.ReadEntries<Volunteer>(config, Volunteer.TableName, (long)id, keyfield: "ID");
            var volunteer = volunteers.FirstOrDefault();
            if (volunteer is null || volunteer.ID != id)
            {
                TraceLogger.LogErrorAuto($"Volunteer not found with ID: {id}");
                return null;
            }
            return volunteer;
        }
        throw new NotImplementedException("Unsupported data source configuration");
    }

    public static async Task<Volunteer?> ReadVolunteerRoleAsync(IDataSourceConfig config, string email)
    {
        if (string.IsNullOrEmpty(email)) return null;
        IEnumerable<Volunteer> volunteers = await ReadAllVolunteersAsync(config);
        Volunteer? volunteer = volunteers.FirstOrDefault(
            v => v.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) == true);
        if (volunteer is null)
        {
            TraceLogger.LogErrorAuto($"Volunteer not found with email: {email}");
            return null;
        }
        TraceLogger.LogWarningAuto($"Volunteer found with email: {email}");
        return volunteer;
    }


    // Used to check for duplicate names when creating a new volunteer, and to get the ID of an existing volunteer when updating
    // Not used by any route directly
    public static async Task<Volunteer> ReadVolunteerByNameAsync(IDataSourceConfig config, string firstlast)
    {
        if (config is ApiClientConfig apiClientConfig)
        {
            VolunteerApiClient volunteerClient = new VolunteerApiClient(apiClientConfig);
            Volunteer volunteer = await volunteerClient.GetVolunteerByNameAsync(firstlast);
            return volunteer;
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Volunteer> volunteers = await DataHelper.ReadEntries<Volunteer>(config, Volunteer.TableName, $"FirstLast = '{firstlast}'");
            var volunteer = volunteers.FirstOrDefault();
            return volunteer;
        }

        throw new NotImplementedException("Unsupported data source configuration");
    }

    #endregion

    #region Methods to support public access with limited fields - not used for CRUD operations, but for public display of volunteer information

    public static async Task<List<Volunteer>> ReadPublicVolunteersAsync(IDataSourceConfig config)
    {
        if (config is ApiClientConfig apiClientConfig)
        {
            VolunteerApiClient volunteerClient = new VolunteerApiClient(apiClientConfig);
            List<Volunteer>? volunteers = await volunteerClient.GetAllVolunteersPublicAsync();
            return volunteers ?? new List<Volunteer>();
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Volunteer> volunteers = await DataHelper.ReadEntries<Volunteer>(config, Volunteer.TableName);
            return volunteers.ToList();
        }

        throw new NotImplementedException("Unsupported data source configuration");
    }

    #endregion

    public static async Task<(bool,Volunteer)> UpdateOrCreateVolunteerAsync(IDataSourceConfig config, Volunteer volunteer)
    {
        if (config is ApiClientConfig apiClientConfig)
        {
            VolunteerApiClient volunteerClient = new VolunteerApiClient(apiClientConfig);

            Volunteer? result = await volunteerClient.UpdateOrCreateVolunteerAsync(volunteer);
            if (result != null)
            {
                volunteer.ID = result.ID;
                return (true, result);
            }
            return (false, volunteer);
        } else if (StaticData.DataSourceConfig is MySqlConfig mySqlConfig)
        {
            if (volunteer is null) return (false, volunteer);
               
            long ID = await MySqlHelperUtils.InsertOrUpdateRecordByIdAsync<Volunteer>(mySqlConfig,
                    volunteer, currentId: volunteer.ID);
            volunteer.ID = (int)ID;
            return (true, volunteer);
        }

        return (false,volunteer);
    }

    public static async Task<bool> DeleteVolunteerAsync(IDataSourceConfig config, int id)
    {
        if (id <= 0) return false;

        if (config is ApiClientConfig apiClientConfig)
        {
            VolunteerApiClient volunteerClient = new VolunteerApiClient(apiClientConfig);
            var statusCode = await volunteerClient.DeleteVolunteerAsync(id);

            if (statusCode == System.Net.HttpStatusCode.OK)
            {
                // Remove from static list
                if (StaticData.Volunteers is not null)
                {
                    StaticData.Volunteers.RemoveAll(n => n.ID == id);
                }
            }
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            // Delete from database
            await MySqlHelperUtils.ExecuteNonQueryAsync(mySqlConfig, $"DELETE FROM `{Volunteer.TableName}` WHERE ID = {id}");
            // Remove from static list
            if (StaticData.Volunteers is not null)
            {
                StaticData.Volunteers.RemoveAll(n => n.ID == id);
            }
        }
        return true;
    }
}
