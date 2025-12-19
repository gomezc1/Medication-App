using MedicationManager.Core.Models;
using MedicationManager.Core.Models.DTOs;
using MedicationManager.Core.Models.Exceptions;
using MedicationManager.Core.Services.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MedicationManager.Core.Services
{
    /// <summary>
    /// Service for managing medications and user medication configurations.
    /// Coordinates between external APIs (RxNorm, OpenFDA) and local storage.
    /// </summary>
    public class MedicationService : IMedicationService
    {
        private readonly IRepository<Medication> _medicationRepository;
        private readonly IRepository<UserMedication> _userMedicationRepository;
        private readonly IRepository<MedicationSet> _medicationSetRepository;
        private readonly IOpenFdaApiService _openFdaService;
        private readonly IRxNormApiService _rxNormService;
        private readonly ILogger<MedicationService> _logger;

        public MedicationService(
            IRepository<Medication> medicationRepository,
            IRepository<UserMedication> userMedicationRepository,
            IRepository<MedicationSet> medicationSetRepository,
            IOpenFdaApiService openFdaService,
            IRxNormApiService rxNormService,
            ILogger<MedicationService> logger)
        {
            _medicationRepository = medicationRepository;
            _userMedicationRepository = userMedicationRepository;
            _medicationSetRepository = medicationSetRepository;
            _openFdaService = openFdaService;
            _rxNormService = rxNormService;
            _logger = logger;
        }

        #region Search Operations

        /// <summary>
        /// Search for medications across local database and external APIs
        /// </summary>
        public async Task<List<MedicationSearchResult>> SearchMedicationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<MedicationSearchResult>();
            }

            _logger.LogInformation("Searching for medications with term: {SearchTerm}", searchTerm);

            var results = new List<MedicationSearchResult>();

            try
            {
                // 1. Search local database first (fastest)
                var localResults = await SearchLocalDatabaseAsync(searchTerm);
                results.AddRange(localResults);

                //2.Search RxNorm API(most comprehensive drug database)
                var rxNormResults = await SearchRxNormApiAsync(searchTerm);

                // Only add RxNorm results that aren't already in local results
                foreach (var rxResult in rxNormResults)
                {
                    if (!results.Any(r => r.Medication.RxCui == rxResult.Medication.RxCui))
                    {
                        results.Add(rxResult);
                    }
                }

                // 3. Search OpenFDA API (for additional brand names and manufacturer info)
                var fdaResults = await SearchOpenFdaApiAsync(searchTerm);

                // Enrich existing results with FDA data or add new ones
                foreach (var fdaResult in fdaResults)
                {
                    var existing = results.FirstOrDefault(r =>
                        r.Medication.Name.Equals(fdaResult.Medication.Name, StringComparison.OrdinalIgnoreCase) ||
                        r.Medication.RxCui == fdaResult.Medication.RxCui);

                    if (existing != null)
                    {
                        // Enrich existing result with FDA data
                        EnrichMedicationWithFdaData(existing.Medication, fdaResult.Medication);
                    }
                    else
                    {
                        // Add as new result
                        results.Add(fdaResult);
                    }
                }

                // Sort by relevance (higher score first)
                results = results
                    .OrderByDescending(r => r.Relevance)
                    .Take(20) // Limit to top 20 results
                    .ToList();

                _logger.LogInformation("Found {Count} medication results for term: {SearchTerm}",
                    results.Count, searchTerm);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for medications with term: {SearchTerm}", searchTerm);

                // Return local results even if API fails
                return results.Any() ? results : new List<MedicationSearchResult>();
            }
        }

        private async Task<List<MedicationSearchResult>> SearchLocalDatabaseAsync(string searchTerm)
        {
            var lowerTerm = searchTerm.ToLowerInvariant();

            var medications = await _medicationRepository.FindAsync(m =>
                m.Name.ToLower().Contains(lowerTerm) ||
                m.GenericName.ToLower().Contains(lowerTerm));

            return medications.Select(m => new MedicationSearchResult
            {
                Medication = m,
                Source = "Local Database",
                Relevance = CalculateRelevance(m, searchTerm)
            }).ToList();
        }

        private async Task<List<MedicationSearchResult>> SearchRxNormApiAsync(string searchTerm)
        {
            try
            {
                var candidates = await _rxNormService.SearchApproximateMatchAsync(searchTerm);
                var results = new List<MedicationSearchResult>();

                foreach (var candidate in candidates.Take(10))
                {
                    // Get detailed information for each candidate
                    var details = await _rxNormService.GetRxCuiDetailsAsync(candidate.RxCui);
                    var ingredients = await _rxNormService.GetActiveIngredientsAsync(candidate.RxCui);

                    var medication = new Medication
                    {
                        RxCui = candidate.RxCui,
                        Name = candidate.Name,
                        GenericName = details?.Name ?? candidate.Name,
                        ActiveIngredients = ingredients,
                        DataSource = "RxNorm",
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    results.Add(new MedicationSearchResult
                    {
                        Medication = medication,
                        Source = "RxNorm API",
                        Relevance = candidate.Score
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error searching RxNorm API for term: {SearchTerm}", searchTerm);
                return new List<MedicationSearchResult>();
            }
        }

        private async Task<List<MedicationSearchResult>> SearchOpenFdaApiAsync(string searchTerm)
        {
            try
            {
                var fdaResponse = await _openFdaService.SearchDrugsAsync(searchTerm, 5);
                var results = new List<MedicationSearchResult>();

                if (fdaResponse.Results == null)
                {
                    return results;
                }

                foreach (var fdaResult in fdaResponse.Results)
                {
                    var openFda = fdaResult.OpenFda;
                    if (openFda == null)
                        continue;

                    var brandName = openFda.BrandName?.FirstOrDefault() ?? "";
                    var genericName = openFda.GenericName?.FirstOrDefault() ?? "";
                    var rxCui = openFda.RxCui?.FirstOrDefault() ?? "";

                    if (string.IsNullOrEmpty(brandName))
                        continue;

                    var medication = new Medication
                    {
                        RxCui = rxCui,
                        Name = brandName,
                        GenericName = genericName,
                        ActiveIngredients = openFda.SubstanceName?.ToList() ?? new List<string>(),
                        Manufacturer = openFda.ManufacturerName?.FirstOrDefault() ?? "",
                        Route = openFda.Route?.FirstOrDefault() ?? "",
                        NDC = openFda.ProductNdc?.FirstOrDefault() ?? "",
                        IsOTC = openFda.ProductType?.Any(pt => pt.Contains("OTC", StringComparison.OrdinalIgnoreCase)) ?? false,
                        DataSource = "OpenFDA",
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    results.Add(new MedicationSearchResult
                    {
                        Medication = medication,
                        Source = "OpenFDA API",
                        Relevance = CalculateRelevance(medication, searchTerm)
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error searching OpenFDA API for term: {SearchTerm}", searchTerm);
                return new List<MedicationSearchResult>();
            }
        }

        private void EnrichMedicationWithFdaData(Medication target, Medication source)
        {
            // Enrich with FDA data if not already present
            if (string.IsNullOrEmpty(target.Manufacturer) && !string.IsNullOrEmpty(source.Manufacturer))
                target.Manufacturer = source.Manufacturer;

            if (string.IsNullOrEmpty(target.NDC) && !string.IsNullOrEmpty(source.NDC))
                target.NDC = source.NDC;

            if (string.IsNullOrEmpty(target.Route) && !string.IsNullOrEmpty(source.Route))
                target.Route = source.Route;

            if (!target.ActiveIngredients.Any() && source.ActiveIngredients.Any())
                target.ActiveIngredients = source.ActiveIngredients;

            // Update OTC flag if FDA says it's OTC
            if (source.IsOTC)
                target.IsOTC = true;
        }

        private int CalculateRelevance(Medication medication, string searchTerm)
        {
            var score = 0;
            var term = searchTerm.ToLowerInvariant();
            var name = medication.Name.ToLowerInvariant();
            var genericName = medication.GenericName?.ToLowerInvariant() ?? "";

            // Exact match gets highest score
            if (name == term || genericName == term)
                score += 100;
            // Starts with gets high score
            else if (name.StartsWith(term) || genericName.StartsWith(term))
                score += 80;
            // Contains gets medium score
            else if (name.Contains(term) || genericName.Contains(term))
                score += 50;

            // Prefer brand names over generics for display
            if (!string.IsNullOrEmpty(medication.Name))
                score += 10;

            // Prefer local database results (already validated)
            if (medication.Id > 0)
                score += 20;

            return score;
        }

        public async Task<Medication?> GetMedicationByRxCuiAsync(string rxCui)
        {
            // Check local database first
            var medication = await _medicationRepository.FindFirstAsync(m => m.RxCui == rxCui);

            if (medication != null)
            {
                return medication;
            }

            // If not in database, fetch from APIs and save
            return await FetchAndSaveMedicationAsync(rxCui);
        }

        public async Task<Medication?> GetMedicationByIdAsync(int id)
        {
            return await _medicationRepository.GetByIdAsync(id);
        }

        private async Task<Medication?> FetchAndSaveMedicationAsync(string rxCui)
        {
            try
            {
                _logger.LogInformation("Fetching medication details for RxCui: {RxCui}", rxCui);

                // Get details from RxNorm
                var details = await _rxNormService.GetRxCuiDetailsAsync(rxCui);
                if (details == null)
                {
                    return null;
                }

                var ingredients = await _rxNormService.GetActiveIngredientsAsync(rxCui);

                var medication = new Medication
                {
                    RxCui = rxCui,
                    Name = details.Name,
                    GenericName = details.Name,
                    ActiveIngredients = ingredients,
                    DataSource = "RxNorm",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                // Try to enrich with OpenFDA data
                try
                {
                    var fdaResponse = await _openFdaService.SearchByRxCuiAsync(rxCui);
                    if (fdaResponse.Results?.Any() == true)
                    {
                        var fdaResult = fdaResponse.Results.First();
                        var openFda = fdaResult.OpenFda;

                        if (openFda != null)
                        {
                            medication.Name = openFda.BrandName?.FirstOrDefault() ?? medication.Name;
                            medication.Manufacturer = openFda.ManufacturerName?.FirstOrDefault() ?? "";
                            medication.NDC = openFda.ProductNdc?.FirstOrDefault() ?? "";
                            medication.Route = openFda.Route?.FirstOrDefault() ?? "";
                            medication.IsOTC = openFda.ProductType?.Any(pt => pt.Contains("OTC", StringComparison.OrdinalIgnoreCase)) ?? false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to enrich medication with OpenFDA data for RxCui: {RxCui}", rxCui);
                }

                // Save to local database
                medication = await _medicationRepository.AddAsync(medication);

                _logger.LogInformation("Saved new medication to database: {Name} (RxCui: {RxCui})",
                    medication.Name, rxCui);

                return medication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching medication for RxCui: {RxCui}", rxCui);
                return null;
            }
        }

        #endregion

        #region User Medication Operations (CRUD)

        public async Task<UserMedication> AddUserMedicationAsync(AddUserMedicationRequest request)
        {
            _logger.LogInformation("Adding user medication for RxCui: {RxCui}", request.RxCui);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.RxCui))
            {
                throw new ValidationException("RxCui is required");
            }

            if (request.Dose <= 0)
            {
                throw new ValidationException("Dose must be greater than 0");
            }

            if (request.Frequency < 1 || request.Frequency > 8)
            {
                throw new ValidationException("Frequency must be between 1 and 8 times per day");
            }

            if (request.WithFood && request.OnEmptyStomach)
            {
                throw new ValidationException("Medication cannot be both with food and on empty stomach");
            }

            // Ensure medication exists in database
            var medication = await GetMedicationByRxCuiAsync(request.RxCui);
            if (medication == null)
            {
                throw new NotFoundException("Medication", request.RxCui);
            }

            var userMedication = new UserMedication
            {
                MedicationId = medication.Id,
                Medication = medication,
                UserDose = request.Dose,
                UserDoseUnit = request.DoseUnit,
                Frequency = request.Frequency,
                TimingPreferences = request.TimingPreferences,
                SpecificTimes = request.SpecificTimes,
                WithFood = request.WithFood,
                OnEmptyStomach = request.OnEmptyStomach,
                SpecialInstructions = request.SpecialInstructions,
                StartDate = request.StartDate ?? DateTime.Now,
                IsActive = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            userMedication = await _userMedicationRepository.AddAsync(userMedication);

            _logger.LogInformation("Added user medication: {Name} (ID: {Id})",
                medication.Name, userMedication.Id);

            return userMedication;
        }

        public async Task<UserMedication> UpdateUserMedicationAsync(int id, UpdateUserMedicationRequest request)
        {
            _logger.LogInformation("Updating user medication ID: {Id}", id);

            var userMedication = await _userMedicationRepository.GetByIdAsync(id);
            if (userMedication == null)
            {
                throw new NotFoundException("User medication", id);
            }

            // Validate
            if (request.Dose <= 0)
            {
                throw new ValidationException("Dose must be greater than 0");
            }

            if (request.Frequency < 1 || request.Frequency > 8)
            {
                throw new ValidationException("Frequency must be between 1 and 8 times per day");
            }

            if (request.WithFood && request.OnEmptyStomach)
            {
                throw new ValidationException("Medication cannot be both with food and on empty stomach");
            }

            // Update properties
            userMedication.UserDose = request.Dose;
            userMedication.UserDoseUnit = request.DoseUnit;
            userMedication.Frequency = request.Frequency;
            userMedication.TimingPreferences = request.TimingPreferences;
            userMedication.SpecificTimes = request.SpecificTimes;
            userMedication.WithFood = request.WithFood;
            userMedication.OnEmptyStomach = request.OnEmptyStomach;
            userMedication.SpecialInstructions = request.SpecialInstructions;
            userMedication.ModifiedDate = DateTime.Now;
            userMedication.IsActive = request.IsActive;

            userMedication = await _userMedicationRepository.UpdateAsync(userMedication);

            _logger.LogInformation("Updated user medication ID: {Id}", id);

            return userMedication;
        }

        public async Task<bool> DeleteUserMedicationAsync(int id)
        {
            _logger.LogInformation("Deleting user medication ID: {Id}", id);

            var userMedication = await _userMedicationRepository.GetByIdAsync(id);
            if (userMedication == null)
            {
                return false;
            }

            // Soft delete - mark as inactive
            userMedication.IsActive = false;
            userMedication.EndDate = DateTime.Now;
            userMedication.ModifiedDate = DateTime.Now;

            await _userMedicationRepository.UpdateAsync(userMedication);

            _logger.LogInformation("Deleted (soft) user medication ID: {Id}", id);

            return true;
        }

        public async Task<List<UserMedication>> GetActiveUserMedicationsAsync()
        {
            var medications = await _userMedicationRepository.FindAsync(m => m.IsActive);

            // Load related medication data
            foreach (var med in medications)
            {
                if (med.Medication == null)
                {
                    med.Medication = await _medicationRepository.GetByIdAsync(med.MedicationId) ?? new Medication();
                }
            }

            return medications;
        }

        public async Task<List<UserMedication>> GetAllUserMedicationsAsync()
        {
            var medications = await _userMedicationRepository.GetAllAsync();

            // Load related medication data
            foreach (var med in medications)
            {
                if (med.Medication == null)
                {
                    med.Medication = await _medicationRepository.GetByIdAsync(med.MedicationId) ?? new Medication();
                }
            }

            return medications;
        }

        public async Task<UserMedication?> GetUserMedicationByIdAsync(int id)
        {
            var userMedication = await _userMedicationRepository.GetByIdAsync(id);

            if (userMedication != null && userMedication.Medication == null)
            {
                userMedication.Medication = await _medicationRepository.GetByIdAsync(userMedication.MedicationId) ?? new Medication();
            }

            return userMedication;
        }

        #endregion

        #region Medication Set Operations

        public async Task<MedicationSet> SaveMedicationSetAsync(string name, string description, List<UserMedication> medications)
        {
            _logger.LogInformation("Saving medication set: {Name}", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("Medication set name is required");
            }

            // Serialize medications to JSON
            var medicationData = medications.Select(m => new
            {
                RxCui = m.Medication.RxCui,
                MedicationName = m.Medication.Name,
                m.UserDose,
                m.UserDoseUnit,
                m.Frequency,
                TimingPreferences = m.TimingPreferences.Select(tp => tp.ToString()).ToList(),
                SpecificTimes = m.SpecificTimes.Select(t => t.ToString(@"hh\:mm")).ToList(),
                m.WithFood,
                m.OnEmptyStomach,
                m.SpecialInstructions
            }).ToList();

            var medicationSet = new MedicationSet
            {
                Name = name,
                Description = description,
                MedicationData = JsonSerializer.Serialize(medicationData),
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            medicationSet = await _medicationSetRepository.AddAsync(medicationSet);

            _logger.LogInformation("Saved medication set: {Name} (ID: {Id})", name, medicationSet.Id);

            return medicationSet;
        }

        public async Task<List<MedicationSet>> GetSavedMedicationSetsAsync()
        {
            return await _medicationSetRepository.GetAllAsync();
        }

        public async Task<MedicationSet?> GetMedicationSetByIdAsync(int id)
        {
            return await _medicationSetRepository.GetByIdAsync(id);
        }

        public async Task<List<UserMedication>> LoadMedicationSetAsync(int setId)
        {
            _logger.LogInformation("Loading medication set ID: {SetId}", setId);

            var medicationSet = await _medicationSetRepository.GetByIdAsync(setId);
            if (medicationSet == null)
            {
                throw new NotFoundException("Medication set", setId);
            }

            // Deserialize medication data
            var medicationDataList = JsonSerializer.Deserialize<List<JsonElement>>(medicationSet.MedicationData);
            if (medicationDataList == null)
            {
                return new List<UserMedication>();
            }

            var userMedications = new List<UserMedication>();

            foreach (var data in medicationDataList)
            {
                var rxCui = data.GetProperty("RxCui").GetString() ?? "";
                var medication = await GetMedicationByRxCuiAsync(rxCui);

                if (medication == null)
                    continue;

                var request = new AddUserMedicationRequest
                {
                    RxCui = rxCui,
                    Dose = data.GetProperty("UserDose").GetDecimal(),
                    DoseUnit = data.GetProperty("UserDoseUnit").GetString() ?? "",
                    Frequency = data.GetProperty("Frequency").GetInt32(),
                    TimingPreferences = data.GetProperty("TimingPreferences")
                        .EnumerateArray()
                        .Select(e => Enum.Parse<TimingPreference>(e.GetString() ?? "Morning"))
                        .ToList(),
                    SpecificTimes = data.GetProperty("SpecificTimes")
                        .EnumerateArray()
                        .Select(e => TimeSpan.Parse(e.GetString() ?? "08:00"))
                        .ToList(),
                    WithFood = data.GetProperty("WithFood").GetBoolean(),
                    OnEmptyStomach = data.GetProperty("OnEmptyStomach").GetBoolean(),
                    SpecialInstructions = data.GetProperty("SpecialInstructions").GetString() ?? ""
                };

                var userMedication = await AddUserMedicationAsync(request);
                userMedications.Add(userMedication);
            }

            _logger.LogInformation("Loaded {Count} medications from set ID: {SetId}",
                userMedications.Count, setId);

            return userMedications;
        }

        public async Task<bool> DeleteMedicationSetAsync(int id)
        {
            _logger.LogInformation("Deleting medication set ID: {Id}", id);
            return await _medicationSetRepository.DeleteAsync(id);
        }

        #endregion
    }
}