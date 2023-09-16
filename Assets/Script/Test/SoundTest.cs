using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnGUI()
    {
       if( GUILayout.Button("PlayReload"))
        {
            SoundManager.Instance.Play("SFX_shotgun_reload",SoundManager.SoundType.SFX);
        }

        if (GUILayout.Button("PlayShot"))
        {
            SoundManager.Instance.Play("SFX_shotgun_shot", SoundManager.SoundType.SFX);
        }

        if (GUILayout.Button("Playswing"))
        {
            SoundManager.Instance.Play("SFX_katana_swing", SoundManager.SoundType.SFX);
        }

        if (GUILayout.Button("Playhit"))
        {
            SoundManager.Instance.Play("SFX_katana_hit", SoundManager.SoundType.SFX);
        }
    }

    
}
