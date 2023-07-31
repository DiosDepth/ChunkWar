using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShip : MonoBehaviour
{


    public Chunk[,] ChunkMap { set { _chunkMap = value; } get { return _chunkMap; } }
    protected Chunk[,] _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];

    public List<Unit> UnitList { set { _unitList = value; } get { return _unitList; } }
    protected List<Unit> _unitList = new List<Unit>();

    public virtual void Initialization()
    {

    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
}
