using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using Trx.Viewer.Ui.ViewModels;
using Trx.Viewer.Ui.Views;

namespace Trx.Viewer.Ui
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddViews(this IServiceCollection services) =>
            services.AddTransient<MainWindow>();

        public static IServiceCollection AddViewModels(this IServiceCollection services) =>
            services.AddSingleton<MainViewModel>();

        public static IServiceCollection AddServices(this IServiceCollection services) =>
            services.AddSingleton<IFileSystem, FileSystem>();

        public static IServiceCollection AddEvents(this IServiceCollection services) =>
            services;
    }
}
