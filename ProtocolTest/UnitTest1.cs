using System.Buffers;
using System.Reflection.PortableExecutable;
using TCPInterceptors;

namespace ProtocolTest
{
    [TestClass]
    public class StreamerTests
    {
        byte[] buffer =
        [
            0x10, 0x20, 0x40, 0x80,
            0x10, 0x20, 0x40, 0x80,
            0x10, 0x20, 0x40, 0x80,
            0x10, 0x20, 0x40, 0x80,
            0x10, 0x20, 0x40, 0x99,
        ];

        [TestMethod]
        public async Task duplexpipe_input()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            DuplexPipe dataPipe = new();

            await dataPipe.InputPipeWrite.WriteAsync(buffer, cts.Token);

            Assert.IsTrue(dataPipe.Input.TryRead(out var result), "try read failed");

            CollectionAssert.AreEqual(buffer, result.Buffer.ToArray(), "data buffed not matching expected");
        }

        [TestMethod]
        public async Task duplexpipe_output()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            DuplexPipe dataPipe = new();

            await Task.Delay(100);
            dataPipe.Output.Write(buffer);
            await dataPipe.Output.FlushAsync(cts.Token);
            await dataPipe.PushOutputAsync(buffer, cts.Token);
            await Task.Delay(100);

            byte[] outbuffer = new byte[512];
            var sz = await dataPipe.OutputPipeRead.ReadAsync(outbuffer, cts.Token);
            var len = buffer.Length;
            Assert.AreEqual(2*len, sz, "unexpected read size");

            CollectionAssert.AreEqual(buffer, outbuffer[..len], "data buffed not matching expected (0)");
            CollectionAssert.AreEqual(buffer, outbuffer[len..sz], "data buffed not matching expected (1)");
        }


        [TestMethod]
        public async Task duplexpipe_buffering()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            // backing streams
            MemoryStream streamIn = new MemoryStream(buffer);
            MemoryStream streamOut = new MemoryStream();
            DuplexPipe dataPipe = new();

            // processor
            StreamForwarding forwarding = new()
            {
                InputStream = streamIn,
                OutputStream = streamOut,
                ByteChannel = dataPipe
            };

            var test = Task.Run(async () =>
            {
                // forward streamIn to pipeIn, write any pipeOut to streamOut from duplexpipe 
                await forwarding.BeginForwardingAsync(cts.Token);
            });

            await Task.Delay(1000);

            Assert.IsTrue(dataPipe.Input.TryRead(out var result), "try read failed");

            CollectionAssert.AreEqual(buffer, result.Buffer.ToArray(), "data buffed not matching expected");

            cts.Cancel();
            await test;

        }

        [TestMethod]
        public async Task duplexpipe_flushing()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            // backing streams
            MemoryStream streamIn = new MemoryStream(buffer);
            MemoryStream streamOut = new MemoryStream();
            DuplexPipe dataPipe = new();

            // processor
            StreamForwarding forwarding = new()
            {
                InputStream = streamIn,
                OutputStream = streamOut,
                ByteChannel = dataPipe
            };

            var test2 = Task.Run(async () =>
            {
                // forward streamIn to pipeIn, write any pipeOut to streamOut from duplexpipe 
                await forwarding.BeginForwardingAsync(cts.Token);
            });

            await Task.Delay(100);
            await dataPipe.PushOutputAsync(buffer, cts.Token);
            await Task.Delay(100);

            byte[] outBuffer = new byte[512];
            var sz = await streamOut.ReadAsync(outBuffer, cts.Token);
            CollectionAssert.AreEqual(buffer, outBuffer[..sz], $"data buffed not matching expected (expected {buffer.Length} but got {sz})");

            cts.Cancel();
            await test2;

        }
    }
}