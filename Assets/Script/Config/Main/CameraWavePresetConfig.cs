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
    [LabelText("波纹距离系数")]
    [LabelWidth(100)]
    public float distanceFactor = 60f;
    [LabelText("波纹时间系数")]
    [LabelWidth(100)]
    public float timeFactor = -30f;
    [LabelText("波纹")]
    [LabelWidth(100)]
    public float totalFactor = 1f;

    [LabelText("宽度")]
    [LabelWidth(100)]
    public float waveWidth = 0.3f;

    [LabelText("速度")]
    [LabelWidth(100)]
    public float waveSpeed = 0.3f;

    [LabelText("持续时间")]
    [LabelWidth(100)]
    public float Duration = 1f;
}
