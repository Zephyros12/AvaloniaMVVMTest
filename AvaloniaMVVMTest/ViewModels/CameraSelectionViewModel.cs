using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;

namespace AvaloniaMVVMTest.ViewModels
{
    public sealed class CameraSelectionViewModel : ViewModelBase
    {
        public ObservableCollection<string> AvailableSerials { get; } = new();

        private string? _selectedSerial;
        public string? SelectedSerial
        {
            get => _selectedSerial;
            set => this.RaiseAndSetIfChanged(ref _selectedSerial, value);
        }

        public ReactiveCommand<Unit, string?> ConfirmCommand { get; }

        public CameraSelectionViewModel(IEnumerable<string> serials)
        {
            foreach (var serial in serials)
            {
                AvailableSerials.Add(serial);
            }

            ConfirmCommand = ReactiveCommand.Create(() => SelectedSerial);
        }
    }
}
