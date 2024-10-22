[![openupm](https://img.shields.io/npm/v/com.tanitaka-tech.unity-process-manager?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.tanitaka-tech.unity-process-manager/)
![license](https://img.shields.io/github/license/tanitaka-tech/UnityProcessManager)

**Docs** ([English](README.md), [日本語](README_JA.md))

## Concept
Most current client frameworks have a major flaw.
The issue is that the class managing the Scene becomes dependent on the components (such as buttons) that make up the View within the Scene.
The challenge is that every time a component is changed, the Scene management class needs to be modified.
This leads to problems such as increased operational costs for Scenes and personalization.
As a solution to the above issues, this package proposes and supports
"Depending on Requests rather than components."
A Request here refers to a struct representing demands from each component that makes up the Scene, such as "Please transition to a Scene" or "Please call an API."
UnityProcessManager not only eliminates the dependency of Scenes on components by using Requests,
but it's also a package for clearly and accurately expressing the flow of processes by handling Requests procedurally.

<img width="952" alt="Screenshot 2024-07-02 at 6 46 58" src="https://github.com/tanitaka-tech/UnityProcessManager/assets/78785830/4960cbb2-71e3-4662-9d35-ea1f51ba302b">

## Usage Example
### Bind RequestHandler (Zenject Example)
```cs
// ----- In some installer
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

### Wait Request
```cs
await ConcurrentProcess.Create(  
        // Effect
        Process.Create(  
            waitTask: async ct => await EffectRequestConsumer.WaitRequestAndConsumeAsync(ct),  
            onPassedTask: async ct =>  
            {  
                await PlayEffectAsync(ct);
                return ProcessContinueType.Continue;
            }),
        
        // Close
        Process.Create(  
            waitTask: async ct => await CloseRequestConsumer.WaitRequestAndConsumeAsync(ct),
            onPassedTask: async ct =>  
            {  
                await CloseAsync(ct);
                return ProcessContinueType.Break;
            }), 
        
        // NextScene
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

### Push Request (R3 Example)
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

## Benefits
- By consolidating the Requests that can occur within a Scene, you can clearly define the responsibilities that the Scene exercises.
- As the procedures are accurately expressed, the flow of processing becomes very easy to follow.

## Drawbacks
- It may be so comfortable that you might find it difficult to use other frameworks.

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