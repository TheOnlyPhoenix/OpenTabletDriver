using System;

namespace OpenTabletDriver
{
    public interface ITimer : IDisposable
    {
        void Start();
        void Stop();
        bool Enabled { get; }
        float Interval { get; set; }
        event Action Elapsed;
    }
}
