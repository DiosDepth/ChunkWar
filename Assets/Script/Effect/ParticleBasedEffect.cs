using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ParticleBasedEffect : BaseEffect
{
    private ParticleSystem[] systems;

    private float duration = 0;
    private CancellationTokenSource cts;

#if UNITY_EDITOR

    private float customTime = 0;
    private float _perTime;
    private bool playFlag;

    [OnInspectorInit]
    private void OnEnable()
    {
        EditorApplication.update += CustomUpdate;
        _perTime = (float)EditorApplication.timeSinceStartup;
     
    }

    [OnInspectorDispose]
    private void OnDisable()
    {
        EditorApplication.update -= CustomUpdate;
        playFlag = false;
    }

    [Button("‘§¿¿")]
    void PreviewPlay()
    {
        systems = gameObject.GetComponentsInChildren<ParticleSystem>();
        for(int i = 0; i < systems.Length; i++)
        {
            systems[i].Play();
        }
        customTime = 0;
        playFlag = true;
    }

    void CustomUpdate()
    {
        var _time = (float)EditorApplication.timeSinceStartup - _perTime;
        _perTime = (float)EditorApplication.timeSinceStartup;

        if (!playFlag)
            return;

        customTime += _time;
        for(int i =0;i< systems.Length; i++)
        {
            systems[i].Simulate(customTime, true);
        }
        SceneView.RepaintAll();
    }

#endif

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
