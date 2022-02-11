using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

#nullable enable

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcClient<T> where T : class
    {
        private readonly string _pipeName;

        public RpcClient(string pipeName)
        {
            _pipeName = pipeName;
        }

        private NamedPipeClientStream? _stream;
        private JsonRpc? _rpc;

        public T? Instance { private set; get; }

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        public async Task Connect()
        {
            _stream = GetStream();
            await _stream.ConnectAsync();

            _rpc = new JsonRpc(_stream);
            _rpc.Disconnected += (_, _) =>
            {
                _stream.Dispose();
                OnDisconnected();
                _rpc.Dispose();
            };

            Instance = _rpc.Attach<T>();
            _rpc.StartListening();

            OnConnected();
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private NamedPipeClientStream GetStream()
        {
            return new NamedPipeClientStream(
                ".",
                _pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly
            );
        }
    }
}
