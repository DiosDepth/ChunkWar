using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Configs_Unit_", menuName = "Configs/Unit/UnitConfig")]
public class BaseUnitConfig : BaseConfig
{
    [LabelText("����")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public UnitType unitType;

    [LabelText("����ת")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public bool redirection = true;


    public Vector2Int[] GetReletiveCoordByCenter(Vector2Int centercoord)
    {
        Vector2Int coord;
        List<Vector2Int> reletiveCoord = new List<Vector2Int>();
        reletiveCoord.Add(centercoord);
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 2)
                {
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.BuildingMapSize);
                    coord = coord - MapPivot;
                    coord += centercoord;
                    reletiveCoord.Add(coord);
                }
            }
        }
        return reletiveCoord.ToArray();
    }

    public Vector2Int[] GetReletiveCoord()
    {
        Vector2Int coord;
        List<Vector2Int> reletiveCoord = new List<Vector2Int>();
        reletiveCoord.Add(MapPivot);
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 2)
                {
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.BuildingMapSize);
                    coord = coord - MapPivot;

                    reletiveCoord.Add(coord);
                }
            }
        }
        return reletiveCoord.ToArray();
    }
}
