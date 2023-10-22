using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Cysharp.Threading.Tasks;

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
    }

    #region BGM
    /// <summary>
    /// ≤•∑≈BGM
    /// </summary>
    /// <param name="bgmEvent"></param>
    /// <param name="volume"></param>
    public void PlayBGM(string bgmEvent, float volume = 1f)
    {
        m_currentBGM = "event:/BGM/" + bgmEvent;
        m_BGMVolume = volume;
        RefreshBGMEvent();
    }

    public void SetBGMVolume(float value)
    {
        BGMEventInstance.setVolume(value);
    }

    #endregion

    public async void PlayBattleSound(string eventName, Transform trans, float randomDelay = 0f)
    {
        if (string.IsNullOrEmpty(eventName))
            return;
        var evtParam = "event:/" + eventName;

        if (randomDelay > 0) 
        {
            var delayTime = UnityEngine.Random.Range(0, randomDelay);
            await UniTask.Delay((int)(delayTime * 1000));
        }
        RuntimeManager.PlayOneShot(evtParam, trans.position);
    }

    public void Play2DSound(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;
        var evtParam = "event:/" + eventName;
        RuntimeManager.PlayOneShot(evtParam);
    }
    public async void PlayUISound(string eventName, float delayTime = 0f)
    {
        if (string.IsNullOrEmpty(eventName))
            return;
        var evtParam = "event:/UI/" + eventName;
        if(delayTime > 0)
        {
            await UniTask.Delay((int)(delayTime * 1000));
        }

        RuntimeManager.PlayOneShot(evtParam);
    }

    public void LoadBanks()
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
