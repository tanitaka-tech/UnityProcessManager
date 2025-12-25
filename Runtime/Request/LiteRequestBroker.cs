using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

#if UNITY_PROCESS_MANAGER_LOGGER
using Microsoft.Extensions.Logging;
#endif

namespace TanitakaTech.UnityProcessManager
{
    public class LiteRequestBroker : 
        ILiteRequestPusher,
        ILiteRequestConsumer
    {
#if UNITY_PROCESS_MANAGER_LOGGER
        private readonly ILogger _logger;
#endif
        private readonly HashSet<string> _waitRequests;

        public LiteRequestBroker(
#if UNITY_PROCESS_MANAGER_LOGGER
            ILogger logger,
#endif
            int initialCapacity = 32
            )
        {
#if UNITY_PROCESS_MANAGER_LOGGER
            _logger = logger;
#endif
            _waitRequests = new HashSet<string>(initialCapacity);
        }

        public LiteRequestBroker(int initialCapacity)
        {
            _waitRequests = new HashSet<string>(initialCapacity);
        }
        
        void ILiteRequestPusher.PushRequest(string request)
        {
            if (_waitRequests.Contains(request))
            {
                _waitRequests.Remove(request);
            }
#if UNITY_PROCESS_MANAGER_LOGGER
            else
            {
                _logger.LogError($"Any consumer is not waiting for a {request}");
            }
#endif
        }

        async UniTask ILiteRequestConsumer.WaitRequestAndConsumeAsync(string waitRequest, CancellationToken cancellationToken)
        {
#if UNITY_PROCESS_MANAGER_LOGGER
            _logger.LogInformation($"Start waiting for a {waitRequest}");
#endif
            _waitRequests.Add(waitRequest);
            cancellationToken.Register(() => _waitRequests.Remove(waitRequest));
            await UniTask.WaitUntil(() => !_waitRequests.Contains(waitRequest), cancellationToken: cancellationToken);
#if UNITY_PROCESS_MANAGER_LOGGER
            _logger.LogInformation($"{waitRequest} was consumed");
#endif
        }
    }
}