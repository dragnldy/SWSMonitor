using MySqlConnector;

namespace DataLibrary.DataSources;

public class MySqlConfig : IDataSourceConfig
{
    // Only one page at a time can attempt this
    public static readonly SemaphoreSlim _semaphore = new(
          initialCount: 1, maxCount: 1);

    public static MySqlConfig? Instance = null;
    public string ConnectionString { get; set; } = string.Empty;
    public MySqlConnection? Connection { get; set; } = null;

    public MySqlConfig(string connectionString, string schema = "sws")
    {
        if (Instance is null)
        {
            // Only do configuration once
            Instance = this;
            ConnectionString = connectionString.Replace("testschema", schema, StringComparison.OrdinalIgnoreCase);
            Connection = OpenDatabaseConnection(ConnectionString);
        }
        else
        {
            Connection = Instance.Connection; // grab the connection from the existing instance
        }

        if (Connection == null || Connection.State != System.Data.ConnectionState.Open)
        {
            if (Connection is not null)
                Connection.Close();
            Connection = SetupConnection(connectionString, Connection);
        }
    }

    public MySqlConnection? SetupConnection(string connectionstring = "", MySqlConnection? currentConnection = null)
    {
        string connstring = string.IsNullOrEmpty(connectionstring) ? Instance?.ConnectionString : connectionstring;
        MySqlConnection? connection = currentConnection ?? OpenDatabaseConnection(connstring);
        if (connection.State != System.Data.ConnectionState.Open)
        {
            CloseDatabaseConnection(connection);
            connection = OpenDatabaseConnection(connstring);
        }
        return connection;
    }
    public MySqlConnection? OpenDatabaseConnection(string connectionString = "")
    {
        MySqlConnection? newConnection;
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = Instance?.ConnectionString ?? throw new Exception("Connection string is not set");
        }
        try
        { 
            newConnection = new MySqlConnection(connectionString);
            newConnection.Open();
            IncreaseTimeout(newConnection);
            return newConnection;
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    private void IncreaseTimeout(MySqlConnection connection)
    {
        // Set the wait_timeout to 600 seconds (10 minutes) for this session
        using (MySqlCommand cmd = new MySqlCommand("SET SESSION wait_timeout = 600", connection))
        {
            cmd.ExecuteNonQuery();
        }
    }

    public MySqlConnection? CloseDatabaseConnection(MySqlConnection? connection)
    {
        if (connection == null) return null;
        if (connection.State != System.Data.ConnectionState.Closed)
        {
            connection.Close();
        }
        return connection;
    }
}
