using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPInterceptors
{
    internal class StreamForwarding
    {
        public DuplexPipe ByteChannel { protected get; init; } = new DuplexPipe();
        public Stream InputStream { protected get; init; }
        public Stream OutputStream { protected get; init; }

        public async Task BeginForwardingAsync(CancellationToken token)
        {
            var _writer = StartFlushingOuput(token);
            var _reader = StartBufferingInput(token);

            await Task.WhenAll(_reader,_writer);
        }

        async Task StartBufferingInput(CancellationToken token)
        {
            try 
            {
                Trace.WriteLine($"[Buffering] starting");

                while (!token.IsCancellationRequested)
                {
                    await InputStream.CopyToAsync(ByteChannel.InputPipeWrite, token);
                }
                
                await ByteChannel.Input.CompleteAsync();
            }
            catch (Exception ex) 
            {
                Trace.WriteLine($"[Buffering] threw [{ex.Message}]");
            }
            finally
            {
                Trace.WriteLine("[Buffering] stopped");
            }
        }

        async Task StartFlushingOuput(CancellationToken token)
        {
            try
            {
                Trace.WriteLine($"[Flushing] starting");

                while (!token.IsCancellationRequested)
                {
                    await ByteChannel.OutputPipeRead.CopyToAsync(OutputStream, token);
                }

                await ByteChannel.Output.CompleteAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Flushing] threw [{ex.Message}]");

            }
            finally
            {
                Trace.WriteLine("[Flushing] stopped");
            }
        }
    }
}
