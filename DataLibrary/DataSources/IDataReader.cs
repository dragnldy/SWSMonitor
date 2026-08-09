using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary.DataSources;

public interface IDataReader
{
    /// <summary>
    /// Reads data from a specified table in a database and returns it as a json string.
    /// </summary>
    /// <param name="connectionInfo"></param>
    /// <returns></returns>
    // public static string ReadTable(object connectionInfo);
}
