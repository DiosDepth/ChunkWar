using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundSource : PoolableObject
{

    public AudioSource source;

    private Coroutine playsoundCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(UnityAction callback = null)
    {

        playsoundCoroutine = StartCoroutine(PlayOnce(callback));

    }

    public IEnumerator PlayOnce(UnityAction callback = null)
    {

        float length = source.clip.length;
        float timestamp = Time.time;
        source.loop = false;
        source.Play();

        while (source.isPlaying)
        {
            yield return null;
        }
        
        if(callback != null)
        {
            callback.Invoke();
        }
        Destroy();
    }
    public void Play(bool isloop)
    {
        source.loop = isloop;
        source.Play();
    }

    public void Stop()
    {
        if(playsoundCoroutine != null)
        {
            StopCoroutine(playsoundCoroutine);
            playsoundCoroutine = null;
        }

        source.Stop();
    }

    public void Pause()
    {
        source.Pause();
    }
    public void UnPause()
    {
        source.UnPause();
    }

    public override void SetActive(bool isactive = true)
    {
        base.SetActive(isactive);
    }

    public override void StartSelf()
    {
        base.StartSelf();
    }

    public override void ResetSelf()
    {
        base.ResetSelf();
    }

    public override void Destroy()
    {
        Stop();
        base.Destroy();
    }
}
