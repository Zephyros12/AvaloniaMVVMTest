using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaMVVMTest.Services
{
    public interface ICameraService : IDisposable
    {
        IList<string> GetAvailableCameraSerials();
        Task<bool> InitializeAsync(string serialNumber);
        Task GrabAsync(Action<byte[], int, int> onImageReceived);
        void Stop();
    }
}
