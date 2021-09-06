using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Windows;
using Trx.Viewer.Ui.Views;

namespace Trx.Viewer.Ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            try
            {
                host = Host.CreateDefaultBuilder()
                    .UseSerilog((hostingContext, loggerConfiguration) => {
                        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                    })
                    .ConfigureServices(services => {
                        ConfigureServices(services);
                    })
                    .Build();
                ServiceProvider = host.Services;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application Start Failed");
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddViews();
                services.AddViewModels();
                services.AddServices();
                services.AddEvents();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();

            var window = ServiceProvider.GetRequiredService<MainWindow>();
            window.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}
