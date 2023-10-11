using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : Singleton<SoundManager>
{
    public enum SoundType
    {
        SFX,
        BGM,
    }

    private float m_BGMVolume = 1;
    private string m_currentBGM;
    private EventInstance BGMEventInstance;

    public SoundManager()
    {
    }

    public override void Initialization()
    {
        base.Initialization();
        LoadBanks();
    }

    #region BGM
    /// <summary>
    /// ≤•∑≈BGM
    /// </summary>
    /// <param name="bgmEvent"></param>
    /// <param name="volume"></param>
    public void PlayBGM(string bgmEvent, float volume = 1f)
    {
        m_currentBGM = "event:/" + bgmEvent;
        m_BGMVolume = volume;
        RefreshBGMEvent();
    }

    public void SetBGMVolume(float value)
    {
        BGMEventInstance.setVolume(value);
    }

    #endregion

    public void PlayBattleSound(string eventName, Transform trans)
    {
        if (string.IsNullOrEmpty(eventName))
            return;
        var evtParam = "event:/" + eventName;
        RuntimeManager.PlayOneShot(evtParam, trans.position);
    }

    private void LoadBanks()
    {
        var bankFiles = Resources.LoadAll<TextAsset>(DataConfigPath.AudioBankRoot);
        for(int i = 0; i < bankFiles.Length; i++)
        {
            RuntimeManager.LoadBank(bankFiles[i]);
        }
    }

    /// <summary>
    /// À¢–¬±≥æ∞“Ù¿÷
    /// </summary>
    private void RefreshBGMEvent()
    {
        if (string.IsNullOrEmpty(m_currentBGM))
            return;

        if (BGMEventInstance.isValid())
        {
            BGMEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            BGMEventInstance.release();
        }

        BGMEventInstance = RuntimeManager.CreateInstance(m_currentBGM);

        SetBGMVolume(m_BGMVolume);
        BGMEventInstance.start();

        ///Lerp Volume TOOD
    }

}
