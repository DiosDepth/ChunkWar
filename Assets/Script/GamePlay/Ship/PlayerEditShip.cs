using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEditShip : PlayerShip
{

    public override void Initialization()
    {
        var shipCfg = RogueManager.Instance.currentShipSelection.itemconfig as PlayerShipConfig;
        playerShipCfg = shipCfg;
        baseShipCfg = shipCfg;
    }

    public override void CreateShip()
    {

        //初始化
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        //初始化Chunk
        for (int row = 0; row < ShipMapInfo.GetLength(0); row++)
        {
            for (int colume = 0; colume < ShipMapInfo.GetLength(1); colume++)
            {
                if (ShipMapInfo[row, colume] == null)
                {
                    continue;
                }

                if (ShipMapInfo[row, colume].type == ChunkType.Core)
                {

                    core = new Core();
                    _chunkMap[row, colume] = core;
                }

                if (ShipMapInfo[row, colume].type == ChunkType.Base)
                {

                    _chunkMap[row, colume] = new Base();
                }

                _chunkMap[row, colume].shipCoord = ShipMapInfo[row, colume].shipCoord;
                _chunkMap[row, colume].state = ShipMapInfo[row, colume].state;
                _chunkMap[row, colume].isBuildingPiovt = ShipMapInfo[row, colume].isBuildingPiovt;
                _chunkMap[row, colume].isOccupied = ShipMapInfo[row, colume].isOccupied;

            }
        }
        //初始化Building
        for (int i = 0; i < UnitInfoList.Count; i++)
        {
            RestoreEditorUnitFromUnitInfo(UnitInfoList[i]);
        }
        //初始化主武器
        if (mainWeapon == null)
        {
            var weaponID = playerShipCfg.MainWeaponID;
            BaseUnitConfig weaponconfig;
            DataManager.Instance.UnitConfigDataDic.TryGetValue(weaponID, out weaponconfig);
            Vector2Int[] _reletivemap = weaponconfig.GetReletiveCoord().AddToAll(core.shipCoord);
            mainWeapon = AddEditUnit(weaponconfig, _reletivemap, core.shipCoord, 0) as ShipWeapon;
        }
    }

    public Unit AddEditUnit(BaseConfig m_unitconfig, Vector2Int[] m_unitmap, Vector2Int m_poscoord, int m_direction, bool isEditorMode = false)
    {
        GameObject obj;
        Vector2Int buildarray;
        obj = Instantiate(m_unitconfig.Prefab);
        obj.transform.parent = buildingsParent.transform;
        obj.transform.localPosition = new Vector3(m_poscoord.x + shipMapCenter.localPosition.x, m_poscoord.y + shipMapCenter.localPosition.y);
        Unit tempunit = obj.GetComponent<Unit>();
        tempunit.direction = m_direction;
        tempunit.UnitID = m_unitconfig.ID;
        tempunit.InitializationEditorUnit(this, m_unitconfig as BaseUnitConfig);

        obj.transform.rotation = Quaternion.Euler(0, 0, -90 * tempunit.direction);
        if (tempunit is Weapon)
        {
            ///商店中建造模式，不更新武器
            var weapon = tempunit as Weapon;
            weapon.SetUnitProcess(false);
        }
        //设置对应的 ChunMap信息，比如是否为Piovt， 是否被占用等。
        if (m_unitmap.Length > 0)
        {
            for (int i = 0; i < m_unitmap.Length; i++)
            {
                buildarray = GameHelper.CoordinateMapToArray(m_unitmap[i], GameGlobalConfig.ShipMapSize);


                ChunkMap[buildarray.x, buildarray.y].unit = tempunit;

                if (m_unitmap[i] == m_poscoord)
                {
                    ChunkMap[buildarray.x, buildarray.y].isBuildingPiovt = true;

                    tempunit.pivot = m_unitmap[i];
                }
                ChunkMap[buildarray.x, buildarray.y].isOccupied = true;

                tempunit.occupiedCoords.Add(m_unitmap[i]);
            }
        }

        _unitList.Add(tempunit);
        RefreshShipEnergy();
        return tempunit;
    }

    private void RestoreEditorUnitFromUnitInfo(UnitInfo m_unitInfo)
    {
        BaseUnitConfig unitconfig;
        Vector2Int occupiedarray;
        GameObject obj;
        Unit tempunit;

        DataManager.Instance.UnitConfigDataDic.TryGetValue(m_unitInfo.UnitID, out unitconfig);
        obj = Instantiate(unitconfig.Prefab);


        if (obj != null)
        {
            tempunit = obj.GetComponent<Unit>();

            if (tempunit != null)
            {
                tempunit.UnitID = m_unitInfo.UnitID;
                tempunit.UID = m_unitInfo.UID;
                tempunit.direction = m_unitInfo.direction;
                tempunit.pivot = m_unitInfo.pivot;
                tempunit.occupiedCoords = m_unitInfo.occupiedCoords;
                tempunit.state = DamagableState.Normal;

                for (int n = 0; n < tempunit.occupiedCoords.Count; n++)
                {
                    occupiedarray = GameHelper.CoordinateMapToArray(tempunit.occupiedCoords[n], GameGlobalConfig.ShipMapSize);
                    _chunkMap[occupiedarray.x, occupiedarray.y].unit = tempunit;
                }


                if (unitconfig.unitType == UnitType.MainWeapons)
                {
                    mainWeapon = tempunit as ShipWeapon;
                    mainWeapon.InitializationEditorUnit(this, unitconfig);
                }
                else
                {
                    tempunit.InitializationEditorUnit(this, unitconfig);
                }

                obj.transform.parent = buildingsParent.transform;
                obj.transform.localPosition = new Vector3(tempunit.pivot.x + shipMapCenter.localPosition.x, tempunit.pivot.y + shipMapCenter.localPosition.y, 0);
                obj.transform.rotation = Quaternion.Euler(0, 0, -90 * tempunit.direction);

                _unitList.Add(tempunit);
            }
        }
    }
}
