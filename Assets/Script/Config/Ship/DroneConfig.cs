using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


/// <summary>
/// ���˻�����
/// </summary>
[CreateAssetMenu(fileName = "Configs_Drone_", menuName = "Configs/Drones")]
public class DroneConfig : BaseShipConfig
{

    [FoldoutGroup("���˻�����")]
    [HorizontalGroup("���˻�����/A", 300)]
    [LabelText("������")]
    [LabelWidth(80)]
    public OwnerType Owner;

    [FoldoutGroup("��Ϊ����")]
    [HorizontalGroup("��Ϊ����/A")]
    [BoxGroup("��Ϊ����/A/����ٶ�")]
    [HideLabel]
    public float MaxVelocity;

    [FoldoutGroup("��Ϊ����")]
    [HorizontalGroup("��Ϊ����/A")]
    [BoxGroup("��Ϊ����/A/�����ٶ�")]
    [HideLabel]
    public float MaxAcceleration;

    [FoldoutGroup("��Ϊ����")]
    [HorizontalGroup("��Ϊ����/A")]
    [BoxGroup("��Ϊ����/A/���ת�ٶ�")]
    [HideLabel]
    public float MaxAngularAcceleration = 3f;

    [FoldoutGroup("��Ϊ����")]
    [HorizontalGroup("��Ϊ����/A")]
    [BoxGroup("��Ϊ����/A/�����ת�ٶ�")]
    [HideLabel]
    public float MaxAngularVelocity = 1f;

    [FoldoutGroup("��Ϊ����")]
    [HorizontalGroup("��Ϊ����/A")]
    [BoxGroup("��Ϊ����/A/�뾶��С")]
    [HideLabel]
    public float boidRadius = 1f;

    [FoldoutGroup("��Ϊ����")]
    [HorizontalGroup("��Ϊ����/A")]
    [BoxGroup("��Ϊ����/A/�����뾶")]
    [HideLabel]
    public float targetSerchingRadius = 15f;

    [FoldoutGroup("Ч������")]
    [HorizontalGroup("Ч������/A")]
    public string DieAudio;

    [FoldoutGroup("Ч������")]
    [LabelText("������Ч")]
    public GeneralEffectConfig DeathEffect;


    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override Vector2Int GetMapPivot()
    {
        return base.GetMapPivot();
    }

#if UNITY_EDITOR
    [OnInspectorInit]
    protected override void InitData()
    {
        base.InitData();

    }
#endif
}
