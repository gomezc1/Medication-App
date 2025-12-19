using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Schedule;
using MedicationManager.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedicationManager.Core.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IInteractionService _interactionService;
        private readonly IDosageValidationService _dosageValidationService;
        private readonly ILogger<ScheduleService> _logger;

        private static readonly Dictionary<TimingPreference, TimeSpan> DefaultTimeSlots = new()
        {
            [TimingPreference.Morning] = new TimeSpan(8, 0, 0),
            [TimingPreference.Noon] = new TimeSpan(12, 0, 0),
            [TimingPreference.Evening] = new TimeSpan(18, 0, 0),
            [TimingPreference.Bedtime] = new TimeSpan(22, 0, 0)
        };

        public ScheduleService(
            IInteractionService interactionService,
            IDosageValidationService dosageValidationService,
            ILogger<ScheduleService> logger)
        {
            _interactionService = interactionService;
            _dosageValidationService = dosageValidationService;
            _logger = logger;
        }

        public async Task<DailySchedule> GenerateScheduleAsync(List<UserMedication> medications)
        {
            _logger.LogInformation("Generating schedule for {Count} medications", medications.Count);

            var schedule = new DailySchedule
            {
                GeneratedDate = DateTime.Now
            };

            // Check interactions
            var interactionResult = await _interactionService.CheckInteractionsAsync(medications);
            schedule.Interactions.AddRange(interactionResult.DrugInteractions);
            schedule.DuplicationWarnings.AddRange(interactionResult.DuplicateWarnings);

            // Validate dosages
            var dosageWarnings = await _dosageValidationService.ValidateAllMedicationsAsync(medications);
            schedule.DosageWarnings.AddRange(dosageWarnings);

            // Generate schedule entries
            var entries = GenerateScheduleEntries(medications);
            schedule.Entries.AddRange(entries);

            return schedule;
        }

        private List<ScheduleEntry> GenerateScheduleEntries(List<UserMedication> medications)
        {
            var allDoses = new List<(TimeSpan Time, MedicationDose Dose)>();

            foreach (var medication in medications.Where(m => m.IsActive))
            {
                var times = DetermineScheduleTimes(medication);
                foreach (var time in times)
                {
                    allDoses.Add((time, new MedicationDose
                    {
                        Medication = medication,
                        Amount = medication.UserDose,
                        Unit = medication.UserDoseUnit,
                        Instructions = medication.SpecialInstructions,
                        RequiresFood = medication.WithFood,
                        RequiresEmptyStomach = medication.OnEmptyStomach
                    }));
                }
            }

            // Group by time (within 30-minute windows)
            var grouped = allDoses
                .GroupBy(d => new TimeSpan(d.Time.Hours, (d.Time.Minutes / 30) * 30, 0))
                .OrderBy(g => g.Key)
                .Select(g => new ScheduleEntry
                {
                    Time = g.Key,
                    TimeSlot = DetermineTimeSlot(g.Key),
                    Medications = g.Select(d => d.Dose).ToList(),
                    GeneralInstructions = BuildGeneralInstructions(g.Select(d => d.Dose).ToList())
                })
                .ToList();

            return grouped;
        }

        private List<TimeSpan> DetermineScheduleTimes(UserMedication medication)
        {
            if (medication.SpecificTimes?.Any() == true)
            {
                return medication.SpecificTimes;
            }

            var times = new List<TimeSpan>();
            var preferences = medication.TimingPreferences;

            switch (medication.Frequency)
            {
                case 1:
                    times.Add(preferences.Any() ? DefaultTimeSlots[preferences[0]] : DefaultTimeSlots[TimingPreference.Morning]);
                    break;
                case 2:
                    times.Add(DefaultTimeSlots[TimingPreference.Morning]);
                    times.Add(DefaultTimeSlots[TimingPreference.Evening]);
                    break;
                case 3:
                    times.Add(DefaultTimeSlots[TimingPreference.Morning]);
                    times.Add(DefaultTimeSlots[TimingPreference.Noon]);
                    times.Add(DefaultTimeSlots[TimingPreference.Evening]);
                    break;
                case 4:
                    times.Add(DefaultTimeSlots[TimingPreference.Morning]);
                    times.Add(DefaultTimeSlots[TimingPreference.Noon]);
                    times.Add(DefaultTimeSlots[TimingPreference.Evening]);
                    times.Add(DefaultTimeSlots[TimingPreference.Bedtime]);
                    break;
                default:
                    var interval = TimeSpan.FromHours(24.0 / medication.Frequency);
                    for (int i = 0; i < medication.Frequency; i++)
                    {
                        times.Add(TimeSpan.FromHours(8 + (i * interval.TotalHours)));
                    }
                    break;
            }

            return times;
        }

        private TimingPreference DetermineTimeSlot(TimeSpan time)
        {
            var hour = time.Hours;
            return hour switch
            {
                >= 6 and < 11 => TimingPreference.Morning,
                >= 11 and < 14 => TimingPreference.Noon,
                >= 14 and < 20 => TimingPreference.Evening,
                _ => TimingPreference.Bedtime
            };
        }

        private string BuildGeneralInstructions(List<MedicationDose> medications)
        {
            var withFood = medications.Count(m => m.RequiresFood);
            var emptyStomach = medications.Count(m => m.RequiresEmptyStomach);

            if (emptyStomach > 0 && withFood > 0)
                return "Take medications on empty stomach first, then wait 30 minutes before taking with food.";
            if (withFood > 0)
                return "Take with food or a meal.";
            if (emptyStomach > 0)
                return "Take on empty stomach (1 hour before or 2 hours after meals).";

            return "";
        }

        public async Task<byte[]> ExportScheduleToPdfAsync(DailySchedule schedule)
        {
            _logger.LogInformation("Exporting schedule to PDF");

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add disclaimer
            var disclaimer = new Paragraph("⚠️ EDUCATIONAL PROTOTYPE - NOT FOR MEDICAL DECISION MAKING\n" +
                "This schedule is for educational purposes only. Always consult your healthcare provider.")
                .SetFontColor(ColorConstants.RED)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(disclaimer);

            // Add title
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var italicFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_OBLIQUE);

            var title = new Paragraph("Daily Medication Schedule")
                .SetFontSize(18)
                .SetFont(boldFont)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Add date
            var date = new Paragraph($"Generated: {schedule.GeneratedDate:MMMM d, yyyy 'at' h:mm tt}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(date);

            // Add schedule entries
            foreach (var entry in schedule.Entries.OrderBy(e => e.Time))
            {
                var timeHeader = new Paragraph($"{entry.FormattedTime} - {entry.TimeSlot}")
                    .SetFontSize(14)
                    .SetFont(boldFont)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetPadding(5)
                    .SetMarginTop(10);
                document.Add(timeHeader);

                foreach (var med in entry.Medications)
                {
                    var medText = $"• {med.DisplayName} - {med.FormattedDose}";
                    if (!string.IsNullOrEmpty(med.Instructions))
                        medText += $"\n  {med.Instructions}";

                    document.Add(new Paragraph(medText).SetMarginLeft(10).SetFontSize(11));
                }

                if (!string.IsNullOrEmpty(entry.GeneralInstructions))
                {
                    document.Add(new Paragraph(entry.GeneralInstructions)
                        .SetFont(italicFont)
                        .SetMarginLeft(10)
                        .SetFontSize(10));
                }
            }

            // Add warnings if any
            if (schedule.HasWarnings)
            {
                document.Add(new Paragraph("\n⚠️ IMPORTANT WARNINGS")
                    .SetFontSize(14)
                    .SetFont(boldFont)
                    .SetFontColor(ColorConstants.RED)
                    .SetMarginTop(20));

                foreach (var interaction in schedule.Interactions)
                {
                    document.Add(new Paragraph($"• {interaction.SeverityLevel} Interaction: {interaction.Drug1Name} and {interaction.Drug2Name}")
                        .SetFontColor(ColorConstants.RED)
                        .SetMarginLeft(10));
                }
            }

            document.Close();
            return memoryStream.ToArray();
        }
    }
}