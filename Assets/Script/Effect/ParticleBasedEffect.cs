using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ParticleBasedEffect : BaseEffect
{
    private ParticleSystem[] systems;

    private float duration = 0;
    private CancellationTokenSource cts;

    public override void Awake()
    {
        base.Awake();
        systems = gameObject.GetComponentsInChildren<ParticleSystem>();
    }

    public override async void OnCreate()
    {
        base.OnCreate();

        if (systems != null && systems.Length > 0)
        {
            systems.OrderByDescending(x => x.main.duration);
            duration = systems[0].main.duration;
        }
        cts = new CancellationTokenSource();
        await UniTask.Delay((int)(duration * 1000), cancellationToken: cts.Token);
        PoolableDestroy();
    }

    public override void PoolableReset()
    {
        base.PoolableReset();
        cts.Cancel();
        duration = 0;
    }
}
