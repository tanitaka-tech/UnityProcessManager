using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TanitakaTech.UnityProcessManager
{
    public readonly struct ConcurrentProcess
    {
        private Memory<Process> Processes { get; }

        private ConcurrentProcess(params Process[] processTasks)
        {
            Processes = new Memory<Process>(processTasks);
        }
        
        public static ConcurrentProcess Create(params Process[] processTasks)
        {
            return new ConcurrentProcess(processTasks);
        }

        public static ConcurrentProcess Create(params ConcurrentProcess[] concurrentProcesses)
        {
            return new ConcurrentProcess(
                concurrentProcesses.SelectMany(concurrentProcess => concurrentProcess.Processes.ToArray()).ToArray()
            );
        }

        public ConcurrentProcess With(params ConcurrentProcess[] concurrentProcesses)
        {
            return new ConcurrentProcess(
                concurrentProcesses
                    .SelectMany(concurrentProcess => concurrentProcess.Processes.ToArray())
                    .Concat(Processes.ToArray())
                    .ToArray()
            );
        }
        
        public async UniTask LoopProcessAsync(CancellationToken cancellationToken = default)
        {
            ProcessContinueType continueType = default;
            do
            {
                continueType = await InternalProcessAsync(cancellationToken);
            } while (continueType == ProcessContinueType.Continue);
        }

        private async UniTask<ProcessContinueType> InternalProcessAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            UniTask[] tasks = new UniTask[Processes.Length];
            for (int i = 0; i < Processes.Length; i++)
            {
                tasks[i] = Processes.Span[i].WaitTask(cancellationTokenSource.Token);
            }

            var passedTaskIndex = await UniTask.WhenAny(tasks);
            cancellationTokenSource.Cancel();
            return await Processes.Span[passedTaskIndex].OnPassedTask(cancellationToken);
        }
    }
}