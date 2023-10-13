using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoShield : MonoBehaviour, IDamageble, IPoolable
{
    private float Radius;

    private Transform _shieldRender;
    private CircleCollider2D _collider;
    private GeneralShieldHPComponet shieldCmpt;
    private BaseShip ownerShip;

    public void Awake()
    {
        _shieldRender = transform.Find("Shield");
        _collider = transform.SafeGetComponent<CircleCollider2D>();
    }

    public void InitShield(GeneralShieldHPComponet cmpt, BaseShip ownerShip)
    {
        shieldCmpt = cmpt;
        this.ownerShip = ownerShip;
        UpdateShieldRatio();
        SetShieldActive(true);
    }

    public void SetShieldActive(bool active)
    {
        _shieldRender.transform.SafeSetActive(active);
        _collider.enabled = active;
    }

    public void UpdateShieldRatio()
    {
        Radius = shieldCmpt.ShieldRatio;
        _shieldRender.localScale = new Vector3(Radius, Radius, 1);
        _collider.radius = Radius;
    }

    public bool TakeDamage(DamageResultInfo info)
    {
        if (ownerShip is PlayerShip)
        {
            ///CalculatePlayerDamage
            GameHelper.ResolvePlayerShieldDamage(info);
        }

        var damage = Mathf.RoundToInt(info.Damage * (1 + info.ShieldDamagePercent / 100f));

        return shieldCmpt.TakeDamage(-damage);
    }

    public void Death(UnitDeathInfo info)
    {

    }

    public void PoolableReset()
    {
        SetShieldActive(false);
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
