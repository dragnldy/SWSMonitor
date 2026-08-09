using DataLibrary.Crud;
using Models;

namespace DataLibrary.ModelExtensions;

public class Survey: SurveyBase
{
    private SurveyBase surveyBase;
    public Survey()
    {
        // default constructor for json deserialization
    }

    public Survey(SurveyBase surveyBase)
    {
        this.surveyBase = surveyBase;
        DataHelper.CopyProperties<SurveyBase, Survey>(surveyBase, this);
    }

    // Only one of these per survey- rest are lists
    public BeachEventBase? BeachEvent { get; set; } = new BeachEventBase(id: 0, surveyid: 0);
    public List<ProfileBase> ProfileEntries { get; set; } = new List<ProfileBase>();
    public List<QuadratBase> QuadratEntries { get; set; } = new List<QuadratBase>();
}
