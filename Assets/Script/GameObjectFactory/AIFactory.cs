using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum FactoryShape
{
    Rectangle,
    Circle,
    Line,
    Arch,
}


public class AIFactory : MonoBehaviour
{

    public AvaliableAIType AIType = AvaliableAIType.AI_Flyings;
    public FactoryShape formMode = FactoryShape.Rectangle;

    //Rectangle settings
    public Vector2 sizeInterval = new Vector2(0.5f, 0.5f);
    public int totalCount;
    public int maxRowCount;
    public float spawnIntervalTime = 0.25f;
    public Transform tempTarget;



    private Vector2Int _rectanglematirx;
    private List<Vector2> _formposlist;
    private List<AIShip> _shiplist;
    private Coroutine _spawncorotine;

    private Vector2 _spawnreferencedir;
    private AIShipConfig _shipconfig;

    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DataManager.Instance.LoadAllData( ()=> 
        {

            _shipconfig = DataManager.Instance.GetAIShipConfig((int)AIType);
            Debug.Log("ShipSize = " + _shipconfig.GetShipSize());

            _rectanglematirx = GetRectangleMatrix();
            Debug.Log("RectangleMatrix = " + GetRectangleMatrix());
            _formposlist = CalculateFormPos();
        }));

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartSpawn()
    {
       _spawncorotine = StartCoroutine("Spawn");

    }

    public IEnumerator Spawn()
    {
        float stamp = 0;
        while(true)
        {
            if (Time.time >= stamp)
            {
                //刷新敌人
            }
            yield return null;
        }
    }
    public void StopSpawn()
    {
        StopCoroutine("Spawn");
    }


    public Vector2 GetSpawnReferenceDir()
    {
        _spawnreferencedir = this.transform.position.DirectionToXY(RogueManager.Instance.currentShip.transform.position).ToVector2();
        return _spawnreferencedir;
    }

    public virtual Vector2Int GetRectangleMatrix()
    {
        Vector2Int tempmatrix = Vector2Int.zero;
        tempmatrix.x = maxRowCount;
        tempmatrix.y = Mathf.CeilToInt( (float)totalCount / (float)maxRowCount);
        return tempmatrix;
    }

    public List<Vector2> CalculateFormPos()
    {
        Vector2 interval = new Vector2(sizeInterval.x + _shipconfig.GetShipSize().x, sizeInterval.y + _shipconfig.GetShipSize().y);
        Vector2 offset = Vector2.zero;
        if (_rectanglematirx.x.IsEven())
        {
            offset.x = interval.x * 0.5f;
        }
        if(_rectanglematirx.y.IsEven())
        {
            offset.y = interval.y * -0.5f;
        }

        Vector2Int posscaler = Vector2Int.zero;
        List<Vector2> templist = new List<Vector2>() ;
        //按照方向创建矩阵
        Vector3 dir = this.transform.position.DirectionToXY(tempTarget.position);
        Matrix4x4 matrix4X4 = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.forward, dir));
        Vector3 pos;

        if (formMode == FactoryShape.Rectangle)
        {
            //计算原始坐标
            if (_rectanglematirx.x==0 || _rectanglematirx.y ==0)
            {
                Debug.LogError(this.gameObject.name + " private _rectanglematirx is 0");
            }
            for (int y = 0;  y <_rectanglematirx.y; y++)
            {
                for (int x = 0; x < _rectanglematirx.x; x++)
                {
                    posscaler = new Vector2Int(x - Mathf.FloorToInt(_rectanglematirx.x * 0.5f), Mathf.FloorToInt(_rectanglematirx.y * 0.5f) - y);

                    pos = new Vector2(posscaler.x * interval.x, posscaler.y * interval.y) + offset;
                    //左乘矩阵
                    pos = matrix4X4 * pos;
                    templist.Add(pos);
                    if(templist.Count == totalCount)
                    {
                        break;
                    }
                }
            }
          




            return templist;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if(_formposlist?.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < _formposlist.Count; i++)
        {
            
            Gizmos.color = Color.green;
            Gizmos.DrawCube(_formposlist[i], _shipconfig.GetShipSize());
            
        }
    }
}
