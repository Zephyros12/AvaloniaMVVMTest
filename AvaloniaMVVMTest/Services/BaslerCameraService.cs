using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basler.Pylon;

namespace AvaloniaMVVMTest.Services
{
    public sealed class BaslerCameraService : ICameraService
    {
        private Camera? Camera { get; set; }
        private bool IsGrabbing { get; set; }
        private bool IsDisposed { get; set; }

        public IList<string> GetAvailableCameraSerials()
        {
            var cameras = CameraFinder.Enumerate();
            return cameras.Select(info => info[CameraInfoKey.SerialNumber]).ToList();
        }

        public Task<bool> InitializeAsync(string serialNumber)
        {
            try
            {
                var info = CameraFinder.Enumerate()
                    .FirstOrDefault(c => c[CameraInfoKey.SerialNumber] == serialNumber);

                if (info == null)
                {
                    Console.WriteLine($"카메라를 찾을 수 없습니다: {serialNumber}");
                    return Task.FromResult(false);
                }

                Camera = new Camera(info);
                Camera.Open();
                return Task.FromResult(Camera.IsOpen);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InitializeAsync 예외: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public async Task GrabAsync(Action<byte[], int, int> onImageReceived)
        {
            if (Camera == null || !Camera.IsOpen || Camera.StreamGrabber.IsGrabbing)
            {
                Console.WriteLine("GrabAsync 사전 조건 실패");
                return;
            }

            IsGrabbing = true;
            var localCamera = Camera;

            await Task.Run(() =>
            {
                try
                {
                    localCamera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);

                    while (IsGrabbing)
                    {
                        using IGrabResult result = localCamera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);

                        if (result.GrabSucceeded && result.PixelData is byte[] buffer)
                        {
                            onImageReceived(buffer, result.Width, result.Height);
                        }
                    }

                    if (localCamera.StreamGrabber.IsGrabbing)
                    {
                        localCamera.StreamGrabber.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GrabAsync 예외 발생: {ex.GetType().Name} - {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            });
        }

        public void Stop()
        {
            IsGrabbing = false;

            if (Camera != null && Camera.IsOpen)
            {
                Camera.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Stop();
                Camera?.Dispose();
                Camera = null;
            }

            IsDisposed = true;
        }

        ~BaslerCameraService()
        {
            Dispose(false);
        }
    }
}
