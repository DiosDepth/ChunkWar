using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sim_FrameWork
{
    [RequireComponent(typeof(Text))]
    public class GetTextByID : MonoBehaviour
    {
        public string Textid;
        void Start()
        {
            InitText();
        }

        void InitText()
        {
            if (!string.IsNullOrEmpty(Textid))
            {
                GetComponent<Text>().text = LocalizationManager.Instance.GetTextValue(Textid);
            }
            else
            {
                GetComponent<Text>().text = "Error!+" + Textid;
            }
        }
    }
}