using System;


namespace DataLibrary.ModelExtensions
{
    // Restrict the attribute to properties only
    [AttributeUsage(AttributeTargets.Property)]
    public class DBIgnoreAttribute : Attribute
    {
        // Use to flag a property to be ignored by database operations (e.g. not included in insert/update queries)
        // Positional parameter (passed via constructor)
        public DBIgnoreAttribute()
        {
        }
    }
}
