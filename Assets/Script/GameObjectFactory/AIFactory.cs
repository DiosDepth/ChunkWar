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

public enum RectangleBaseMode
{
    RowBase,
    ColumeBase,
}

public class AIFactory : MonoBehaviour
{

    public AvaliableAIType AIType = AvaliableAIType.AI_Flyings;
    public FactoryShape formMode = FactoryShape.Rectangle;

    //Rectangle settings
    public Vector2 sizeInterval = new Vector2(0.5f, 0.5f);
    public RectangleBaseMode baseMode = RectangleBaseMode.RowBase;
    public int totalCount;
    public int maxRowCount;
    public float spawnIntervalTime = 0.25f;




    private List<Vector2> _formposlist;
    private List<AIShip> _shiplist;
    private Coroutine _spawncorotine;

    private Vector2 _spawnreferencedir;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DataManager.Instance.LoadAllData( ()=> 
        {
            AIShipConfig config;
            config = DataManager.Instance.GetAIShipConfig((int)AIType);
            Debug.Log("ShipSize = " + config.GetShipSize());
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
                //Ë¢ÐÂµÐÈË
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

    public List<Vector2> CalculateFormPos()
    {
        
        List<Vector2> templist = new List<Vector2>() ;
        if(formMode == FactoryShape.Rectangle)
        {
            for (int i = 0; i < totalCount; i++)
            {

            }

            return templist;
        }

        return null;
    }


}
