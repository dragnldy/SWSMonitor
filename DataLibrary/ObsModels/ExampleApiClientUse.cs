using DataLibrary.DataSources.FeatureClients;
using Models;
using SurveyApiClients.FeatureClients;

namespace DataLibrary.ObsModels;


/// <summary>
/// Examples of how to use the BeachDataApiClient
/// </summary>
public class BeachDataApiExamples
{
    public static async Task RunExamples()
    {
        // Setup HttpClient
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiClientHelper.BASE_URL)
        };

        var client = new BeachDataApiClient(httpClient);

        try
        {
            // WASM is single-threaded and fully supports async/await
            // Never use .Wait(), .Result, or Task.Run() in WASM - they cause "Cannot wait on monitors" error
            // The URL must support CORS (Cross-Origin Resource Sharing)

            var response = await httpClient.GetAsync(ApiClientHelper.BASE_URL);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
            }
            TraceLogger.LogInformation(response.ToString());
        }
        catch (Exception ex)
        {
            TraceLogger.LogError(nameof(BeachDataApiExamples), nameof(RunExamples), $"Error connecting to API: {ex.Message}");
        }
        // Cannot use .ConfigureAwait in WASM because it causes deadlock,
        // but in other contexts you might want to use it to avoid blocking the UI thread
        // The URL must support CORS (Cross-Origin Resource Sharing)


        // Example 1: Get all beaches
        TraceLogger.LogInformation("=== Get All Beaches ===");
        var beaches = await client.GetAllBeachesAsync();
        foreach (var beach in beaches ?? Enumerable.Empty<BeachData>())
        {
            TraceLogger.LogInformation($"Beach: {beach.BeachName} (ID: {beach.ID})");
        }

        // Example : Get beach by Good ID
        TraceLogger.LogInformation("=== Get Beach By ID ===");
        var beachbyID = await client.GetBeachByIdAsync(3);
        if (beachbyID is not null && beachbyID.ID == 3)
        {
            TraceLogger.LogInformation($"Beach: {beachbyID.BeachName} (ID: {beachbyID.ID})");
        }
        else
        {
            TraceLogger.LogInformation($"Beach with ID: 3 not found");
        }

        // Example : Get beach by Bad ID
        TraceLogger.LogInformation("=== Get Beach By ID ===");
        beachbyID = await client.GetBeachByIdAsync(99999);
        if (beachbyID is not null && beachbyID.ID != 99999)
        {
            TraceLogger.LogInformation($"Correctly failed to find beach 99999");
        }
        else
        {
            TraceLogger.LogInformation($"Beach with ID: 999999 erroneously returned result");
        }
        // Example 5: Get beaches by island
        TraceLogger.LogInformation("\n=== Get Beaches By Island ===");
        var whidbeyBeaches = await client.GetBeachesByIslandAsync("Whidbey");
        TraceLogger.LogInformation($"Found {whidbeyBeaches?.Count ?? 0} beaches on Whidbey");

        // Example 6: Get monitored beaches
        TraceLogger.LogInformation("\n=== Get Monitored Beaches ===");
        var monitoredBeaches = await client.GetMonitoredBeachesAsync();
        TraceLogger.LogInformation($"Found {monitoredBeaches?.Count ?? 0} monitored beaches");


        // Example 2: Create a new beach
        TraceLogger.LogInformation("\n=== Create New Beach ===");
        var newBeach = new BeachDataApiClient.CreateBeachRequest
        {
            BeachName = "Example Beach",
            Island = "San Juan Island",
            Latitude = "48,30.5",
            Longitude = "-122,45.2",
            CurrentlyMonitored = 1,
            County = 1,
            DnrClass = 2,
            SurveyWidth = 100,
            AdditionalNotes = "Created via API example"
        };

        var createdBeach = await client.CreateBeachAsync(newBeach);
        if (createdBeach is not null)
            TraceLogger.LogInformation($"Created beach with ID: {createdBeach?.ID}");
        else
            TraceLogger.LogInformation("Failed to create beach");

        // Example 3: Get beach by ID
        TraceLogger.LogInformation("\n=== Get Beach By ID ===");
        if (createdBeach != null)
        {
            var fetchedBeach = await client.GetBeachByIdAsync(createdBeach.ID);
            if (fetchedBeach is not null)
                TraceLogger.LogInformation($"Fetched: {fetchedBeach?.BeachName}");
            else
                TraceLogger.LogInformation("Beach not found by ID");
        }

        // Example 4: Update beach
        TraceLogger.LogInformation("\n=== Update Beach ===");
        if (createdBeach != null)
        {
            var updateRequest = new BeachDataApiClient.UpdateBeachRequest
            {
                Id = createdBeach.ID,
                BeachName = "Updated Example Beach",
                Island = "San Juan Island",
                Latitude = "48,30.5",
                Longitude = "-122,45.2",
                CurrentlyMonitored = 0, // Changed to not monitored
                SurveyWidth = 150, // Updated width
                AdditionalNotes = "Updated via API example"
            };

            var updatedBeach = await client.UpdateBeachAsync(createdBeach.ID, updateRequest);
            if (updatedBeach is not null)
                TraceLogger.LogInformation($"Updated: {updatedBeach?.BeachName}, Monitored: {updatedBeach?.IsCurrentlyMonitored}");
            else
                TraceLogger.LogInformation($"Failed to update beach");
        }

        // Example 7: Delete beach
        TraceLogger.LogInformation("\n=== Delete Beach ===");
        if (createdBeach != null)
        {
            var deleted = await client.DeleteBeachAsync(createdBeach.ID);
            TraceLogger.LogInformation($"Beach deleted: {deleted}");
        }
    }

    /// <summary>
    /// Example with error handling
    /// </summary>
    public static async Task ExampleWithErrorHandling()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };

        var client = new BeachDataApiClient(httpClient);

        try
        {
            // Try to get a beach that doesn't exist
            var beach = await client.GetBeachByIdAsync(999999);
            if (beach == null)
            {
                Console.WriteLine("Beach not found");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
        }

        try
        {
            // Try to create a beach with missing required field
            var invalidBeach = new BeachDataApiClient.CreateBeachRequest
            {
                BeachName = "", // Empty name - should fail validation
                Island = "Test Island"
            };

            await client.CreateBeachAsync(invalidBeach);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            Console.WriteLine("Validation error: Beach name is required");
        }

        try
        {
            // Try to update a non-existent beach
            var updateRequest = new BeachDataApiClient.UpdateBeachRequest
            {
                Id = 999999,
                BeachName = "Non-existent Beach"
            };

            await client.UpdateBeachAsync(999999, updateRequest);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Update error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example using dependency injection
    /// </summary>
    public class BeachService
    {
        private readonly BeachDataApiClient _apiClient;

        public BeachService(BeachDataApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> EnsureBeachExistsAsync(string beachName, string island)
        {
            // Check if beach already exists
            var beaches = await _apiClient.GetAllBeachesAsync();
            var existingBeach = beaches?.FirstOrDefault(b =>
                b.BeachName.Equals(beachName, StringComparison.OrdinalIgnoreCase) &&
                b.Island?.Equals(island, StringComparison.OrdinalIgnoreCase) == true);

            if (existingBeach != null)
            {
                Console.WriteLine($"Beach already exists with ID: {existingBeach.ID}");
                return true;
            }

            // Create new beach
            var newBeach = new BeachDataApiClient.CreateBeachRequest
            {
                BeachName = beachName,
                Island = island,
                CurrentlyMonitored = 1
            };

            var created = await _apiClient.CreateBeachAsync(newBeach);
            Console.WriteLine($"Created new beach with ID: {created?.ID}");
            return created != null;
        }

        public async Task<int> GetMonitoredBeachCountAsync()
        {
            var beaches = await _apiClient.GetMonitoredBeachesAsync();
            return beaches?.Count ?? 0;
        }

        public async Task<Dictionary<string, int>> GetBeachesByIslandStatisticsAsync()
        {
            var allBeaches = await _apiClient.GetAllBeachesAsync();
            return allBeaches?
                .Where(b => !string.IsNullOrEmpty(b.Island))
                .GroupBy(b => b.Island!)
                .ToDictionary(g => g.Key, g => g.Count())
                ?? new Dictionary<string, int>();
        }
    }
    /// <summary>
    /// Example service class using the API client
    /// </summary>
    public class VolunteerService
    {
        private readonly VolunteerApiClient _apiClient;

        public VolunteerService(VolunteerApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Get all lead volunteers
        /// </summary>
        public async Task<List<Volunteer>> GetLeadVolunteersAsync()
        {
            var volunteers = await _apiClient.GetAllVolunteersAsync();
            return volunteers?
                .Where(v => v.IsLead && v.IsActive)
                .OrderBy(v => v.FirstLast)
                .ToList() ?? new List<Volunteer>();
        }

        /// <summary>
        /// Get species experts for a specific island
        /// </summary>
        public async Task<List<Volunteer>> GetSpeciesExpertsByIslandAsync(string island)
        {
            var volunteers = await _apiClient.GetVolunteersByIslandAsync(island);
            return volunteers?
                .Where(v => v.IsSpeciesExpert && v.IsActive)
                .ToList() ?? new List<Volunteer>();
        }

        /// <summary>
        /// Promote volunteer to lead
        /// </summary>
        public async Task<bool> PromoteToLeadAsync(int volunteerId)
        {
            var volunteer = await _apiClient.GetVolunteerByIdAsync(volunteerId);
            if (volunteer == null) return false;

            var updateRequest = new VolunteerApiClient.UpdateVolunteerRequest
            {
                Id = volunteer.ID,
                FirstLast = volunteer.FirstLast,
                FirstName = volunteer.FirstName,
                LastName = volunteer.LastName,
                Email = volunteer.Email,
                Phone = volunteer.Phone,
                CellPhone = volunteer.CellPhone,
                Address = volunteer.Address,
                City = volunteer.City,
                State = volunteer.State,
                Zip = volunteer.Zip,
                Island = volunteer.Island,
                Active = volunteer.IsActive ? 1 : 0,
                Lead = 1, // Promote to lead
                SpeciesExpert = volunteer.IsSpeciesExpert ? 1 : 0,
                StartDate = volunteer.StartDate,
                AppRole = "Admin", // Update role
                VolunteerNotes = $"{volunteer.VolunteerNotes}\nPromoted to lead on {DateTime.Now:yyyy-MM-dd}"
            };

            var updated = await _apiClient.UpdateVolunteerAsync(volunteerId, updateRequest);
            return updated?.IsLead ?? false;
        }

        /// <summary>
        /// Get volunteer statistics by island
        /// </summary>
        public async Task<Dictionary<string, VolunteerStats>> GetVolunteerStatsByIslandAsync()
        {
            var volunteers = await _apiClient.GetAllVolunteersAsync();
            return volunteers?
                .Where(v => !string.IsNullOrEmpty(v.Island))
                .GroupBy(v => v.Island!)
                .ToDictionary(
                    g => g.Key,
                    g => new VolunteerStats
                    {
                        Total = g.Count(),
                        Active = g.Count(v => v.IsActive),
                        Leads = g.Count(v => v.IsLead),
                        SpeciesExperts = g.Count(v => v.IsSpeciesExpert)
                    }
                ) ?? new Dictionary<string, VolunteerStats>();
        }

        public record VolunteerStats
        {
            public int Total { get; init; }
            public int Active { get; init; }
            public int Leads { get; init; }
            public int SpeciesExperts { get; init; }
        }
    }

    public class VolunteerApiExamples
    {
        public static async Task RunExamples()
        {
            // Setup HttpClient
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var client = new VolunteerApiClient(httpClient);

            // Example 1: Get all volunteers
            Console.WriteLine("=== Get All Volunteers ===");
            var volunteers = await client.GetAllVolunteersAsync();
            foreach (var volunteer in volunteers ?? Enumerable.Empty<Volunteer>())
            {
                Console.WriteLine($"Volunteer: {volunteer.FirstLast} (ID: {volunteer.ID}) - Active: {volunteer.IsActive}");
            }

            // Example 2: Create a new volunteer
            Console.WriteLine("\n=== Create New Volunteer ===");
            var newVolunteer = new VolunteerApiClient.CreateVolunteerRequest
            {
                FirstLast = "John Doe",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "360-555-0100",
                CellPhone = "360-555-0101",
                Address = "123 Marine Drive",
                City = "Friday Harbor",
                State = "WA",
                Zip = 98250,
                Island = "San Juan Island",
                Active = 1,
                Lead = 0,
                SpeciesExpert = 0,
                StartDate = DateTime.Now,
                AppRole = "Edit",
                VolunteerNotes = "New volunteer with marine biology background"
            };

            var createdVolunteer = await client.CreateVolunteerAsync(newVolunteer);
            Console.WriteLine($"Created volunteer with ID: {createdVolunteer?.ID}");

            // Example 3: Get volunteer by ID
            Console.WriteLine("\n=== Get Volunteer By ID ===");
            if (createdVolunteer != null)
            {
                var fetchedVolunteer = await client.GetVolunteerByIdAsync(createdVolunteer.ID);
                Console.WriteLine($"Fetched: {fetchedVolunteer?.FirstLast} - Email: {fetchedVolunteer?.Email}");
            }

            // Example 4: Update volunteer
            Console.WriteLine("\n=== Update Volunteer ===");
            if (createdVolunteer != null)
            {
                var updateRequest = new VolunteerApiClient.UpdateVolunteerRequest
                {
                    Id = createdVolunteer.ID,
                    FirstLast = "John Doe",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe.updated@example.com", // Updated email
                    Phone = "360-555-0100",
                    CellPhone = "360-555-0101",
                    Address = "123 Marine Drive",
                    City = "Friday Harbor",
                    State = "WA",
                    Zip = 98250,
                    Island = "Orcas Island", // Changed island
                    Active = 1,
                    Lead = 1, // Promoted to lead
                    SpeciesExpert = 1, // Now a species expert
                    StartDate = DateTime.Now.AddYears(-1),
                    AppRole = "Admin", // Changed role
                    VolunteerNotes = "Promoted to lead volunteer and species expert"
                };

                var updatedVolunteer = await client.UpdateVolunteerAsync(createdVolunteer.ID, updateRequest);
                Console.WriteLine($"Updated: {updatedVolunteer?.FirstLast}");
                Console.WriteLine($"  - Lead: {updatedVolunteer?.IsLead}");
                Console.WriteLine($"  - Species Expert: {updatedVolunteer?.IsSpeciesExpert}");
                Console.WriteLine($"  - Island: {updatedVolunteer?.Island}");
            }

            // Example 5: Get active volunteers
            Console.WriteLine("\n=== Get Active Volunteers ===");
            var activeVolunteers = await client.GetActiveVolunteersAsync();
            Console.WriteLine($"Found {activeVolunteers?.Count ?? 0} active volunteers");

            // Example 6: Get volunteers by island
            Console.WriteLine("\n=== Get Volunteers By Island ===");
            var sanJuanVolunteers = await client.GetVolunteersByIslandAsync("San Juan Island");
            Console.WriteLine($"Found {sanJuanVolunteers?.Count ?? 0} volunteers on San Juan Island");
            foreach (var vol in sanJuanVolunteers ?? Enumerable.Empty<Volunteer>())
            {
                Console.WriteLine($"  - {vol.FirstLast} ({vol.AppRole})");
            }

            // Example 7: Delete volunteer
            Console.WriteLine("\n=== Delete Volunteer ===");
            if (createdVolunteer != null)
            {
                var deleted = await client.DeleteVolunteerAsync(createdVolunteer.ID);
                Console.WriteLine($"Volunteer deleted: {deleted}");
            }
        }

        private static object Volunteer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Example with error handling
        /// </summary>
        public static async Task ExampleWithErrorHandling()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var client = new VolunteerApiClient(httpClient);

            try
            {
                // Try to get a volunteer that doesn't exist
                var volunteer = await client.GetVolunteerByIdAsync(999999);
                if (volunteer == null)
                {
                    Console.WriteLine("Volunteer not found");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error: {ex.Message}");
            }

            try
            {
                // Try to create a volunteer with missing required field
                var invalidVolunteer = new VolunteerApiClient.CreateVolunteerRequest
                {
                    FirstLast = "", // Empty name - should fail validation
                    Email = "test@example.com"
                };

                await client.CreateVolunteerAsync(invalidVolunteer);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Console.WriteLine("Validation error: FirstLast is required");
            }

            try
            {
                // Try to update a non-existent volunteer
                var updateRequest = new VolunteerApiClient.UpdateVolunteerRequest
                {
                    Id = 999999,
                    FirstLast = "Non-existent Volunteer"
                };

                await client.UpdateVolunteerAsync(999999, updateRequest);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
            }
        }

    }
}
