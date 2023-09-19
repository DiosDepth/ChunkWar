using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera { get { return _currentCamera; } }
    public CinemachineVirtualCamera vcam { get { return _currentVCamera; } }
    public CinemachineBrain cameraBrain { get { return _currentCameraBrain; } }
    private Dictionary<string, CinemachineVirtualCamera> vcamDic = new Dictionary<string, CinemachineVirtualCamera>();

    public float minCameraOrthSize = 10;
    public Transform referencePoint;

    private bool isCameraUpdate;
    private Camera _currentCamera;
    private CinemachineBrain _currentCameraBrain;
    private CinemachineVirtualCamera _currentVCamera;
    private string _resPath = "Prefab/CameraPrefab/";
    private string _mainCameraName = "MainCamera";
    private string _inGameCMvcamName = "InGameCMvcam";


    private float _refvel;

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
        MonoManager.Instance.AddUpdateListener(UpdateOrthSizeByDistance);
    }
    

    public void SetReferencePoint(Transform reftrs)
    {
        referencePoint = reftrs;
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
        return vcam;
    }

    public void SetFollowPlayerShip(float offsetX = 0)
    {
        var shipTrans = RogueManager.Instance.currentShip;
        if (shipTrans == null)
            return;

        ChangeVCameraFollowTarget(shipTrans.transform);
        vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = offsetX;
        vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = 0;
    }

    public void SetOrthographicSize(int size)
    {
        vcam.m_Lens.OrthographicSize = size;
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

    public void ChangeVCameraFollowTarget(Transform target, string vcamname = "InGameCMvcam")
    {
        CinemachineVirtualCamera vcam;
        vcamDic.TryGetValue(vcamname, out vcam);
        if (vcam == null)
        {
            Debug.Log("Can't find " + vcamname + " in vcamDic");
            return;
        }
        vcam.Follow = target;
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

    public void SetVCameraPos(Vector2 pos)
    {
        vcam.ForceCameraPosition(pos, Quaternion.identity);
    }

    public void UpdateOrthSizeByDistance()
    {
        if(isCameraUpdate)
        {
           float size= Mathf.Max(minCameraOrthSize, Mathf.Abs(_currentVCamera.Follow.position.y - referencePoint.position.y));
            _currentVCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(_currentVCamera.m_Lens.OrthographicSize, size, ref _refvel, 0.5f);
        }
      
    }
}
