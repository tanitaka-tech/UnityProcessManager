using System.Threading;
using Cysharp.Threading.Tasks;

namespace TanitakaTech.UnityProcessManager
{
    public interface IRequestConsumer<TRequest>
    {
        public UniTask<TRequest> WaitRequestAndConsumeAsync(CancellationToken cancellationToken);
    }
}