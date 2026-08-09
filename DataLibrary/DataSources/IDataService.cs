using DataLibrary.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary.DataSources;

public interface IDataService
{
    // These could be asynchronous methods or properties returning ObservableCollections/Lists
    IEnumerable<BeachData> GetAvailableBeaches();
    IEnumerable<Volunteer> GetAvailableVolunteers();
    IEnumerable<Species> GetAvailableSpecies();
    IEnumerable<LookupTable> GetAvailableLookupTables();
    IEnumerable<CityState> GetAvailableCityStates();
    IEnumerable<SurveyBase> GetAvailableSurveys();
}
public class SurveyDataService : IDataService
{
    public IEnumerable<BeachData> GetAvailableBeaches()
    {
        return StaticData.Beaches; // Or load them directly from the database here
    }

    public IEnumerable<Volunteer> GetAvailableVolunteers()
    {
        return StaticData.Volunteers; // Or load them directly
    }

    public IEnumerable<Species> GetAvailableSpecies()
    {
        return StaticData.Species; // Or load them directly
    }

    public IEnumerable<LookupTable> GetAvailableLookupTables()
    {
        return StaticData.LookupTables; // Or load them directly
    }

    public IEnumerable<CityState> GetAvailableCityStates()
    {
        return StaticData.CityStates; // Or load them directly
    }

    public IEnumerable<SurveyBase> GetAvailableSurveys()
    {
        return StaticData.Surveys; // Or load them directly
    }

}