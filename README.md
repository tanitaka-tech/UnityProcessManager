[![openupm](https://img.shields.io/npm/v/com.tanitaka-tech.unity-process-manager?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.tanitaka-tech.unity-process-manager/)
![license](https://img.shields.io/github/license/tanitaka-tech/UnityProcessManager)

**Docs** ([English](README.md), [日本語](README_JA.md))

## Mission of this Library

- Decouple the classes that manage the flow of processes from the classes that trigger events.
- Provide a means to clearly and concisely express the flow of processes.

## Recommended Use Cases
- Handling events that lead to processes that are risky to run in parallel, such as scene transitions or API calls.
- Changing the events to wait for according to the flow of processes.

## Usage
1. Use `RequestPusher` to send requests when events are triggered in each class.
2. Use `RequestConsumer` to await the push of requests.
3. Use `ConcurrentProcess` to wait for multiple `RequestConsumer` instances in parallel and describe the processing for each request individually when it arrives.

## Usage Example

### ① Bind RequestHandler (Zenject Example)
```cs
// ----- In some installer

// You need to define a unique type for each request
var effectRequestHandler = new RequestHandler<EffectRequest>();
Container.BindInstance<IRequestPusher<EffectRequest>>(effectRequestHandler);
var closeRequestHandler = new RequestHandler<CloseRequest>();
Container.BindInstance<IRequestPusher<CloseRequest>>(closeRequestHandler);
var nextSceneRequestHandler = new RequestHandler<NextSceneRequest>();
Container.BindInstance<IRequestPusher<NextSceneRequest>>(nextSceneRequestHandler);
var waitRequestClass = new WaitRequestClass(
        effectRequestConsumer: effectRequestHandler,
        closeRequestConsumer: closeRequestHandler,
        nextSceneRequestConsumer: nextSceneRequestHandler
);
```

### ② Push Request (R3 Example)
```cs
// ----- In some object
[SerializeField] private Button _closeButton;
[Inject] private IRequestPusher<CloseRequest> _closeRequestPusher;
private void Start()
{
        _closeButton.OnClickAsObservable()
                .Subscribe(_ => _closeRequestPusher.PushRequest(new CloseRequest()))
                .AddTo(this);
}
```

### ③ Wait Request
```cs
await ConcurrentProcess.Create(  
        // When the waitTask is exited, the onPassedTask await is executed. At that time, the waitTask of other Processes is canceled.
        // This specification ensures that only one Request is handled at a time.
        Process.Create(  
            waitTask: async ct => await EffectRequestConsumer.WaitRequestAndConsumeAsync(ct),  
            onPassedTask: async ct =>  
            {  
                await PlayEffectAsync(ct);
                // Returning Continue in onPassedTask continues the parallel waiting for Requests
                return ProcessContinueType.Continue;
            }),
        
        Process.Create(  
            waitTask: async ct => await CloseRequestConsumer.WaitRequestAndConsumeAsync(ct),
            onPassedTask: async ct =>  
            {  
                await CloseAsync(ct);
                // Returning Break in onPassedTask exits the await of LoopProcessAsync
                return ProcessContinueType.Break;
            }), 
        
        Process.Create(  
            waitTask: async ct => await NextSceneRequestConsumer.WaitRequestAndConsumeAsync(ct),
            onPassedTask: async ct =>  
            {  
                await LoadNextSceneAsync(ct);
                return ProcessContinueType.Break;
            }), 
        )    
        .LoopProcessAsync(cancellationToken: ct);
```

## LiteRequestBroker

LiteRequestBroker is a class that can be used to send requests without using the RequestHandler class.

It is good to use in the following cases:
- The request does not have any parameters
- If the request is not being handled simultaneously

### ① Bind LiteRequestBroker
```cs
// ----- In some installer

var liteRequestBroker = new LiteRequestBroker();
Container.BindInstance<ILiteRequestPusher>(liteRequestBroker);
Container.BindInstance<ILiteRequestConsumer>(liteRequestBroker);
```

### ② Push Request (R3 Example)
```cs
// ----- In some object
[SerializeField] private Button _closeButton;
[Inject] private ILiteRequestPusher _liteRequestPusher;
private void Start()
{
        _closeButton.OnClickAsObservable()
                // I reccommend defining a const string for each request
                .Subscribe(_ => _liteRequestPusher.PushRequest("CloseRequest"))
                .AddTo(this);
}
```

### ③ Wait Request
```cs
await ConcurrentProcess.Create(  
        Process.Create(  
            waitTask: async ct => await LiteRequestConcumer.WaitRequestAndConsumeAsync("CloseRequest", ct),  
            onPassedTask: async ct =>  
            {  
                await CloseAsync(ct);
                return ProcessContinueType.Break;
            }),
          
            // ...
        )    
        .LoopProcessAsync(cancellationToken: ct);
```


## Logging

You can enable logging by defining the following:

`UNITY_PROCESS_MANAGER_LOGGER`

```csharp
await ConcurrentProcess.CreateWithLog(
       Logger, // Some Microsoft.Extensions.Logging implements class.(I recommend using ZLogger)
       MoveToLicensesProcessProvider // Some IProcessProvider implements class.(This is the class you created)
       )
       .LoopProcessAsync(cancellationToken: cancellationToken);
```



## Installation ☘️
### Install via git URL
1. Open the Package Manager
2. Press [＋▼] button and click Add package from git URL...
3. Enter the following:
    - https://github.com/tanitaka-tech/UnityProcessManager.git

### Install via OpenUPM
```sh
openupm add com.tanitaka-tech.unity-process-manager
```

## Contribution 🎆
Issues and PRs are very welcome!
I'll take a look when I have free time.
Also, if you're interested in this project, please give it a star!

## Requirement
- [UniTask](https://github.com/Cysharp/UniTask)