using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_Camp_", menuName = "Configs/Camps")]
public class CampConfig : SerializedScriptableObject
{
    [HorizontalGroup("A", 150)]
    [LabelText("ID")]
    [LabelWidth(50)]
    public int CampID;

    [HorizontalGroup("A", 400)]
    [LabelText("����")]
    [LabelWidth(50)]
    public string CampName;

    [HorizontalGroup("A", 400)]
    [LabelText("����")]
    [LabelWidth(50)]
    public string CampDesc;

    [HorizontalGroup("B", 200)]
    [LabelText("Ĭ�Ͻ���")]
    [LabelWidth(50)]
    public bool Unlock;

    [PreviewField(80, Alignment = ObjectFieldAlignment.Left)]
    public Sprite CampIcon;

    [ListDrawerSettings(CustomAddFunction ="AddLevel")]
    [LabelText("�ȼ�����")]
    public CampLevelConfig[] LevelConfigs = new CampLevelConfig[0];

    [ListDrawerSettings(CustomAddFunction = "AddBuff")]
    [LabelText("BUFF����")]
    public CommonBuffItem[] BuffItems = new CommonBuffItem[0];

    private CampLevelConfig AddLevel()
    {
        return new CampLevelConfig();
    }

    private CommonBuffItem AddBuff()
    {
        return new CommonBuffItem();
    }
}

public enum GeneralUnlockItemType
{
    Ship,
    ShipUnit,
    ShipPlug
}

[HideReferenceObjectPicker]
public class CampLevelConfig
{
    public int LevelIndex;
    public int RequireTotalEXP;

    public GeneralUnlockItemType UnlockType;
    public int UnlockItemID;
}

[HideReferenceObjectPicker]
public class CommonBuffItem
{
    [HorizontalGroup("B", 400)]
    [LabelText("Key")]
    [LabelWidth(50)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("B", 400)]
    [LabelText("Key")]
    [LabelWidth(50)]
    public float[] LevelMap = new float[0];

    [LabelText("���ı�")]
    [LabelWidth(50)]
    public int[] CostMap = new int[0];
}