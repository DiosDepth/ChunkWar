using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : PoolableObject
{

    public ParticleSystem system;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayVFX()
    {
        system.Play();
    }
    public override void Reset()
    {
        base.Reset();
    }

    public override void SetActive(bool isactive = true)
    {
        base.SetActive(isactive);
    }

    public override void Destroy()
    {
        base.Destroy();


    }
    private void OnParticleSystemStopped()
    {
        Destroy();
    }

}
