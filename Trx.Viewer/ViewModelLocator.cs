using Microsoft.Extensions.DependencyInjection;
using Trx.Viewer.Ui.ViewModels;

namespace Trx.Viewer.Ui
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => App.ServiceProvider.GetRequiredService<MainViewModel>();
    }
}
