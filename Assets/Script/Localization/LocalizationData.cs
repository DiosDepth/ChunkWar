using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sim_FrameWork.Config
{
    [System.Serializable]
    public class LocalizationData 
    {

        public Dictionary<string, string> LanguageDic;

        public LocalizationData()
        {
            LanguageDic = new Dictionary<string, string>();
        }
    }
}