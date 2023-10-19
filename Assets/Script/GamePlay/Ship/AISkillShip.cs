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

    private List<ModifyTriggerData> _skillDatas = new List<ModifyTriggerData>();

    public override void Initialization()
    {
        base.Initialization();
        InitAIShipSkill();
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
}
