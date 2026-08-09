using DataLibrary.DataSources;
using DataLibrary.ModelExtensions;
using MySqlConnector;
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLibrary;

public class SqlQueryBuilder
{

    // Build an INSERT or REPLACE query for the provided record
    // Of format INSERT INTO `TableName` (`Col1`, `Col2`, ...) VALUES (@Col1, @Col2, ...)
    internal static string BuildInsertQuery<T>(T record, string tableName = "", string? keyfield = "ID", string? paramPrefix = null,
        Actions action = Actions.Insert, KeyTypes keytype = KeyTypes.Long)
    {
        bool isInsert = action == Actions.Insert;
        // If the tablename is not provided, try to get it from the class static property TableName
        // or fallback to the class name

        StringBuilder sb = new StringBuilder(); // tablename and column names
        StringBuilder pb = new StringBuilder(); // parameter placeholders
        sb.Append(isInsert ? $"INSERT INTO" : $"REPLACE INTO");

        var type = record.GetType();

        if (string.IsNullOrEmpty(tableName))
        {
            // See if we can get a tablename from the class- would be in const string TableName
            FieldInfo fieldInfo = type!.GetField(
                "TableName", BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);
            if (fieldInfo != null) { tableName = (string?)fieldInfo.GetValue(null) ?? type.Name; }
        }

        sb.Append($" `{tableName}` (");
        pb.Append($" VALUES (");

        // Now iterate over properties to build columns and value holders- we will skip properties with JsonIgnore
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            // Skip indexers
            if (prop.GetIndexParameters().Length > 0) continue;

            // Skip properties annotated with JsonIgnore
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null) continue;

            // Skip properties annotated with DBIgnore
            if (prop.GetCustomAttribute<DBIgnoreAttribute>() is not null) continue;

            string propName = prop.Name;
            if (isInsert && keytype != KeyTypes.Guid && propName.Equals(keyfield, StringComparison.OrdinalIgnoreCase) ) continue;

            sb.Append($"`{propName}`, ");
            pb.Append($"@{(string.IsNullOrEmpty(paramPrefix) ? propName : $"{paramPrefix}{propName}")}, ");
        }

        sb.Length -= 2; // Remove last comma and space
        pb.Length -= 2; // Remove last comma and space

        sb.Append($") {pb.ToString()})");
        return sb.ToString();
    }

    internal static string BuildUpdateQuery<T>(T record, string tableName="", string keyfield = "", string paramPrefix="")
    {
        StringBuilder sb = new StringBuilder(); // tablename and column names
        var type = record.GetType();

        // Now iterate over properties to build columns and value holders- we will skip properties with JsonIgnore
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            // Skip indexers
            if (prop.GetIndexParameters().Length > 0) continue;

            // Skip properties annotated with JsonIgnore
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null) continue;
            
            // Skip properties annotated with DBIgnore
            if (prop.GetCustomAttribute<DBIgnoreAttribute>() is not null) continue;

            string propName = prop.Name;
            if (propName.Equals(keyfield, StringComparison.OrdinalIgnoreCase)) continue;

            sb.Append($"`{propName}` = ");
            sb.Append($"@{(string.IsNullOrEmpty(paramPrefix) ? propName : $"{paramPrefix}{propName}")}, ");
        }

        sb.Length -= 2; // Remove last comma and space
        return sb.ToString();
    }
    // Adds parameters to the provided MySqlCommand by reflecting over the public instance properties
    // of the record. Properties decorated with System.Text.Json.Serialization.JsonIgnoreAttribute are skipped.
    // If a field is a primary key (keyfield) and action is "Insert", it is skipped
    // if action is not insert, the keyfield is checked for <=0 and set to DBNull.Value if so
    // This allows the primary key to be auto-incremented.
    // - Null values are translated to DBNull.Value.
    // - DateTime values are passed as DateTime.
    // - Enum values are converted to their underlying integer.
    // - Non-string IEnumerable (e.g. lists) are serialized to JSON.
    // - Other values are passed as-is.
    // paramPrefix can be used to avoid name collisions (e.g. "p_" -> "@p_PropertyName")
    public static void AddParametersFromRecord<T>(MySqlCommand cmd, T record, string? keyfield = "", 
        string? paramPrefix = null, Actions action= Actions.Insert, KeyTypes keytype = KeyTypes.Long)
    {
        bool isInsert = action == Actions.Insert;

        if (cmd is null) throw new ArgumentNullException(nameof(cmd));
        if (record is null) return;

        var type = record.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            // Skip indexers
            if (prop.GetIndexParameters().Length > 0) continue;

            // Skip properties annotated with JsonIgnore
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null) continue;

            // Skip properties annotated with DBIgnore
            if (prop.GetCustomAttribute<DBIgnoreAttribute>() is not null) continue;

            string propName = prop.Name;
            if (isInsert && keytype != KeyTypes.Guid && propName.Equals(keyfield, StringComparison.OrdinalIgnoreCase)) continue;

            string parameterName = "@" + (string.IsNullOrEmpty(paramPrefix) ? propName : $"{paramPrefix}{propName}");

            object? value;
            try
            {
                value = prop.GetValue(record);
            }
            catch
            {
                // If property getter throws, skip it
                continue;
            }

            object dbValue;
            if (keytype != KeyTypes.Guid && propName.Equals(keyfield, StringComparison.InvariantCultureIgnoreCase))
            {
                if (int.TryParse(value?.ToString(), out int intValue))
                {
                    if (intValue <= 0)
                        dbValue = DBNull.Value;
                    else
                        dbValue = intValue;
                }
                else
                {
                    dbValue = DBNull.Value;
                }
            }
            else
            {
                if (value is null)
                {
                    dbValue = DBNull.Value;
                }
                else if (value is DateTime dt)
                {
                    dbValue = dt;
                }
                else if (value is DateTimeOffset dto)
                {
                    dbValue = dto.DateTime;
                }
                else if (prop.PropertyType.IsEnum || value.GetType().IsEnum)
                {
                    dbValue = Convert.ToInt32(value);
                }
                else if (value is string)
                {
                    dbValue = value;
                }
                else if (value is IEnumerable && !(value is string))
                {
                    // Serialize collections/complex types to JSON string
                    try
                    {
                        dbValue = JsonSerializer.Serialize(value);
                    }
                    catch
                    {
                        if (value is null)
                            dbValue = DBNull.Value;
                        else
                            dbValue = value!.ToString();
                    }
                }
                else
                {
                    dbValue = value;
                }
            }
            cmd.Parameters.AddWithValue(parameterName, dbValue);
        }
    }
}