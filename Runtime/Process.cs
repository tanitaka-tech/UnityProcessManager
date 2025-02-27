using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TanitakaTech.UnityProcessManager
{
    public readonly struct Process
    {
        public Func<CancellationToken, UniTask> WaitTask { get; }
        public Func<CancellationToken, UniTask<ProcessContinueType>> OnPassedTask { get; }

        private Process(Func<CancellationToken, UniTask> waitTask, Func<CancellationToken, UniTask<ProcessContinueType>> onPassedTask)
        {
            WaitTask = waitTask;
            OnPassedTask = onPassedTask;
        } 
        
        public static Process Create(Func<CancellationToken, UniTask> waitTask, Func<CancellationToken, UniTask<ProcessContinueType>> onPassedTask)
        {
            return new Process(waitTask, onPassedTask);
        }
        
        public static Process CreateWithWaitResult<TWaitResult>(Func<CancellationToken, UniTask<TWaitResult>> waitTask, Func<TWaitResult, CancellationToken, UniTask<ProcessContinueType>> onPassedTask)
        {
            TWaitResult waitResult = default;
            return new Process(
                waitTask: async (ct) => waitResult = await waitTask(ct), 
                onPassedTask: (ct) => onPassedTask(waitResult, ct)
            );
        }
        
        public UniTask ToUniTask(CancellationToken cancellationToken)
        {
            var process = this;
            return UniTask.Create(async () =>
            {
                await process.WaitTask(cancellationToken);
                await process.OnPassedTask(cancellationToken);
            });
        }
    }

    public static class ProcessExtensions
    {
        public static Process Wrap(this Process process, Process wrapProcess)
        {
            return Process.Create(
                waitTask: async ct =>
                {
                    await wrapProcess.WaitTask(ct);
                    await process.WaitTask(ct);
                },
                onPassedTask: async ct =>
                {
                    await wrapProcess.OnPassedTask(ct);
                    return await process.OnPassedTask(ct);
                }
            );
        }
    }
}