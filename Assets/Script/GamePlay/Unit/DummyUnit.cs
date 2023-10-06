using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DummyUnit : Unit
{

    public override void Start()
    {
        base.Start();

        var unitconfig = DataManager.Instance.GetUnitConfig(0);
        _baseUnitConfig = unitconfig;
        baseAttribute.InitProeprty(this, _baseUnitConfig, false);
        Initialization(null, _baseUnitConfig);
    }

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        HpComponent = new GeneralHPComponet(baseAttribute.HPMax, baseAttribute.HPMax);
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);

        if (_owner is AIShip)
        {
            AIManager.Instance.AddSingleUnit(this);

            IsRestoreable = false;
        }
        if (_owner is PlayerShip)
        {
            AIManager.Instance.AddTargetUnit(this);
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
            IsRestoreable = true;

            SetUnitProcess(true);
            unitSprite.color = Color.white;
        }
        GameManager.Instance.RegisterPauseable(this);
    }


    public override bool TakeDamage(DamageResultInfo info)
    {
        if (HpComponent == null)
            return false;

        int Damage = info.Damage;
        bool critical = info.IsCritical;

        ///Check Damage
        if (Damage == 0)
            return false;

        if (info.IsPlayerAttack)
        {
      
            var enemyClass = EnemyClassType.Normal;
            if (enemyClass == EnemyClassType.Elite || enemyClass == EnemyClassType.Boss)
            {
                var damageAddition = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EliteBossDamage);
                var newDamage = info.Damage * (1 + damageAddition / 100f);
                newDamage = Mathf.Clamp(newDamage, 0, float.MaxValue);
                info.Damage = Mathf.RoundToInt(newDamage);
            }
        }

        LevelManager.Instance.UnitHit(info);
        ///ֻ�е��˲���ʾ�˺�����
        ///������Ҫ��ʾ��Ӧ��Ư������
        UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Top, this.gameObject, (panel) =>
        {
            panel.transform.position = CameraManager.Instance.mainCamera.WorldToScreenPoint(transform.position);
            panel.Initialization();
            panel.SetText(Mathf.Abs(Damage), critical);
            panel.SetSize(48);
            panel.SetColor(Color.red);
            panel.Show();

        });
        
      

        if (info.IsHit)
        {
            bool isDie = HpComponent.ChangeHP(-info.Damage);
            if (isDie)
            {
                UnitDeathInfo deathInfo = new UnitDeathInfo
                {
                    isCriticalKill = info.IsCritical
                };
                Death(deathInfo);
            }
            return isDie;
        }
        return false;
    }
    public override void Death(UnitDeathInfo info)
    {
        GameManager.Instance.UnRegisterPauseable(this);
        state = DamagableState.Destroyed;

        if (IsCoreUnit)
        {
            _owner.CheckDeath(this, info);
            //destroy owner
        }
        else
        {
            if (_owner is AIShip)
            {
                AIManager.Instance.RemoveSingleUnit(this);
            }

            if (_owner is PlayerShip)
            {
                AIManager.Instance.RemoveTargetUnit(this);
                RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
                if (_baseUnitConfig.unitType == UnitType.Weapons || _baseUnitConfig.unitType == UnitType.Buildings)
                {
                    (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveActiveUnit(this);
                }
            }
        }
        this.gameObject.SetActive(false);
        SetUnitProcess(false);

        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) =>
        {

            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
            unitSprite.color = Color.black;
        });
    }
}
