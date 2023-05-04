using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundManager : Singleton<SoundManager>
{
    public enum SoundType
    {
        SFX,
        BGM,
    }


    public string soundSourcePath = "Prefab/Sound/SoundSource";


    private SoundSource BGMSource = null;
    private float BGMVolume = 1;

    private GameObject SFXContainer = null;
    private List<SoundSource> _SFXList = new List<SoundSource>();
    private float SFXVolume = 1;

    private Dictionary<string, AudioClip> _BGMAudioClipDic = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _SFXAudioClipDic = new Dictionary<string, AudioClip>();




    public SoundManager()
    {
        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();

    }
    public void Play(string name, SoundType type, bool isloop = false)
    {
        //��ȡ������Ϣ
        SoundDataInfo soundinfo;
        DataManager.Instance.SoundDataDic.TryGetValue(name, out soundinfo);
        if(soundinfo == null)
        {
            Debug.LogWarning("Can't find sound path from SoundDataDic");
            return;
        }

        //�������BGM
        if(type == SoundType.BGM)
        {
            if(BGMSource == null)
            {
                PoolManager.Instance.GetObject(soundSourcePath, true, (obj) => 
                {
                    BGMSource = obj.GetComponent<SoundSource>();
                    BGMSource.source = obj.GetComponent<AudioSource>();
                    BGMSource.source.volume = BGMVolume;
                    if (_BGMAudioClipDic.ContainsKey(name))
                    {

                        BGMSource.source.clip = _BGMAudioClipDic[name];
                        BGMSource.Play(isloop);
                    }
                    else
                    {
                        ResManager.Instance.LoadAsync<AudioClip>(soundinfo.SoundSourcePath, (audioclip) =>
                        {
                            if(audioclip == null)
                            {
                                Debug.LogError("AudioClip is null, please check SoundData");
                            }
                            else
                            {
                                _BGMAudioClipDic.Add(name, audioclip);
                                BGMSource.source.clip = _BGMAudioClipDic[name];
                                BGMSource.Play(isloop);
                            }

                        });
                    }

                });
            }
            else
            {
                if (_BGMAudioClipDic.ContainsKey(name))
                {
                    BGMSource.source.clip = _BGMAudioClipDic[name];
                    BGMSource.Play(isloop);
                }
                else
                {
                    ResManager.Instance.LoadAsync<AudioClip>(soundinfo.SoundSourcePath, (audioclip) =>
                    {
                        if (audioclip == null)
                        {
                            Debug.LogError("AudioClip is null, please check SoundData");
                        }
                        else
                        {
                            _BGMAudioClipDic.Add(name, audioclip);
                            BGMSource.source.clip = _BGMAudioClipDic[name];
                            BGMSource.Play(isloop);
                        }

                    });
                }
            }
        }

        //�������SFX
        if(type == SoundType.SFX)
        {
            if(SFXContainer == null)
            {
                SFXContainer = new GameObject();
                SFXContainer.name = "SFXContrainer";
            }

            PoolManager.Instance.GetObject(soundSourcePath, true, (obj) =>
            {
                obj.transform.parent = SFXContainer.transform;
               SoundSource tempsound =  obj.GetComponent<SoundSource>();
                tempsound.source = obj.GetComponent<AudioSource>();
                tempsound.source.volume = SFXVolume;

                if(!_SFXList.Contains(tempsound))
                {
                    _SFXList.Add(tempsound);
                }

                if (_SFXAudioClipDic.ContainsKey(name))
                {
                    tempsound.source.clip = _SFXAudioClipDic[name];
                    tempsound.Play();
                }
                else
                {
                    ResManager.Instance.LoadAsync<AudioClip>(soundinfo.SoundSourcePath, (audioclip) =>
                    {
                        _SFXAudioClipDic.Add(name, audioclip);
                        tempsound.source.clip = _SFXAudioClipDic[name];
                        tempsound.Play();
                    });
                }
            });
        }
    }


    public void StopPlay(SoundType type)
    {
        switch (type)
        {
            case SoundType.SFX:
                for (int i = 0; i < _SFXList.Count; i++)
                {
                    _SFXList[i].Stop();
                }
                break;
            case SoundType.BGM:
                BGMSource.Stop();
                break;
        }
    }



    public void Clear(SoundType type)
    {
        if(type == SoundType.BGM)
        {
            _BGMAudioClipDic.Clear();
        }
        if(type == SoundType.SFX)
        {
            _SFXAudioClipDic.Clear();
            _SFXList.Clear();
        }
    }
    public void ChangeVolume (SoundType type, float volume)
    {
        switch (type)
        {
            case SoundType.SFX:
                SFXVolume = volume;
                for (int i = 0; i < _SFXList.Count; i++)
                {
                    _SFXList[i].source.volume = SFXVolume;
                }
                break;
            case SoundType.BGM:
                BGMVolume = volume;
                BGMSource.source.volume = BGMVolume;
                break;
        }

    }



}
