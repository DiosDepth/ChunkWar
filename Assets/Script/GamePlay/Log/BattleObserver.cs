using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GM_Observer
{
#if GMDEBUG
    public class BattleObserver : MonoBehaviour
    {
        [ReadOnly]
        public List<UnitProperty_ObserverData> PropertyDatas = new List<UnitProperty_ObserverData>();

        public int WaveHardLevelIndex;

        private RogueManager _rogueMgr;

        [Button("ˢ��")]
        [OnInspectorInit]
        public void Refresh()
        {
            _rogueMgr = RogueManager.Instance;
            RefreshMainProperty();
            WaveHardLevelIndex = _rogueMgr.GetHardLevelValueIndex;
        }

        private void RefreshMainProperty()
        {
            PropertyDatas.Clear();
            var mainProperty = RogueManager.Instance.MainPropertyData.GetAllPool();
            for (int i = 0; i < mainProperty.Count; i++) 
            {
                var pool = mainProperty[i];
                UnitProperty_ObserverData data = new UnitProperty_ObserverData(pool);
                PropertyDatas.Add(data);
            }
        }
    }

    [System.Serializable]
    public class UnitProperty_ObserverData
    {
        [LabelText("Key")]
        [LabelWidth(80)]
        [HorizontalGroup("AA", 300)]
        public PropertyModifyKey Key;

        [LabelText("����ֵ")]
        [LabelWidth(80)]
        [HorizontalGroup("AA", 200)]
        public float Value;



        public UnitProperty_ObserverData(UnitPropertyPool pool)
        {
            Key = pool.PropertyKey;
            Value = pool.GetFinialValue();
        }
    }

    [System.Serializable]
    public class UPOD_Table
    {
        public uint FromUID;
        public float Value;

        public UPOD_Table(uint fromUID, float value)
        {
            FromUID = fromUID;
            Value = value;
        }

        private string GetName()
        {
            return string.Empty;
        }
    }


#endif


}

