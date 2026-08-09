using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DataLibrary;

public static class DataValidation
{

    const string simpleEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    const string altEmail = @"/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/";

    public static bool CleanAndValidateEmail(string email)
    {
        Regex regexEmail = new Regex(simpleEmail);
        var matches = regexEmail.Match(email);
        return matches.Success;
    }
}
