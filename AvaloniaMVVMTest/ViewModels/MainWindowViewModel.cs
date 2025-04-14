using Autofac;
using AvaloniaMVVMTest.ViewModels;

namespace AvaloniaMVVMTest.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        public CameraViewModel CameraViewModel { get; }

        public MainWindowViewModel()
        {
            using var scope = App.Container.BeginLifetimeScope();
            CameraViewModel = scope.Resolve<CameraViewModel>();
        }
    }
}
