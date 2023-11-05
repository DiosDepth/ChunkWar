using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    private List<BaseEffect> _allEffects = new List<BaseEffect>();

    public GameObject EffectRoot
    {
        get
        {
            if(_effRoot == null)
            {
                _effRoot = new GameObject("EffectRoot");
                GameObject.DontDestroyOnLoad(_effRoot);
            }
            return _effRoot;
        }
    }

    private GameObject _effRoot;

    public override void Initialization()
    {
        base.Initialization();
    }

    public void OnUpdate()
    {
        for (int i = 0; i < _allEffects.Count; i++) 
        {
            var eff = _allEffects[i];
            if(eff != null)
            {
                eff.OnUpdate();
            }
        }
    }

    /// <summary>
    /// 创建特效
    /// </summary>
    /// <param name="effPath"></param>
    /// <param name="position"></param>
    public void CreateEffect(string effPath, Vector2 position, Quaternion rotation)
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + effPath, true, (obj) =>
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            var effectBase = obj.transform.SafeGetComponent<BaseEffect>();
            effectBase.OnCreate();
        }, EffectRoot.transform);
    }

    public void CreateEffect(GeneralEffectConfig config, Vector2 position, float OverrideScale = 1)
    {
        if (config.RandomPosition)
        {
            var offsetX = UnityEngine.Random.Range(-config.PosRandomX, config.PosRandomX);
            var offsetY = UnityEngine.Random.Range(-config.PosRandomY, config.PosRandomY);
            position = new Vector2(position.x + offsetX, position.y + offsetY);
        }

        float scale = 1;
        if (config.RandomScale)
        {
            scale = UnityEngine.Random.Range(config.ScaleMin, config.ScaleMax);
        }

        CreateEffect(config.EffectName, position, scale * OverrideScale);
    }

    public void CreateEffect(string effPath, Vector2 position, float scale = 1)
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + effPath, true, (obj) =>
        {
            obj.transform.position = position;
            var effectBase = obj.transform.SafeGetComponent<BaseEffect>();
            effectBase.MultipleScale(scale);
            effectBase.OnCreate();
        }, EffectRoot.transform);
    }

    public void CreateEffectAndFollow(string effPath, Transform followTarget, float scale = 1)
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + effPath, true, (obj) =>
        {
            var effectBase = obj.transform.SafeGetComponent<BaseEffect>();
            effectBase.OnCreate();
            effectBase.SetFollowTarget(followTarget);
        }, EffectRoot.transform);
    }

    public void CreateEffectAndFollow(GeneralEffectConfig config, Transform followTarget)
    {
        float scale = 1;
        if (config.RandomScale)
        {
            scale = UnityEngine.Random.Range(config.ScaleMin, config.ScaleMax);
        }
        CreateEffectAndFollow(config.EffectName, followTarget, scale);
    }

    public void RemoveEffect(BaseEffect eff)
    {
        _allEffects.Remove(eff);
    }

    public void RegisterEffect(BaseEffect eff)
    {
        _allEffects.Add(eff);
    }
}
