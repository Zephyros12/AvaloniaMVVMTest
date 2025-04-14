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
        public Camera? Camera { get; set; }
        public PixelDataConverter? Converter { get; set; }
        public DateTime LastFrameTime { get; set; } = DateTime.MinValue;
        public static TimeSpan MinInterval { get; } = TimeSpan.FromMilliseconds(33);

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
                Camera = new Camera(serialNumber);
                Camera.Open();

                Converter = new PixelDataConverter
                {
                    OutputPixelFormat = PixelType.BGRA8packed
                };

                Camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                Camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"카메라 시작 오류: {ex.Message}");
            }
        }

        private void OnImageGrabbed(object? sender, ImageGrabbedEventArgs e)
        {
            if (!e.GrabResult.GrabSucceeded || Converter == null)
                return;

            if (DateTime.Now - LastFrameTime < MinInterval)
                return;
            LastFrameTime = DateTime.Now;

            IGrabResult grabResult = e.GrabResult;
            int width = grabResult.Width;
            int height = grabResult.Height;
            int stride = width * 4;

            byte[] buffer = new byte[Converter.GetBufferSizeForConversion(grabResult)];
            Converter.Convert(buffer, grabResult);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();

            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    PreviewImage?.Dispose();
                    PreviewImage = new Bitmap(
                        PixelFormat.Bgra8888,
                        AlphaFormat.Unpremul,
                        ptr,
                        new PixelSize(width, height),
                        new Vector(96, 96),
                        stride);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bitmap 렌더링 오류: {ex.Message}");
                }
                finally
                {
                    handle.Free();
                }
            });
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
            if (Camera is { StreamGrabber.IsGrabbing: true })
            {
                Camera.StreamGrabber.Stop();
                Camera.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
            }

            Camera?.Close();
            Camera?.Dispose();
            Camera = null;

            PreviewImage?.Dispose();
            PreviewImage = null;
        }
    }
}