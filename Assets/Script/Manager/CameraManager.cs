using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Com.LuisPedroFonseca.ProCamera2D;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera { get { return _currentCamera; } }

    public float minCameraOrthSize = 10;

    private bool isCameraUpdate;
    private Camera _currentCamera;
    private ProCamera2D _proCamera;
    private ProCamera2DShake _shakeCmpt;

    private string _resPath = "Prefab/CameraPrefab/";
    private string _mainCameraName = "MainCamera";


    private float _refvel;
    private bool _isShaking = false;
    private float shakeDuration = 0.5f;

    public CameraManager()
    {
        Initialization();
    }
    public override void Initialization()
    {
        if(_currentCamera != null)
        {
            return;
        }
        base.Initialization();
        _currentCamera = CreateMainCamera(_mainCameraName);
        _proCamera = _currentCamera.transform.SafeGetComponent<ProCamera2D>();
        _shakeCmpt = _currentCamera.transform.SafeGetComponent<ProCamera2DShake>();
        _shakeCmpt.OnShakeCompleted += () =>
        {
            _isShaking = false;
        };
        _currentCamera.orthographicSize = GameGlobalConfig.CameraDefault_OrthographicSize;
        MonoManager.Instance.DontDestroyOnLoad(_currentCamera);
        MonoManager.Instance.DontDestroyOnLoad(_shakeCmpt.transform.parent);
    }

    /// <summary>
    /// 相机震动
    /// </summary>
    /// <param name="cfg"></param>
    public async void ShakeBattleCamera(CameraShakeConfig cfg)
    {
        if (_isShaking)
        {
            _shakeCmpt.StopShaking();
        }

        if(cfg.UsePreset && !string.IsNullOrEmpty(cfg.PresetName))
        {
            _shakeCmpt.Shake(cfg.PresetName);
        }
        
        var Amplitude = UnityEngine.Random.Range(cfg.Amplitude.x, cfg.Amplitude.y);
        var frequency = UnityEngine.Random.Range(cfg.Frequency.x, cfg.Frequency.y);
    }

    public void SetCameraUpdate(bool isupdate)
    {
        isCameraUpdate = isupdate;
    }
    public Camera CreateMainCamera(string cameraname)
    {
         return ResManager.Instance.Load<GameObject>(_resPath + cameraname).GetComponent<Camera>();
    }

    public void SetFollowPlayerShip(float offsetX = 0, bool force = true)
    {
        var shipTrans = RogueManager.Instance.currentShip;
        if (shipTrans == null)
            return;

        ChangeVCameraFollowTarget(shipTrans.transform);
        if (force)
        {
            _proCamera.MoveCameraInstantlyToPosition(shipTrans.transform.position);
        }
    }

    public void SetFOV(int size)
    {
        _currentCamera.orthographicSize = size;
    }

    public void ChangeVCameraFollowTarget(Transform target, bool force = true)
    {
        if (_proCamera == null)
            return;

        _proCamera.AddCameraTarget(target);
    }

    public void SetVCameraBoard(Collider2D board, string vcamname = "InGameCMvcam")
    {
    }

}
