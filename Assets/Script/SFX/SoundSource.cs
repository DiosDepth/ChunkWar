using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundSource : MonoBehaviour,IPoolable
{

    public AudioSource source;

    private Coroutine playsoundCoroutine;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
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
        PoolableDestroy();
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

    public virtual void Initialization()
    {

    }

    public void PoolableDestroy()
    {

    }

    public void PoolableReset()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }



}
