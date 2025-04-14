using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaMVVMTest.ViewModels;

namespace AvaloniaMVVMTest.Views
{
    public partial class CameraView : UserControl
    {
        public CameraView()
        {
            InitializeComponent();
        }

        private async void OnOpenCameraClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CameraViewModel vm && VisualRoot is Window parent)
            {
                await vm.OpenCameraDialogAsync(parent);
            }
        }
    }
}
