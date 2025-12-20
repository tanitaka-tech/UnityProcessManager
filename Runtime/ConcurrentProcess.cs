using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

#if UNITY_PROCESS_MANAGER_LOGGER
using Microsoft.Extensions.Logging;
#endif

namespace TanitakaTech.UnityProcessManager
{
    public readonly struct ConcurrentProcess
    {
        private readonly Memory<Process> _processes;

        private ConcurrentProcess(params Process[] processTasks)
        {
            _processes = new Memory<Process>(processTasks);
        }
        
        public static ConcurrentProcess Create(params Process[] processTasks)
        {
            return new ConcurrentProcess(processTasks);
        }

        public static ConcurrentProcess Create(params IProcessProvider[] processProviders)
        {
            return new ConcurrentProcess(processProviders.Select(processProvider => processProvider.Provide()).ToArray());
        }

#if UNITY_PROCESS_MANAGER_LOGGER
        public static ConcurrentProcess CreateWithLog(ILogger logger, params IProcessProvider[] processProviders)
        {
            int count = processProviders.Length;
            int waitNum = 0;
            return new ConcurrentProcess(
                processProviders.Select(processProvider => processProvider.Provide()
                    .Wrap(Process.Create(waitTask: ct =>
                        {
                            waitNum++;
                            if (waitNum == count)
                            {
                                logger.LogInformation("WaitTask count: {0}\n{1}", count, processProviders.Select(pp => pp.GetType().Name));
                                waitNum = 0;
                            }
                            return UniTask.CompletedTask;
                        },
                        onPassedTask: ct =>
                        {
                            logger.LogInformation("OnPassedTask processProvider: {0}", processProvider.GetType().Name);
                            return UniTask.FromResult(ProcessContinueType.Continue);
                        })))
                    .ToArray());
        }
#endif

        public static ConcurrentProcess Create(params ConcurrentProcess[] concurrentProcesses)
        {
            return new ConcurrentProcess(
                concurrentProcesses.SelectMany(concurrentProcess => concurrentProcess._processes.ToArray()).ToArray()
            );
        }

        public ConcurrentProcess With(params ConcurrentProcess[] concurrentProcesses)
        {
            return new ConcurrentProcess(
                concurrentProcesses
                    .SelectMany(concurrentProcess => concurrentProcess._processes.ToArray())
                    .Concat(_processes.ToArray())
                    .ToArray()
            );
        }
        
        public async UniTask LoopProcessAsync(CancellationToken cancellationToken = default)
        {
            var length = _processes.Length;
            UniTask[] tasks = new UniTask[length];
            ProcessContinueType continueType = default;
            do {
                CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                for (int i = 0; i < length; i++)
                {
                    tasks[i] = _processes.Span[i].WaitTask(cancellationTokenSource.Token);
                }
                var passedTaskIndex = await UniTask.WhenAny(tasks);
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                continueType = await _processes.Span[passedTaskIndex].OnPassedTask(cancellationToken);
            } while (continueType == ProcessContinueType.Continue && !cancellationToken.IsCancellationRequested);
        }
    }
}