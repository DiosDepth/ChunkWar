using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : GUIBasePanel
{
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Initialization()
    {
        base.Initialization();
        

    }

    public void ChangeScore(int score)
    {
        GetGUIComponent<TMP_Text>("Score").text = score.ToString();
    }


}
