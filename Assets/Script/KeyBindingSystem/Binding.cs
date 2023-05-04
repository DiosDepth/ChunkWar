using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class BindingInfo
{
    public bool isComposite;
    public int index;
    public int compositeStartIndex;
    public int compositeEndIndex;
    public int compositeCount;


    public BindingInfo(bool m_isComposite = false,int m_index = -1, int m_compositeStartIndex = -1, int m_compositeEndIndex = -1, int m_compositeCount = 0)
    {
        isComposite = m_isComposite;
        index = m_index;
        compositeStartIndex = m_compositeStartIndex;
        compositeEndIndex = m_compositeEndIndex;
        compositeCount = m_compositeCount;
    }
}


public class Binding : MonoBehaviour
{
    public KeyBinding keyBinding;
    public Text text;
    public Text buttonName;
    public string actionMapName;
    public string actionName;
    [SerializeField]
    public InputAction targetAction;
    public InputActionRebindingExtensions.RebindingOperation rebindingOperation;


    public int bindingIndex = -1;
    public string bindingid;

    [SerializeField]
    private BindingInfo _bindinginfo;

    // Start is called before the first frame update

    public void Initialization()
    {
        keyBinding = GetComponentInParent<KeyBinding>();
        targetAction = KeyBindingManager.Instance.inputAsset.FindActionMap(actionMapName).FindAction(actionName);
        text.text = actionName;
        RefreshBindinginfo(keyBinding?.Scheme);
      

    }
        void Start()      
    {

        Initialization();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void StartInteractiveRebinding()
    {
        if(!_bindinginfo.isComposite)
        {
            PerformInteractiveRebind(targetAction, _bindinginfo.index);
        }
        else
        {
            PerformInteractiveRebind(targetAction, _bindinginfo.compositeStartIndex,true);
        }

    }

    private void PerformInteractiveRebind(InputAction action, int index, bool allCompositeParts = false)
    {
        if (action == null)
        {
            return;
        }

        //确定Overlay显示的文字
        string overlaytext = "press " + action.bindings[index].name + " Key";
        keyBinding.ShowOverLay(overlaytext);
        
        
        //设置并且引用一个RebindingOperation
        rebindingOperation = action.PerformInteractiveRebinding(index);

        //设置回调
        rebindingOperation
            .OnCancel((operation) =>
            {
                keyBinding.HiddenOverLay();
            })
            .OnComplete((operation) =>
            {
                Debug.Log("after path = " + action.bindings[index].path);
                Debug.Log("after override path = " + action.bindings[index].overridePath);

                keyBinding.HiddenOverLay();
                UpdateButtonName();

                //如果这个属于一个composite
                if(allCompositeParts)
                {
                    var nextBindingIndex = index + 1;
                    if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                        PerformInteractiveRebind(action, nextBindingIndex, true);
                }

            });

        Debug.Log("before path = " + action.bindings[index].path);
        Debug.Log("before override path = " + action.bindings[index].overridePath);
        rebindingOperation.Start();//开启一个交互的rebinding过程，会修改Overriderpath，

    }


    public void RefreshBindinginfo(string scheme)
    {
        _bindinginfo = GetBindingInfoByScheme(scheme);
        UpdateButtonName();
    }
    /// <summary>
    /// 根据Scheme来获取当前绑定的信息，一个绑定对应一个按键
    /// </summary>
    /// <param name="scheme"></param>
    /// <returns></returns>
    private BindingInfo GetBindingInfoByScheme( string scheme)
    {
        BindingInfo info = new BindingInfo() ;
        bool isfindComposite = false;
        for (int i = 0; i < targetAction.bindings.Count; i++)
        {
            //Debug.Log("Binding name = " + targetAction.bindings[i].effectivePath + " | " + "isComposite = " + targetAction.bindings[i].isComposite + " | " + "isPartComposite = " + targetAction.bindings[i].isPartOfComposite);
            //如果不是组合按钮，并且也不是组合按钮的一部分
            if (!targetAction.bindings[i].isComposite && !targetAction.bindings[i].isPartOfComposite)
            {
                if(targetAction.bindings[i].groups == scheme)
                {
                    info.index = i;
                    info.isComposite = false;
                }
            }


            if (targetAction.bindings[i].isComposite && !targetAction.bindings[i].isPartOfComposite)
            {
                if (!isfindComposite)
                {
                    if (targetAction.bindings[i + 1].groups == scheme)
                    { 
                        info.index = i;
                        info.compositeStartIndex = info.index + 1;
                        info.isComposite = true;
                        isfindComposite = true;
                    }
                }
            }

            if (!targetAction.bindings[i].isComposite && targetAction.bindings[i].isPartOfComposite)
            {
                if (targetAction.bindings[i].groups == scheme)
                {
                    info.compositeCount++;
                }
            }


        }

        if(info.isComposite)
        {
            info.compositeEndIndex = info.index + info.compositeCount;
        }
        //todo 

        return info;
    }



    private void UpdateButtonName()
    {
        InputBinding.DisplayStringOptions options;
        options = InputBinding.DisplayStringOptions.DontIncludeInteractions;
        if(!_bindinginfo.isComposite)
        {
            
            var text = targetAction.GetBindingDisplayString(_bindinginfo.index, options);
            buttonName.text = text;
        }
        else
        {
            var text = "";
            for (int i = _bindinginfo.compositeStartIndex; i <= _bindinginfo.compositeEndIndex; i++)
            {
                text += targetAction.GetBindingDisplayString(i, options);
            }
            buttonName.text = text;
        }
    
    }
}