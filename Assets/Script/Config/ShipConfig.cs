using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_Ship_", menuName = "Configs/Unit/ShipConfig")]
public class ShipConfig : BaseConfig, ISerializationCallbackReceiver
{

    public ShipType ShipType;
    public int[,] ShipMap = new int[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];

    [HideInInspector]
    [SerializeField]
    public int[] m_FlattendMapLayout;

    [HideInInspector]
    [SerializeField]
    public int m_FlattendMapLayoutRows;

    public void OnBeforeSerialize()
    {
        int c1 = ShipMap.GetLength(0);
        int c2 = ShipMap.GetLength(1);
        int count = c1 * c2;
        m_FlattendMapLayout = new int[count];
        m_FlattendMapLayoutRows = c1;
        for (int i = 0; i < count; i++)
        {
            m_FlattendMapLayout[i] = ShipMap[i / c1, i % c1];
        }
    }
    public void OnAfterDeserialize()
    {
        int count = m_FlattendMapLayout.Length;
        int c1 = m_FlattendMapLayoutRows;
        int c2 = count / c1;
        ShipMap = new int[c1, c2];
        for (int i = 0; i < count; i++)
        {
            ShipMap[i / c1, i % c1] = m_FlattendMapLayout[i];
        }
    }
}
