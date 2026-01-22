using System.Threading.Channels;

namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// Interface for the evaluation queue that accepts jobs for background processing.
    /// </summary>
    public interface IEvaluationQueue
    {
        /// <summary>
        /// Enqueues an attempt for background evaluation.
        /// </summary>
        ValueTask EnqueueAsync(EvaluationJob job, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dequeues the next evaluation job. Blocks until a job is available.
        /// </summary>
        ValueTask<EvaluationJob> DequeueAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Channel-based evaluation queue for background processing.
    /// </summary>
    public class EvaluationQueue : IEvaluationQueue
    {
        private readonly Channel<EvaluationJob> _channel;

        public EvaluationQueue()
        {
            // Unbounded channel - all submitted attempts will be queued
            _channel = Channel.CreateUnbounded<EvaluationJob>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(EvaluationJob job, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(job, cancellationToken);
        }

        public async ValueTask<EvaluationJob> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
