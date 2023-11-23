using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostEffectManager : MonoBehaviour
{
    [FoldoutGroup("²¨ÎÆ")]
    [LabelText("²¨ÎÆ¾àÀëÏµÊý")]
    public float distanceFactor = 120.0f;
    [FoldoutGroup("²¨ÎÆ")]
    [LabelText("²¨ÎÆÊ±¼äÏµÊý")]
    public float timeFactor = -30.0f;
    [FoldoutGroup("²¨ÎÆ")]
    [LabelText("SinÏµÊý")]
    public float totalFactor = 1f;

    [FoldoutGroup("²¨ÎÆ")]
    [LabelText("²¨¿í¶È")]
    public float waveWidth = 1f;
    [FoldoutGroup("²¨ÎÆ")]
    [LabelText("²¨ËÙ¶È")]
    public float waveSpeed = 0.2f; 

    private float waveStartTime;
    private Vector4 startPos; 
    private Material waveMaterial;

    private bool enableWave = false;

    private void Awake()
    {
        waveMaterial = new Material(Shader.Find("Main/WaveEffect"));
        waveMaterial.hideFlags = HideFlags.DontSave;
    }

    public void CreateWave(Vector2 position)
    {
        var screenPos = Camera.main.WorldToScreenPoint(position);
        startPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
        enableWave = true;
        waveStartTime = Time.time;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (enableWave)
        {
            SetWaveMaterialParams();
            Graphics.Blit(source, destination, waveMaterial);
            //waveStartTime += Time.deltaTime;
            //if (waveStartTime > 2 / waveSpeed)
            //{ 
            //    enableWave = false;
            //}
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    private void SetWaveMaterialParams()
    {
        float curWaveDistance = (Time.time - waveStartTime) * waveSpeed;
        waveMaterial.SetFloat("_distanceFactor", distanceFactor);
        waveMaterial.SetFloat("_timeFactor", timeFactor);
        waveMaterial.SetFloat("_totalFactor", totalFactor);
        waveMaterial.SetFloat("_waveWidth", waveWidth);
        waveMaterial.SetFloat("_curWaveDis", curWaveDistance);
        waveMaterial.SetVector("_startPos", startPos);
    }
}
