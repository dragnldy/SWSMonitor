using DataLibrary.Crud;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DataLibrary.DataSources;

public enum Actions
{
    Insert,
    Replace,
    Update
}

public enum KeyTypes
{
    Long,
    Guid
}

// Interface with MySQL database using MySqlConnector library
// This cannot be used with Blazor apps as Webassembly does not support direct database connections
// It is designed to be used in console, Avalonia, Maui or server-side applications
public static class MySqlHelperUtils
{
    // Basic input sanitization to prevent SQL injection- only required when input comes from an untrusted source and is being used to construct raw SQL queries (like beach/volunteer name)
    public static string SanitizeInputSqlString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        // Basic sanitization: escape single quotes and remove semicolons
        return input.Replace("'", "''").Replace(";", string.Empty);
    }

    public static Regex sqlInjectionStrings = new Regex(@"(@|--|'|;|\b(SELECT|UNION|DROP|DELETE|INSERT|UPDATE|EXECUTE|EXEC|OR|AND)\b)", RegexOptions.IgnoreCase);
    public static bool IsSafeFromInjectionString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;
        return !sqlInjectionStrings.IsMatch(input);
    }

    #region Generic Method to Insert or Update a record using 'INSERT ... ON DUPLICATE KEY UPDATE' and reflection to generate SQL
    // Note that 'REPLACE' works similarly to 'INSERT ... ON DUPLICATE KEY UPDATE'
    // but it deletes the existing record and inserts a new one, which can have implications for auto-incrementing IDs and foreign key relationships.
    // 'INSERT ... ON DUPLICATE KEY UPDATE' will update the existing record without changing its ID or affecting related records.

    public static async Task<long> InsertOrUpdateRecordByIdAsync<T>(MySqlConfig config,T record,long currentId = -1L, string keyField = "ID")
    {
        Actions action = currentId <= 0 ? Actions.Insert : Actions.Replace;
        long newId = currentId;
        if (record is null)
        {
            throw new InvalidOperationException("Record supplied for insert/update is null.");
        }

        // if the primary key is a guid and we need to update or insert, we can use 'replace' syntax
        string query = SqlQueryBuilder.BuildInsertQuery<T>(record, action: action, keyfield: keyField, keytype: KeyTypes.Long);
        if (action == Actions.Update)
        {
            query = SqlQueryBuilder.BuildUpdateQuery<T>(record, keyfield: keyField);
            query += $" where {keyField} = {currentId}";
        }
        MySqlConnection connection = UpdateConnector();
        using (MySqlCommand command = new MySqlCommand(query, connection))
        {
            try
            {
                // Add parameters to the command from the record object
                SqlQueryBuilder.AddParametersFromRecord<T>(command, record, action: action, keyfield: keyField, keytype: KeyTypes.Long);
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                int rowsAffected = command.ExecuteNonQuery();

                newId = (action == Actions.Insert) ? command.LastInsertedId : currentId;
                System.Diagnostics.Debug.WriteLine($"{rowsAffected} row(s) updated/inserted successfully. Record ID: {newId}");

                if (rowsAffected > 0)
                {
                    if (action == Actions.Insert)
                    {
                        return (command.LastInsertedId == 0 ? 1 : newId); // Some inserts won't result in autonumbered Id
                    }
                    else
                    {
                        return currentId;
                    }
                }
                else
                {
                    newId = -1;
                    System.Diagnostics.Debug.WriteLine("No rows were updated/inserted.");
                }
            }
            catch (MySqlException ex)
            {
                TraceLogger.LogError(nameof(MySqlHelperUtils), "InsertOrUpdateRecord", $"Error updating record: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error updating record: {ex.Message}");
            }
            finally
            {
                if (connection is not null)
                    connection.Close();
            }
            return -1L;
        }
    }


    public static async Task<long> InsertOrUpdateRecord<T>(T record, string query = "", string keyfield = "ID", string action = "Insert", long currentId = -1L, string currentGuid = "", string keytype = "long")
    {
        (long newId, string newGuid) result = await InsertOrUpdateRecordAsync<T>(record, query, keyfield, GetAction(action), currentId, currentGuid, GetKeyType(keytype));
        return result.newId;
    }

    private static Actions GetAction(string action)
    {
        return action.ToLowerInvariant() switch
        {
            "insert" => Actions.Insert,
            "replace" => Actions.Replace,
            "update" => Actions.Update,
            _ => throw new ArgumentException($"Invalid action: '{action}'. Valid actions are: Insert, Replace, Update.", nameof(action))
        };
    }

    private static KeyTypes GetKeyType(string keytype)
    {
        return keytype.ToLowerInvariant() switch
        {
            "long" => KeyTypes.Long,
            "guid" => KeyTypes.Guid,
            _ => throw new ArgumentException($"Invalid key type: '{keytype}'. Valid key types are: Long, Guid.", nameof(keytype))
        };
    }

    public static async Task<string> xInsertOrUpdateRecordWithGuidAsync<T>(T record, string query = "", string keyfield = "AirTableID", string action = "Insert", string currentGuid = "")

    {
        (long newId, string newGuid) result = await InsertOrUpdateRecordAsync<T>(record, query: query, keyfield: keyfield, action: GetAction(action),
            currentId: -1l, currentGuid: currentGuid, keytype: GetKeyType("guid"));
        return result.newGuid;
    }    
    public static async Task<long> InsertOrUpdateRecordWithIdAsync<T>(T record, string query = "", string keyfield = "ID", string action = "Insert", long currentId = -1L)
    {
        (long newId, string newGuid) result = await InsertOrUpdateRecordAsync<T>(record, query: query, keyfield: keyfield, action: GetAction(action),
            currentId: currentId, currentGuid: "", keytype: GetKeyType("long"));
        return result.newId;
    }
    public static async Task<(long,string)> InsertOrUpdateRecordAsync<T>
        (T record, string query="", string keyfield="ID", Actions action= Actions.Insert, 
            long currentId = -1L, string currentGuid = "", KeyTypes keytype = KeyTypes.Long )
    {
        MySqlConnection connection = UpdateConnector();

        long newId = currentId;
        string newGuid = currentGuid;

        if (record is null)
        {
           throw new InvalidOperationException("Record supplied for insert/update is null.");
        }

        if (string.IsNullOrEmpty(query))
        {
            bool canreplace = action == Actions.Replace || keytype == KeyTypes.Guid;

            if (canreplace || action == Actions.Insert)
            {
                // if the primary key is a guid and we need to update or insert, we can use 'replace' syntax
                query = SqlQueryBuilder.BuildInsertQuery<T>(record, tableName: "", paramPrefix: "", action: action, keyfield: keyfield, keytype: keytype);
            }
            else
            { 
                // For updates, we need to ensure we have a key value to identify the record
                if (currentId == -1L && string.IsNullOrEmpty(currentGuid))
                {
                    throw new InvalidOperationException("For update/replace actions, a current ID or GUID must be provided.");
                }
                query = SqlQueryBuilder.BuildUpdateQuery<T>(record, tableName: "", paramPrefix: "", keyfield: keyfield);
                if (keytype == KeyTypes.Long)
                    query += $" where {keyfield} = {currentId}";
                else
                    query += $" where {keyfield} = {currentGuid}";
            }
        }
        using (MySqlCommand command = new MySqlCommand(query, connection))
        {
            try
            {
                // Add parameters to the command from the record object
                SqlQueryBuilder.AddParametersFromRecord<T>(command, record, action:action, paramPrefix: "", keyfield: keyfield, keytype: keytype);
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                int rowsAffected = command.ExecuteNonQuery();

                newId = (action == Actions.Insert) ? command.LastInsertedId : currentId;
                System.Diagnostics.Debug.WriteLine($"{rowsAffected} row(s) updated/inserted successfully. Record ID: {newId} GUID: {newGuid}");
                connection.Close();

                if (rowsAffected > 0)
                {
                    if (action == Actions.Insert)
                    {
                        return (command.LastInsertedId == 0 ? 1 : newId, newGuid); // Some inserts won't result in autonumbered Id
                    }
                    else
                    {
                        return (currentId, currentGuid);
                    }
                }
                else
                {
                    newId = -1;
                    System.Diagnostics.Debug.WriteLine("No rows were updated/inserted.");
                }
            }
            catch (MySqlException ex)
            {
                TraceLogger.LogError(nameof(MySqlHelperUtils), "InsertOrUpdateRecord", $"Error updating record: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error updating record: {ex.Message}");
            }
            finally
            {
                // Connection will be automatically closed by the 'using' statement
            }
        }
        return (newId, newGuid);
    }
    #endregion Generic Method to Insert or Update a record using 'REPLACE'

    public static MySqlConnection? UpdateConnector(IDataSourceConfig? inconfig = null, bool noRetry = false)
    {
        try
        {
            MySqlConfig config = (inconfig as MySqlConfig) ?? MySqlConfig.Instance;

            if (config is null)
                throw new ArgumentException($"Invalid config type: {inconfig.GetType().Name}. Expected MySqlConfig or null.", nameof(inconfig));

            MySqlConnection? connection = config.Connection ?? config.OpenDatabaseConnection();
            connection = CycleConnection(config, connection);
            // config.Connection = connection;

            try
            {
                // perform a quick connection check to wake up the connection
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1";
                    var result = cmd.ExecuteScalar();

                    if (result == null || result.ToString() != "1")
                    {
                        // Connection is not active or not responding
                        throw new InvalidOperationException("MySQL connection is not responding.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                TraceLogger.LogError(nameof(MySqlHelperUtils), nameof(UpdateConnector), $"MySQL connection check failed: {ex.Message}");
                // try one more time
                if (noRetry) return null;
                    connection = CycleConnection(config, connection);
            }
            return connection;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError(nameof(MySqlHelperUtils), nameof(ReadTable), $"Failed to open MySQL connection.: {ex.Message}");
            var connection = UpdateConnector(inconfig, noRetry: true);
            if (connection is null) 
                throw;
        }

        return null;
    }

    private static MySqlConnection CycleConnection(MySqlConfig config, MySqlConnection? connection = null)
    {
        if (connection is not null && connection.State == ConnectionState.Open)
        {
            try
            {
                connection.Close();
                System.Diagnostics.Debug.WriteLine("MySQL connection closed to cycle connection.");
            }
            catch (MySqlException ex)
            {
                TraceLogger.LogError(nameof(MySqlHelperUtils), nameof(CycleConnection), $"Error closing MySQL connection: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error closing MySQL connection: {ex.Message}");
            }
        }
        return config.OpenDatabaseConnection(); 
    }

    #region Generalized update routine using a sql statement- not currently used
    public static async Task<bool> UpdateRecord(string mysqlstatement)
    {
        MySqlConnection connection = UpdateConnector();
        try
        {
            // Implementation for updating existing record in the specified table
            using (MySqlCommand cmd = new MySqlCommand(mysqlstatement, connection))
            {
                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating record: {ex.Message}");
            return false;
        }
        return true;
    }
    #endregion

    #region Generic Read Table Method
    public static async Task<List<DataRecord>> ReadTable(IDataSourceConfig? config, string tableName, string? recordSelect = null)
    {

        MySqlConnection connection = UpdateConnector(config);

        List<DataRecord> allRecords = new();
        try
        {
            try
            { 
                // SQL query to retrieve data
                string sql = $"SELECT * FROM {tableName}";
                if (!string.IsNullOrEmpty(recordSelect))
                {
                    if (!recordSelect.TrimStart().StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
                    {
                        recordSelect = "WHERE " + recordSelect;
                    }
                    sql += $" {recordSelect}";
                }
                // Create MySqlCommand to execute the query
                using var cmd = new MySqlCommand(sql, connection);
                // Execute the command and retrieve data using MySqlDataReader
                using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
                // Loop through the retrieved data and print to console
                while (await reader.ReadAsync())
                {
                    var fields = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        fields[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }

                    DataRecord newRecord = new DataRecord { Fields = fields };
                    allRecords.Add(newRecord);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening database connection: {ex.Message}");
            }

            if (connection is not null)
                connection.Close();

        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine(exc.ToString());
        }
        return allRecords.Count > 0 ? allRecords : new List<DataRecord>(); // Return empty list if no records found
    }
    #endregion Generic Read Table Method

    #region Generic routine to run non-query SQL statements (INSERT, UPDATE, DELETE)

    // Returns -1 if error, otherwise rows affected

    public static async Task<int> ExecuteNonQueryAsync(IDataSourceConfig? config = null, string sql = "")
    {
        MySqlConnection connection = UpdateConnector(config);

        if (string.IsNullOrEmpty(sql))
        {
            TraceLogger.LogWarning(nameof(MySqlHelperUtils), nameof(ExecuteNonQueryAsync), "No SQL statement provided.");
            System.Diagnostics.Debug.WriteLine("No valid connection or SQL statement.");
            return -1;
        }
        // Create MySqlCommand to execute the query
        using (MySqlCommand command = new MySqlCommand(sql, connection))
        {
            try
            {
                int rowsAffected = await command.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine($"{rowsAffected} row(s) affected: {sql}");
                return rowsAffected;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating record: {ex.Message}");
            }
            finally
            {
                // Connection will be automatically closed by the 'using' statement
            }
        }
        return -1;
    }
    #endregion Generic routine to run non-query SQL statements (INSERT, UPDATE, DELETE)

    #region Generic routine to delete records for a survey not in a list
    public static async Task<bool> DeleteFromTableNotInIdList(IDataSourceConfig config, string tablename, long surveyId, List<long> pelist)
    {
        if (config is null || config is not MySqlConfig)
            throw new Exception("Invalid config type. Expected MySqlConfig.");

        MySqlConnection connection = UpdateConnector(config);
        string filter = $"SURVEYID = {surveyId}";

        // This method is intended to delete all entries in a survey that are not in the list
        try
        {
            if (pelist.Any())
            {
                string idlist = $"({string.Join(",", pelist)})";
                filter += $" AND ID NOT IN {idlist}";
            }
            // Delete using data source specific logic
            var deleteQuery = $"DELETE FROM `{tablename}` WHERE {filter}";
            using (var cmd = new MySqlConnector.MySqlCommand(deleteQuery, connection))
            {
                var numRowsAffected = await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"Deleted {numRowsAffected} records from table {tablename} with filter {filter}");
            }
            if (connection is not null)
                connection.Close();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting MonitorNames with filter {filter}: {ex.Message}");
            if (connection is not null)
                connection.Close();
            return false;
        }

        return false;
    }

    public static async Task<bool> DeleteFromTableWithIdListx(IDataSourceConfig config, string tablename, List<long> pelist)
    {
        if (config is null || config is not MySqlConfig)
            throw new Exception("Invalid config type. Expected MySqlConfig.");

        MySqlConnection connection = UpdateConnector(config);

        // This method is intended to delete specific entry records by their IDs, regardless of the survey.
        try
        {
            if (!pelist.Any())
                return true; // Nothing to delete, so we can consider this a success

            string idlist = $"({string.Join(",", pelist)})";

            // Delete using data source specific logic
            var deleteQuery = $"DELETE FROM `{tablename}` WHERE ID IN {idlist}";
            using (var cmd = new MySqlConnector.MySqlCommand(deleteQuery, connection))
            {
                var numRowsAffected = await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"Deleted {numRowsAffected} records from table {tablename} with ID list {idlist}");
            }
            if (connection is not null)
                connection.Close();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting MonitorNames with ID list {ex.Message}");
            if (connection is not null)
                connection.Close();
            return false;
        }

        return false;
    }

    public static async Task<bool> DeleteFromTableWithFilter(IDataSourceConfig config, string tableName, string filter)
    {
        MySqlConnection connection = UpdateConnector(config);

        if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(tableName))
        {
            Debug.WriteLine($"TableName and filter required for record deletion");
            return false;
        }
        try
        {
            var deleteQuery = $"DELETE FROM `{tableName}` WHERE {filter}";
            using (var cmd = new MySqlCommand(deleteQuery, connection))
            {
                var numRowsAffected = await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"Deleted {numRowsAffected} records from table {tableName} with filter {filter}");
            }
            if (connection is not null)
                connection.Close();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting records from table {tableName} with filter {filter}: {ex.Message}");
            if (connection is not null)
                connection.Close();
            return false;
        }
    }

    internal static async Task<int> GetTableRecordCount(IDataSourceConfig config, string tableOrViewName)
    {
        if (string.IsNullOrEmpty(tableOrViewName))
        {
            TraceLogger.LogWarningAuto("Table or view name is null or empty. Cannot get record count.");
            return 0;
        }
        MySqlConnection connection = UpdateConnector(config);
        try
        {
            var countQuery = $"SELECT COUNT(*) FROM `{tableOrViewName}`";
            using (var cmd = new MySqlCommand(countQuery, connection))
            {
                var result = await cmd.ExecuteScalarAsync();
                if (connection is not null)
                    connection.Close();

                if (result != null && int.TryParse(result.ToString(), out int count))
                {
                    return count;
                }
            }
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error getting record count for table {tableOrViewName}: {ex.Message}");
            if (connection is not null)
                connection.Close();
        }
        return 0;
    }

    #endregion Generic routine to delete records based on a list of IDs
}

