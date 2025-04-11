[![openupm](https://img.shields.io/npm/v/com.tanitaka-tech.unity-process-manager?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.tanitaka-tech.unity-process-manager/)
![license](https://img.shields.io/github/license/tanitaka-tech/UnityProcessManager)

**Docs** ([English](README.md), [日本語](README_JA.md))

## このライブラリのミッション

- 処理の流れを管理するクラスと、イベントを発火するクラスを疎結合にする
- 処理の流れを分かりやすく簡潔に表現する手段の提供

## おすすめのユースケース
- Scene遷移やAPI呼び出しなど、**並列で行われると怖い処理**に繋がるイベントのハンドリング
- 待機するイベントを処理の流れに応じて変えたい時

## 使い方
1. `RequestPusher`を使用して、各クラスでイベント発火時にRequestを投げる
2. `RequestConsumer`を使用して、RequestのPushをawait
3. `ConcurrentProcess`を使用して、複数の`RequestConsumer`の並列待機し、各Requestが来た時の処理を個別に記述

### ① Bind RequestHandler (Zenject Example)
```cs
// ----- In some installer

// Request毎に独自の型を定義する必要があります
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
        // waitTaskを抜けた時にonPassedTaskのawaitが行われます。その際、他ProcessのwaitTaskはキャンセルされます
        // この仕様によって、同時にハンドリングするRequestが常に一つであることを保証します
        Process.Create(  
            waitTask: async ct => await EffectRequestConsumer.WaitRequestAndConsumeAsync(ct),  
            onPassedTask: async ct =>  
            {  
                await PlayEffectAsync(ct);
                // onPassedTaskでContinueを返すと、Requestの並列待機を継続する
                return ProcessContinueType.Continue;
            }),
        
        Process.Create(  
            waitTask: async ct => await CloseRequestConsumer.WaitRequestAndConsumeAsync(ct),
            onPassedTask: async ct =>  
            {  
                await CloseAsync(ct);
                
                // onPassedTaskでBreakを返すと、LoopProcessAsyncのawaitを抜ける
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

LiteRequestBrokerは、RequestHandlerクラスを使用せずにリクエストを送信するために使用できるクラスです。

下記のような場合に使用することをおすすめします。
- リクエストがパラメータを持たない時
- 同じリクエストが同時に待機されない時

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

下記のdefine symbolを定義することでログ関連の実装を有効にできます。

`UNITY_PROCESS_MANAGER_LOGGER`

```csharp
await ConcurrentProcess.CreateWithLog(
       Logger, // Microsoft.Extensions.Loggingを継承したクラス(ログ出しにはZLoggerがおすすめです)
       MoveToLicensesProcessProvider // IProcessProviderを継承した自作クラス
       )
       .LoopProcessAsync(cancellationToken: cancellationToken);
```

## Installation ☘️

### Install via git URL
1. Open the Package Manager
1. Press [＋▼] button and click Add package from git URL...
1. Enter the following:
    - https://github.com/tanitaka-tech/UnityProcessManager.git

### Install via OpenUPM
```sh
openupm add com.tanitaka-tech.unity-process-manager
```

## Contribution 🎆
IssueやPR作成など、大歓迎です！

また、このプロジェクトに興味を持っていただけた方は、ぜひスターを付けてください！

## Requirement
- [UniTask](https://github.com/Cysharp/UniTask)
