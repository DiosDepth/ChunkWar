using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 带有技能更新的AIship
 */
public class AISkillShip : AIShip, IPropertyModify
{
    public uint UID
    {
        get;
        set;
    }

    public PropertyModifyCategory Category
    {
        get { return PropertyModifyCategory.AIShipSkill; }
    }

    public string GetName
    {
        get
        {
            return LocalizationManager.Instance.GetTextValue(AIShipCfg.GeneralConfig.Name);
        }
    }

    public Transform[] ExtraUnitGroup = new Transform[0];

    private List<ModifyTriggerData> _skillDatas = new List<ModifyTriggerData>();

    public override void Initialization()
    {
        base.Initialization();
        InitAIShipSkill();
        InitExtraUnitGroups();
    }

    public Transform GetRandomAttachPoint()
    {
        var trans = transform.Find("AttachPoints");
        if (trans == null)
            return null;

        var childCount = trans.childCount;
        var index = UnityEngine.Random.Range(0, childCount);
        return trans.GetChild(index);
    }

    public Transform GetAttachPoint(string name)
    {
        var trans = transform.Find("AttachPoints");
        if (trans == null)
            return null;

        foreach(Transform t in trans)
        {
            if (string.Compare(t.name, name) == 0)
                return t;
        }
        return null;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPauseGame())
            return;

        for (int i = 0; i < _skillDatas.Count; i++) 
        {
            _skillDatas[i].OnUpdateBattle();
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        UID = 0;
        RemoveSkills();
    }

    protected override void Death(UnitDeathInfo info)
    {
        base.Death(info);
    }

    private void InitAIShipSkill()
    {
        var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.AIShipSkill, this);
        UID = uid;
        var triggers = AIShipCfg.Skills;
        if (triggers != null && triggers.Length > 0)
        {
            for (int i = 0; i < triggers.Length; i++)
            {
                var triggerData = triggers[i].Create(triggers[i], uid);
                if (triggerData != null)
                {
                    triggerData.UID = uid;
                    triggerData.OnTriggerAdd();
                    _skillDatas.Add(triggerData);
                }
            }
        }
    }

    private void RemoveSkills()
    {
        for (int i = 0; i < _skillDatas.Count; i++)
        {
            _skillDatas[i].OnTriggerRemove();
        }
    }

    #region Extra Units

    private void InitExtraUnitGroups()
    {
        for (int i = 0; i < ExtraUnitGroup.Length; i++) 
        {
            ExtraUnitGroup[i].SafeSetActive(false);
        }
    }

    /// <summary>
    /// 激活额外units
    /// </summary>
    /// <param name="name"></param>
    public void ActiveExtraUnits(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        var parent = GetExtraUnitGroup(name);
        if(parent != null)
        {
            parent.SafeSetActive(true);
            var allUnits = parent.GetComponentsInChildren<Unit>();
            for (int i = 0; i < allUnits.Length; i++) 
            {
                AddExtraUnit(allUnits[i]);
            }
        }
    }

    private Transform GetExtraUnitGroup(string name)
    {
        for (int i = 0; i < ExtraUnitGroup.Length; i++)
        {
            if (string.Compare(name, ExtraUnitGroup[i].name) == 0)
            {
                return ExtraUnitGroup[i];
            }
        }
        return null;
    }

    private void AddExtraUnit(Unit unit)
    {
        var unitconfig = DataManager.Instance.GetUnitConfig(unit.UnitID);
        unit.gameObject.SetActive(true);

        unit.Initialization(this, unitconfig);
        unit.SetUnitProcess(true);
        _unitList.Add(unit);
        if (unit.IsCoreUnit)
        {
            CoreUnits.Add(unit);
        }
        ///Do Spawn
        unit.DoSpawnEffect();

        if (AIShipCfg.BillboardType == EnemyHPBillBoardType.Elite_Scene)
        {
            ///注册Unit的血条显示
            unit.RegisterHPBar();
        }
    }

    #endregion
}
