using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SyncDataSample.Models;

namespace SyncDataSample.Engine
{
    public class CacheManager : IDisposable
    {
        readonly HttpListener _httpListener = null;
        public delegate void ClientEventHandler(ClientEventArgs args);
        public event ClientEventHandler OnReceivedClientMessage;

        public CacheManager(HttpListener httpListener)
        {
            _httpListener = httpListener;
            _httpListener.Prefixes.Add("http://localhost:8888/");
            _httpListener.Start();
        }

        private void OnClientMessageEvent(ClientEventArgs args)
        {
            if (OnReceivedClientMessage != null)
            {
                OnReceivedClientMessage(args);
            }
        }

        public async Task ClientListeningAsync(CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested && _httpListener.IsListening)
                {
                    HttpListenerContext ctx = await _httpListener.GetContextAsync();

                    ThreadPool.QueueUserWorkItem((o) => {

                        string methodName = ctx.Request.Url.LocalPath;

                        if (!String.IsNullOrWhiteSpace(methodName) && methodName.ToUpper().Trim() != "/FAVICON.ICO")
                        {
                            OnClientMessageEvent(new ClientEventArgs() { Body = methodName });
                        }

                        ctx.Response.OutputStream.Close();
                    });
                }
            });
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpListener?.Stop();
                _httpListener?.Close();
            }
        }

        ~CacheManager()
            => Dispose(false);

    }
}
