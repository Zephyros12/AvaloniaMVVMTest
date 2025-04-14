using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaMVVMTest.ViewModels;
using System.Threading.Tasks;

namespace AvaloniaMVVMTest.Views
{
    public partial class CameraSelectionView : Window
    {
        public CameraSelectionView()
        {
            InitializeComponent();
        }

        public async Task<string?> ShowSelectionDialogAsync(Window owner, CameraSelectionViewModel viewModel)
        {
            Owner = owner;
            DataContext = viewModel;

            var result = await ShowDialog<string?>(owner);
            return result;
        }

        private void OnConfirmClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CameraSelectionViewModel vm)
            {
                Close(vm.SelectedSerial);
            }
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
