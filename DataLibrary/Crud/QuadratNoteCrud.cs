using DataLibrary.DataSources;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary.Crud
{
    public static class QuadratNoteCrud
    {
        public static async Task<List<string>> ReadAllSpeciesNotes(IDataSourceConfig config)
        {
            // pull from a view that has short unique notes only
            IEnumerable<string> speciesNotes = await DataHelper.ReadAllEntriesAsync<string>(config, "quadratnotes");
            return speciesNotes.OrderBy(sp => sp).ToList();
        }
    }
}
