using System.Threading;
using Cysharp.Threading.Tasks;

namespace TanitakaTech.UnityProcessManager
{
    public class RequestHandler<TRequest> :
        IRequestPusher<TRequest>,
        IRequestConsumer<TRequest> where TRequest : struct
    {
        private TRequest _requestValue;
        private bool _isWaitingRequest = false;

        public void PushRequest(TRequest requestValue)
        {
            if (!_isWaitingRequest) return;

            _isWaitingRequest = false;
            _requestValue = requestValue;
        }

        public async UniTask<TRequest> WaitRequestAndConsumeAsync(CancellationToken cancellationToken)
        {
            _isWaitingRequest = true;
            cancellationToken.Register(() => _isWaitingRequest = false);
            await UniTask.WaitUntil(() => !_isWaitingRequest, cancellationToken: cancellationToken);
            return _requestValue;
        }
    }
}