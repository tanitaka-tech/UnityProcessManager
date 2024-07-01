[![openupm](https://img.shields.io/npm/v/com.tanitaka.unity-process-manager?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.tanitaka.unity-process-manager/)
![license](https://img.shields.io/github/license/tanitaka-tech/UnityProcessManager)

⚠️このパッケージは現在開発中です...

## コンセプト

現状の殆どのクライアントフレームワークには大きな欠陥があります。
それは、Sceneを管理するクラスがScene内のViewを構成するパーツ(ボタンなど)に対して依存してしまう点です。

パーツを変更するたびにScene管理クラスを修正する必要があるという点が課題です。
これのせいでSceneの運用コストが増加したり、属人化したりなどの問題が発生します。

そこでこのパッケージでは上記の課題の解決策として
「パーツに依存するのではなく、Requestに依存すること」を提唱、そして支援します。

ここで言うRequestとは、「Scene遷移をして欲しい」「APIを呼んでほしい」などの
Sceneを構成する各パーツからの要求を示す構造体です。

UnityProcessManagerはRequestを使うことでSceneからパーツへの依存を排除するほか、
Requestを手続き的にハンドリングすることで、 Sceneの流れを分かりやすく、正確に表現するためのパッケージです。

## Usage Example

### Input wait loop (WIP)
```cs
var result = await ConcurrentUniTaskHandler.Create(  
        // Effect
        ProcessTask.Create(  
            waitTask: WaitEffectRequestAsync,  
            onPassedTask: async ct =>  
            {  
                await PlayEffectAsync(ct);
                return true;  
            }),
    
        // Close
        ProcessTask.Create(  
            waitTask: WaitCloseRequestAsync,  
            onPassedTask: async ct =>  
            {  
                await CloseAsync(ct);
                return false;  
            }), 
    
        // NextScene
        ProcessTask.Create(  
            waitTask: WaitMoveNextSceneRequestAsync,  
            onPassedTask: async ct =>  
            {  
                await LoadNextSceneAsync(ct);
                return false;
            }), 
    )    
    .LoopProcessFirstCompletedTaskAsync(  
        checkNeedLoop: result => result,  
        cancellationToken: cancellationToken  
);
```

## メリット
- Scene内で発生し得るRequestをまとめることで、そのSceneが行使する責務を明確にすることができます。
- 手続きが正確に表現されるため、処理の流れが非常に追いやすくなります。
- 手続き処理を

## デメリット
- あまりにも快適すぎて、他のフレームワークが使えなくなる恐れがあります。

## Installation ☘️

### Install via git URL
1. Open the Package Manager
1. Press [＋▼] button and click Add package from git URL...
1. Enter the following:
    - https://github.com/tanitaka-tech/UnityProcessManager.git

### ~~Install via OpenUPM~~ (not yet)
```sh
openupm add com.tanitaka-tech.unity-process-manager
```

## Contribution 🎆
IssueやPR作成など、大歓迎です！
手が空いた際に見させていただきます。

また、このプロジェクトに興味を持っていただけた方は、ぜひスターを付けてください！

## Requirement
- [UniTask](https://github.com/Cysharp/UniTask)
