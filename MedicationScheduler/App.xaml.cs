using MedicationManager.Core.Models;
using MedicationManager.Core.Services;
using MedicationManager.Core.Services.Interfaces;
using MedicationManager.Infrastructure.Data;
using MedicationManager.Infrastructure.ExternalServices;
using MedicationScheduler.Services;
using MedicationScheduler.Services.Interfaces;
using MedicationScheduler.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;

namespace MedicationScheduler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        public App()
        {
            // Surface any unhandled UI exceptions so we can see them
            this.DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "Unhandled exception");
                e.Handled = true; // so the process doesn't immediately die
            };
            ConfigureSerilog();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {

            _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // add additional configuration sources if needed
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Register external API services with caching
                    services.AddExternalApiServicesWithCache();
                    // add API Health monitor
                    services.AddSingleton<ApiHealthMonitor>();

                    // Register database
                    // add additional configuration sources if needed
                    // get the user's application support directory
                    var appSupportDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "MedicationScheduler");
                    if (!Directory.Exists(appSupportDir))
                    {
                        Directory.CreateDirectory(appSupportDir);
                    }
                    var dbPath = Path.Combine(appSupportDir, "medications.db");
                    var connectionString = $"Data Source={dbPath}";
                    
                    services.AddDbContext<MedicationDbContext>(options =>
                    {
                        options.UseSqlite(connectionString);
                        options.EnableSensitiveDataLogging(true);
                        options.EnableDetailedErrors(true);
                        options.EnableThreadSafetyChecks(true);
                    });
                    services.AddScoped<DbInitializer>();

                    // Repositories & Services
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<IMedicationService, MedicationService>();
                    services.AddScoped<IInteractionService, InteractionService>();
                    services.AddScoped<INavigationService, NavigationService>();
                    services.AddScoped<IDialogService, DialogService>();
                    services.AddScoped<IScheduleService, ScheduleService>();
                    services.AddScoped<IDosageValidationService, DosageValidationService>();
                    services.AddScoped<IDataSeedService, DataSeedService>();

                    // add logging
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddSerilog(dispose: true);
                    });

                    // View Models
                    services.AddTransient<MainViewModel>();

                    // inject main window and UI

                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<MedicationListViewModel>();
                    services.AddTransient<AddEditMedicationViewModel>();
                    services.AddTransient<ScheduleViewModel>();
                    services.AddTransient<InteractionWarningsViewModel>();

                    services.AddSingleton<MainWindow>();

                })
                .Build();

            // Init database
            await InitializeDatabaseAsync();

            // Show disclaimer dialog
            if (!await ShowDisclaimerAsync())
            {
                Shutdown();
                return;
            }

            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_host != null)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                    _host.Dispose();
                }

                Log.CloseAndFlush();
            }
            finally
            {
                base.OnExit(e);
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                using var scope = _host!.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MedicationDbContext>();
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();

                Log.Information("Initializing database...");
                await dbInitializer.InitializeAsync();

                // Seed initial data if needed
                //var seedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();
                //await seedService.SeedInitialDataAsync();

                Log.Information("Database initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing database");
                throw;
            }
        }

        private void ConfigureSerilog()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MedicationScheduler",
                "Logs");

            Directory.CreateDirectory(logDirectory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(logDirectory, "medication-manager-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug()
                .CreateLogger();

            Log.Information("Application starting...");
        }

        private async Task<bool> ShowDisclaimerAsync()
        {
            // Check if user has already accepted disclaimer
            using var scope = _host!.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MedicationDbContext>();

            var disclaimerSetting = await dbContext.Set<AppSetting>()
                .FirstOrDefaultAsync(s => s.SettingKey == "DisclaimerAccepted");

            if (disclaimerSetting?.SettingValue == "true")
            {
                return true; // Already accepted
            }

            // Show disclaimer dialog
            var result = MessageBox.Show(
                "⚠️ EDUCATIONAL PROTOTYPE - NOT FOR MEDICAL DECISION MAKING\n\n" +
                "This application is an educational prototype and is not intended for real medical decision-making. " +
                "Always consult your healthcare provider before making any changes to your medication regimen.\n\n" +
                "This application is:\n" +
                "• NOT FDA-approved\n" +
                "• NOT HIPAA-compliant\n" +
                "• NOT a substitute for professional medical advice\n\n" +
                "By clicking 'I Understand', you acknowledge that you will NOT use this application for actual medical decisions.\n\n" +
                "Do you understand and accept these terms?",
                "Important Medical Disclaimer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                // Save acceptance
                if (disclaimerSetting == null)
                {
                    disclaimerSetting = new AppSetting
                    {
                        SettingKey = "DisclaimerAccepted",
                        SettingValue = "true",
                        SettingType = "bool"
                    };
                    dbContext.Set<AppSetting>().Add(disclaimerSetting);
                }
                else
                {
                    disclaimerSetting.SettingValue = "true";
                    disclaimerSetting.ModifiedDate = DateTime.Now;
                }

                await dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Log.Information("Application started");
        }

        /// <summary>
        /// Gets a service from the DI container
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (Current is App app && app._host != null)
            {
                return app._host.Services.GetRequiredService<T>();
            }

            throw new InvalidOperationException("Application host is not initialized");
        }
    }



}
