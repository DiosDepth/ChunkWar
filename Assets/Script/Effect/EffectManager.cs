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
    /// ������Ч
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

    public void CreateEffect(string effPath, Vector2 position)
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + effPath, true, (obj) =>
        {
            obj.transform.position = position;
            var effectBase = obj.transform.SafeGetComponent<BaseEffect>();
            effectBase.OnCreate();
        }, EffectRoot.transform);
    }

    public void CreateEffectAndFollow(string effPath, Transform followTarget)
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + effPath, true, (obj) =>
        {
            var effectBase = obj.transform.SafeGetComponent<BaseEffect>();
            effectBase.OnCreate();
            effectBase.SetFollowTarget(followTarget);
        }, EffectRoot.transform);
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