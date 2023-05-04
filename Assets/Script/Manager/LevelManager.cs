using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public enum LevelEventType
{
    LevelLoading,
    LevelLoaded,
    LevelActive,

}

public struct LevelEvent
{
    public LevelEventType evtType;
    public int levelIndex;
    public AsyncOperation asy;

    public LevelEvent(LevelEventType m_type, int m_index, AsyncOperation m_asy )
    {
        evtType = m_type;
        levelIndex = m_index;
        asy = m_asy;
    }
    private static LevelEvent e;

    public static void Trigger(LevelEventType m_type, int m_index, AsyncOperation m_asy)
    {
        e.evtType = m_type;
        e.levelIndex = m_index;
        e.asy = m_asy;
        EventCenter.Instance.TriggerEvent<LevelEvent>(e);
    }
}

public class LevelManager : Singleton<LevelManager>,EventListener<LevelEvent>
{

    private AsyncOperation asy;
    public string playerPrefabPath = "Prefab/PlayerPrefab/Player";


    public Transform startPoint;
    public SpawnLine[] spawnLineList;
    public Character player;



    public LevelManager()
    {
        Initialization();
        this.EventStartListening<LevelEvent>();

    }

    public override void Initialization()
    {
        base.Initialization();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEvent(LevelEvent evt)
    {
        switch (evt.evtType)
        {
            case LevelEventType.LevelLoading:
                break;
            case LevelEventType.LevelLoaded:
                //run in the LoadingScreen.cs

                break;
            case LevelEventType.LevelActive:

              
                break;
        }

    }
    public void CollectLevelInfo()
    {
        startPoint = GameObject.Find("StartPoint").transform;
        spawnLineList = GameObject.FindObjectsOfType<SpawnLine>();
        player = GameObject.FindObjectOfType<Character>();
    }


    public void StartSpawn()
    {
        for (int i = 0; i < spawnLineList.Length; i++)
        {
            spawnLineList[i].StartSpawnMovingBlocker();
        }
    }

    public void StopSpawn()
    {
        for (int i = 0; i < spawnLineList.Length; i++)
        {
            spawnLineList[i].StopSpawn();
        }
    }
    public IEnumerator SpawnActorAtPos(string prefabpath, bool isactive, Vector3 pos, Quaternion rot, UnityAction<GameObject> callback)
    {
        ResManager.Instance.LoadAsync<GameObject>(prefabpath, (obj) =>
        {
            obj.SetActive(isactive);
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            if (callback != null)
            {
                callback(obj);
            }

        });
        yield return null;
    }
    public IEnumerator SpawnActorAtPos(string prefabpath,bool isactive,Transform trs, UnityAction<GameObject> callback)
    {
        ResManager.Instance.LoadAsync<GameObject>(prefabpath, (obj) =>
        {
            obj.SetActive(isactive);
            obj.transform.position = trs.position;
            obj.transform.rotation = trs.rotation;
            if(callback != null)
            {
                callback(obj);
            }
            
        });
        yield return null;
    }

    public IEnumerator LoadScene(int levelindex,UnityAction<AsyncOperation> callback)
    {
        asy = SceneManager.LoadSceneAsync(levelindex);
        asy.allowSceneActivation = false;

        while (asy.progress < 0.9f)
        {
            yield return new WaitForEndOfFrame();
        }
        asy.allowSceneActivation = true;
        yield return null;
        callback?.Invoke(asy);

        yield return null;
    }
    public IEnumerator LevelPreparing(UnityAction callback = null)
    {
        // yield return new WaitForSeconds(5);
        CollectLevelInfo();
       
        if (player == null)
        {
            yield return LevelManager.Instance.SpawnActorAtPos(playerPrefabPath, false, startPoint, (obj) =>
            {

                LevelManager.Instance.player = obj.GetComponent<Character>();

                CameraManager.Instance.ChangeVCameraFollowTarget(obj.transform);
                //CameraManager.Instance.ChangeVCameraLookAtTarget(obj.transform);
                CameraManager.Instance.SetVCameraBoard(GameObject.Find("CameraBoard").GetComponent<PolygonCollider2D>());
                CameraManager.Instance.SetReferencePoint(GameObject.Find("DeadZone").transform);
                CameraManager.Instance.SetCameraUpdate(true);
                PoolManager.Instance.ClearAll();
                callback?.Invoke();
            });
        }
        else
        {
            LevelReset();
          
            callback?.Invoke();
        }
        yield return null;
    }
    public void GameOver()
    {
        StopSpawn();
       
        
        CameraManager.Instance.SetCameraUpdate(false);
    }

    public void LevelReset()
    {
        player.transform.position = startPoint.position;
        player.transform.rotation = Quaternion.identity;
        player.rb.velocity = Vector3.zero;
        player.gameObject.SetActive(true);
        player.Initialization();
   
        StartSpawn();
    }
}
