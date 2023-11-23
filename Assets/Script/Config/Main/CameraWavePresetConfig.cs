using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_CameraWave_", menuName = "Configs/WavePreset")]
public class CameraWavePresetConfig : SerializedScriptableObject
{
    public List<CameraWavePresetItemConfig> Presets = new List<CameraWavePresetItemConfig>();

}

[System.Serializable]
public class CameraWavePresetItemConfig
{
    public int PresetID;
    [LabelText("���ƾ���ϵ��")]
    [LabelWidth(100)]
    public float distanceFactor = 60f;
    [LabelText("����ʱ��ϵ��")]
    [LabelWidth(100)]
    public float timeFactor = -30f;
    [LabelText("����")]
    [LabelWidth(100)]
    public float totalFactor = 1f;

    [LabelText("���")]
    [LabelWidth(100)]
    public float waveWidth = 0.3f;

    [LabelText("�ٶ�")]
    [LabelWidth(100)]
    public float waveSpeed = 0.3f;

    [LabelText("����ʱ��")]
    [LabelWidth(100)]
    public float Duration = 1f;
}
