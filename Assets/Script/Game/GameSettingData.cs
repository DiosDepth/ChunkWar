using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingData 
{
    public byte TargetMaxFrame = 60;

    public float MaxSoundVolume = 1.0f;
    public float MaxSoundEffectVolume = 1.0f;
    public float MaxSoundBGMVolume = 1.0f;

    /// <summary>
    /// ´¹Ö±Í¬²½
    /// </summary>
    public bool VerticalSync
    {
        get { return m_VerticalSync; }
        set
        {
            m_VerticalSync = value;
            if (m_VerticalSync)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }
        }
    }

    private bool m_VerticalSync = false;
}
