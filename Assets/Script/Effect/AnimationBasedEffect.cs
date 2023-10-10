using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBasedEffect : BaseEffect
{
    private Animation _anim;

    public override void Awake()
    {
        base.Awake();
        _anim = transform.SafeGetComponent<Animation>();
    }

    public override void OnCreate()
    {
        base.OnCreate();
        _anim.Play();
    }
}
