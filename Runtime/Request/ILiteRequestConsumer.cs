using System.Threading;
using Cysharp.Threading.Tasks;

namespace TanitakaTech.UnityProcessManager
{
    public interface ILiteRequestConsumer
    {
        UniTask WaitRequestAndConsumeAsync(string waitRequest, CancellationToken cancellationToken);
    }
}