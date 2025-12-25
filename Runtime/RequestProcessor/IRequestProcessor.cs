using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TanitakaTech.UnityProcessManager
{
    public interface IRequestProcessor
    {
        UniTask WaitAsync(CancellationToken cancellationToken);
        UniTask<ProcessResult> ProcessAsync(CancellationToken cancellationToken);
    }

    public enum ProcessResult
    {
        Continue,
        Break
    }

    public static class RequestProcesserExtensions
    {
        public static async UniTask RunAsync(this IReadOnlyList<IRequestProcessor> processors, CancellationToken cancellationToken)
        {
            var length = processors.Count;
            if (length == 0) return;

            var tasks = new UniTask[length];
            ProcessResult processResult;
            do {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                for (int i = 0; i < length; i++)
                {
                    tasks[i] = processors[i].WaitAsync(cts.Token);
                }
                var passedTaskIndex = await UniTask.WhenAny(tasks);
                cts.Cancel();
                cts.Dispose();
                processResult = await processors[passedTaskIndex].ProcessAsync(cancellationToken);
            } while (processResult == ProcessResult.Continue && !cancellationToken.IsCancellationRequested);
        }
    }
}