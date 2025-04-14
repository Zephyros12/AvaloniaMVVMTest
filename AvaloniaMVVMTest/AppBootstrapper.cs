using Autofac;
using AvaloniaMVVMTest.Services;
using AvaloniaMVVMTest.ViewModels;

namespace AvaloniaMVVMTest
{
    public static class AppBootstrapper
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<BaslerCameraService>()
                   .As<ICameraService>()
                   .SingleInstance();

            builder.RegisterType<CameraViewModel>().AsSelf();
            builder.RegisterType<MainWindowViewModel>().AsSelf();

            return builder.Build();
        }
    }
}
