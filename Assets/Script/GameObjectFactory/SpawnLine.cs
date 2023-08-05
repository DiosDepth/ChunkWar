using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpawnLine : MonoBehaviour
{
    public Vector3 lineEnd;

    public bool flipDirection = false;
    [SerializeField]
    public Vector3 moveDirction;
    [Range(0,1)]
    public float threshold;


    [Header("---SpawnSettings---")]
    public float spawnIntervalMin = 1;
    public float spawnIntervalMax = 2;

    private Vector3 _randomThresholdPoint;
    private float _thresholdLength;
    private Vector3 _thresholdDirection;
    // Start is called before the first frame update
    void Start()
    {
        _randomThresholdPoint = Vector3.Lerp(transform.position, transform.TransformPoint(lineEnd), threshold);
        _thresholdLength = (transform.TransformPoint( lineEnd) - _randomThresholdPoint).magnitude;
        _thresholdDirection = (transform.TransformPoint(lineEnd) - _randomThresholdPoint).normalized;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartSpawnMovingBlocker()
    {
        StartCoroutine("Spawn");

    }

    public IEnumerator Spawn()
    {
        float stamp = 0;
        while(true)
        {
            if (Time.time >= stamp)
            {
                //Ë¢ÐÂµÐÈË
            }
            yield return null;
        }
    }
    public void StopSpawn()
    {
        StopCoroutine("Spawn");
    }



    public Vector3  GetRadomPosFromLine()
    {
        Vector3 info = new Vector3();
        Random.InitState((int)System.DateTime.Now.Ticks);
        float seed = Random.Range(0f, 1f);
        float ran = Mathf.Lerp(0, _thresholdLength, seed);
        info = _randomThresholdPoint + _thresholdDirection * ran;
        return info;
    }



}
