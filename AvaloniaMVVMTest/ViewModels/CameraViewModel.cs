using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Basler.Pylon;
using ReactiveUI;

namespace AvaloniaMVVMTest.ViewModels
{
    public sealed class CameraViewModel : ViewModelBase
    {
        private Camera? _camera;
        private PixelDataConverter? _converter;

        private Bitmap? _previewImage;
        public Bitmap? PreviewImage
        {
            get => _previewImage;
            private set => this.RaiseAndSetIfChanged(ref _previewImage, value);
        }

        public async Task OpenCameraDialogAsync(Window owner)
        {
            var infos = CameraFinder.Enumerate();
            if (infos.Count == 0)
            {
                await ShowDialogAsync(owner, "No Basler camera found.");
                return;
            }

            var dialog = new Views.CameraSelectionView();
            var vm = new CameraSelectionViewModel(infos.Select(info => info[CameraInfoKey.SerialNumber]));
            string? selectedSerial = await dialog.ShowSelectionDialogAsync(owner, vm);

            if (!string.IsNullOrWhiteSpace(selectedSerial))
            {
                StartCamera(selectedSerial);
            }
        }

        private void StartCamera(string serialNumber)
        {
            try
            {
                _camera = new Camera(serialNumber);
                _camera.Open();

                _converter = new PixelDataConverter
                {
                    OutputPixelFormat = PixelType.BGRA8packed
                };

                _camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                _camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"카메라 시작 오류: {ex.Message}");
            }
        }

        private void OnImageGrabbed(object? sender, ImageGrabbedEventArgs e)
        {
            if (!e.GrabResult.GrabSucceeded || _converter == null)
            {
                return;
            }

            IGrabResult grabResult = e.GrabResult;
            int width = grabResult.Width;
            int height = grabResult.Height;
            int stride = width * 4;

            byte[] buffer = new byte[_converter.GetBufferSizeForConversion(grabResult)];
            _converter.Convert(buffer, grabResult);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();

            Dispatcher.UIThread.Post(() =>
            {
                using var bitmap = new Bitmap(
                    PixelFormat.Bgra8888,
                    AlphaFormat.Unpremul,
                    pointer,
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    stride);

                using var stream = new MemoryStream();
                bitmap.Save(stream);
                stream.Position = 0;

                PreviewImage?.Dispose();
                PreviewImage = new Bitmap(stream);
            });

            handle.Free();
        }

        private async Task ShowDialogAsync(Window parent, string message)
        {
            var dialog = new Window
            {
                Title = "Notice",
                Width = 300,
                Height = 150
            };

            var button = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            button.Click += (_, _) => dialog.Close();

            dialog.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Children =
        {
            new TextBlock { Text = message },
            button
        }
            };

            await dialog.ShowDialog(parent);
        }

        public void StopCamera()
        {
            if (_camera is { StreamGrabber.IsGrabbing: true })
            {
                _camera.StreamGrabber.Stop();
                _camera.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
            }

            _camera?.Close();
            _camera?.Dispose();
            _camera = null;

            PreviewImage?.Dispose();
            PreviewImage = null;
        }
    }
}
