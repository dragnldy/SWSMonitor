using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models;

public partial class BeachEvent : BeachEventBase
{
    public BeachEvent(string beachName, DateTime surveyDate) : base()
    {
        BeachName = beachName;
        SurveyDate = surveyDate;
    }
}
