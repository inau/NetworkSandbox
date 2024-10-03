using System.IO.Pipelines;

namespace TCPInterceptors
{
    public class DuplexPipe : IDuplexPipe
    {
        protected Pipe InputPipe { get; init; } = new();
        protected Pipe OutputPipe { get; init; } = new();

        /// <summary>
        /// Read end of the buffered input from the wrapped input stream
        /// </summary>
        public PipeReader Input => InputPipe.Reader;

        /// <summary>
        /// Write end of the buffered input from the wrapped input stream
        /// </summary>
        public PipeWriter Output => OutputPipe.Writer;

        public Stream InputPipeWrite => InputPipe.Writer.AsStream();
        public Stream OutputPipeRead => OutputPipe.Reader.AsStream();

        public async Task PushOutputAsync(ReadOnlyMemory<byte> bytes, CancellationToken token)
        {
            var result = await Output.WriteAsync(bytes, token);
            if(!result.IsCanceled && !result.IsCompleted )
            {
                await OutputPipe.Writer.FlushAsync(token);            }
        }
    }
}
