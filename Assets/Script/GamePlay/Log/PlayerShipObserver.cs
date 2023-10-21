using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GM_Observer
{
#if GMDEBUG

    public class UnitLocalPropertyObserverData
    {
        public class LocalData
        {
            public uint Key;
            public float Value;
        }

        public UnitPropertyModifyKey Type;
        public List<LocalData> NormalModify = new List<LocalData>();
        public List<LocalData> SlotModify = new List<LocalData>();
    }

    [System.Serializable]
    public class UnitObserverData
    {
        [HorizontalGroup("AAA",200)]
        [LabelText("ID")]
        [LabelWidth(60)]
        public int UnitID;

        [HorizontalGroup("AAA", 200)]
        [LabelText("Ãû³Æ")]
        [LabelWidth(60)]
        public string UnitName;

        public Dictionary<UnitPropertyModifyKey, UnitLocalPropertyObserverData> LocalPropertyDic = new Dictionary<UnitPropertyModifyKey, UnitLocalPropertyObserverData>();

    }

    public class PlayerShipObserver : MonoBehaviour
    {
        [ListDrawerSettings]
        [ReadOnly]
        public List<UnitObserverData> Data = new List<UnitObserverData>();

        private PlayerShip parentShip;

        private void Start()
        {
            parentShip = transform.SafeGetComponent<PlayerShip>();
        }

        [Button("Refresh")]
        [OnInspectorInit]
        private void OnRefresh()
        {
            Data.Clear();
            var targetUnits = parentShip.UnitList;
            for(int i = 0; i < targetUnits.Count; i++)
            {
                var target = targetUnits[i];
                UnitObserverData data = new UnitObserverData();
                data.UnitID = target.UnitID;
                data.UnitName = LocalizationManager.Instance.GetTextValue(target._baseUnitConfig.GeneralConfig.Name);
                data.LocalPropertyDic = RefreshLocalProeprtyData(target);

                Data.Add(data);
            }

        }

        private Dictionary<UnitPropertyModifyKey, UnitLocalPropertyObserverData> RefreshLocalProeprtyData(Unit target)
        {
            Dictionary<UnitPropertyModifyKey, UnitLocalPropertyObserverData> localPropertyDic = new Dictionary<UnitPropertyModifyKey, UnitLocalPropertyObserverData>();

            var localData = target.LocalPropetyData.GetAllPools();
            for (int i = 0; i < localData.Count; i++)
            {
                var newData = localData[i].CreateData();
                localPropertyDic.Add(newData.Type, newData);
            }
            return localPropertyDic;
        }

    }
#endif
}