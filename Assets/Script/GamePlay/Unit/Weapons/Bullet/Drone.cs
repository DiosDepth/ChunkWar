using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : Bullet
{
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }


    public override void Reset()
    {
        base.Reset();
    }

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void Destroy()
    {
        base.Destroy();
    }

    public override void SetActive(bool isactive = true)
    {
        base.SetActive(isactive);
    }
}
