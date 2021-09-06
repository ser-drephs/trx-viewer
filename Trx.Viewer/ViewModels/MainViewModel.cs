using GalaSoft.MvvmLight;
using Microsoft.Extensions.Logging;

namespace Trx.Viewer.Ui.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(ILogger<MainViewModel> logger)
        {
            Logger = logger;
        }

        public ILogger<MainViewModel> Logger { get; }
    }
}
