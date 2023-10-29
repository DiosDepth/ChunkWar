using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera { get { return _currentCamera; } }
    public CinemachineVirtualCamera vcam { get { return _currentVCamera; } }
    public CinemachineBrain cameraBrain { get { return _currentCameraBrain; } }
    private Dictionary<string, CinemachineVirtualCamera> vcamDic = new Dictionary<string, CinemachineVirtualCamera>();

    public float minCameraOrthSize = 10;

    private bool isCameraUpdate;
    private Camera _currentCamera;
    private CinemachineBrain _currentCameraBrain;
    private CinemachineVirtualCamera _currentVCamera;
    private CinemachineBasicMultiChannelPerlin _shakeNoise;

    private string _resPath = "Prefab/CameraPrefab/";
    private string _mainCameraName = "MainCamera";
    private string _inGameCMvcamName = "InGameCMvcam";


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

        _currentCameraBrain = mainCamera.GetComponent<CinemachineBrain>();
        _currentVCamera = CreateVCamera(_inGameCMvcamName);

        MonoManager.Instance.DontDestroyOnLoad(_currentCamera);
        MonoManager.Instance.DontDestroyOnLoad(_currentVCamera);
    }

    /// <summary>
    /// 相机震动
    /// </summary>
    /// <param name="cfg"></param>
    public async void ShakeBattleCamera(CameraShakeConfig cfg)
    {
        if (_isShaking)
            return;

        _isShaking = true;
        var Amplitude = UnityEngine.Random.Range(cfg.Amplitude.x, cfg.Amplitude.y);
        var frequency = UnityEngine.Random.Range(cfg.Frequency.x, cfg.Frequency.y);

        _shakeNoise.m_AmplitudeGain = Amplitude;
        _shakeNoise.m_FrequencyGain = frequency;

        await UniTask.Delay((int)(cfg.Duration * 1000));
        ///Reset
        if (_currentVCamera == null || _shakeNoise == null)
            return;

        _shakeNoise.m_AmplitudeGain = 0;
        _shakeNoise.m_FrequencyGain = 0;
        _isShaking = false;
    }

    public void SetCameraUpdate(bool isupdate)
    {
        isCameraUpdate = isupdate;
    }
    public Camera CreateMainCamera(string cameraname)
    {
         return ResManager.Instance.Load<GameObject>(_resPath + cameraname).GetComponent<Camera>();
    }

    public CinemachineVirtualCamera CreateVCamera(string cameraname)
    {
        CinemachineVirtualCamera vcam;
        vcam = ResManager.Instance.Load<GameObject>(_resPath + cameraname).GetComponent<CinemachineVirtualCamera>();
        if(vcam == null)
        {
            return null;
        }

        vcamDic[_inGameCMvcamName] = vcam;
        _shakeNoise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        return vcam;
    }

    public void SetFollowPlayerShip(float offsetX = 0, bool force = true)
    {
        var shipTrans = RogueManager.Instance.currentShip;
        if (shipTrans == null)
            return;

        ChangeVCameraFollowTarget(shipTrans.transform);
        vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = offsetX;
        vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = 0;
    }

    public void SetOrthographicSize(int size, float duration = 0f)
    {
        if(duration <= 0f)
        {
            _currentVCamera.m_Lens.OrthographicSize = size;
            return;
        }

        var currentSize = _currentVCamera.m_Lens.OrthographicSize;
        LeanTween.value(_currentVCamera.gameObject, currentSize, size, duration).setOnUpdate(
            (value) =>
            {
                _currentVCamera.m_Lens.OrthographicSize = value;
            }).setOnComplete(() =>
            {
                _currentVCamera.m_Lens.OrthographicSize = size;
            });
    }

    public void ResetMainCamera()
    {

    }

    public CinemachineVirtualCamera  GetVCamera(string name = "InGameCMvcam")
    {
        CinemachineVirtualCamera vcam;
        vcamDic.TryGetValue(name, out vcam);
        if(vcam == null)
        {
            return null;
        }
        return vcam;
    }


    public void ChangeVCameraLookAtTarget(Transform target, string vcamname = "InGameCMvcam")
    {
        
        CinemachineVirtualCamera vcam;
        vcamDic.TryGetValue(vcamname, out vcam);
        if(vcam == null)
        {
            Debug.Log("Can't find " + vcamname + " in vcamDic");
            return;
        }
        vcam.LookAt = target;
    }

    public void ChangeVCameraFollowTarget(Transform target, bool force = true, string vcamname = "InGameCMvcam")
    {
        CinemachineVirtualCamera vcam;
        vcamDic.TryGetValue(vcamname, out vcam);
        if (vcam == null)
        {
            Debug.Log("Can't find " + vcamname + " in vcamDic");
            return;
        }

        if (target == null)
            return;

        vcam.Follow = target;
        if (force)
        {
            vcam.ForceCameraPosition(target.transform.position, target.transform.rotation);
        }
    }

    public void SetVCameraBoard(Collider2D board, string vcamname = "InGameCMvcam")
    {

        CinemachineVirtualCamera vcam;
        vcamDic.TryGetValue(vcamname, out vcam);
        if (vcam == null)
        {
            Debug.Log("Can't find " + vcamname + " in vcamDic");
            return;
        }

        vcam.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = board;
    }

}
