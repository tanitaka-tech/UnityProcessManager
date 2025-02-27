#if UNITY_PROCESS_MANAGER_LOGGER
using System.Threading;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TanitakaTech.UnityProcessManager
{
    public class RequestHandlerWithLog<TRequest> :
        IRequestPusher<TRequest>,
        IRequestConsumer<TRequest> where TRequest : struct
    {
        private ILogger Logger { get; }
        private RequestHandler<TRequest> RequestHandler { get; }

        public RequestHandlerWithLog(ILogger logger)
        {
            Logger = logger;
            RequestHandler = new RequestHandler<TRequest>();
        }

        void IRequestPusher<TRequest>.PushRequest(TRequest requestValue)
        {
            Logger.LogInformation($"PushRequest: {requestValue}");
            RequestHandler.PushRequest(requestValue);
        }

        async UniTask<TRequest> IRequestConsumer<TRequest>.WaitRequestAndConsumeAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Start WaitRequestAndConsumeAsync");
            var request = await  RequestHandler.WaitRequestAndConsumeAsync(cancellationToken);
            Logger.LogInformation($"End WaitRequestAndConsumeAsync request:{request}");
            return request;
        }
    }
}
#endif