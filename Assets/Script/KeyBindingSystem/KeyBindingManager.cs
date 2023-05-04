using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[System.Serializable]
public class KeyMap
{
    public string keyBindingPath;
    public string keyName;


    public KeyMap(string path)
    {
        keyBindingPath = path;
        keyName = BindingPathToName(keyBindingPath);
    }
    public string BindingPathToName(string path)
    {
        string[] str = path.Split('/');
        return  str[1];
    }
}

[System.Serializable]
public class KeyMapConfig
{

    public Dictionary<Guid, string> keyBindingMap;

    public KeyMapConfig()
    {

        keyBindingMap = new Dictionary<Guid, string>();
    }
}

public class KeyBindingManager : Singleton<KeyBindingManager>
{
    public PlayerInput playerInput;
    public InputController inputController;
    public InputActionAsset inputAsset;
    public KeyMapConfig keyBindingConfig;

    public KeyBindingManager()
    {
        Initialization();
    }
    public override void Initialization()
    {
        base.Initialization();
        inputController = new InputController();
        inputAsset = inputController.asset;
        keyBindingConfig = new KeyMapConfig();
        LoadBinding();
        Debug.Log("KeyBindingManager initialization completed!");

    }

    public void LoadBinding()
    {
        LoadBinding(inputAsset);
    }

    public void LoadBinding(InputActionAsset targetmap)
    {
        keyBindingConfig = (KeyMapConfig)SaveLoadManager.Load("keyBindingConfig");
        if (keyBindingConfig == null)
        {
            
            return;
        }
        ReadOnlyArray<InputBinding> bindings;

        foreach(InputActionMap map in targetmap.actionMaps)
        {
            bindings = map.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                if (keyBindingConfig.keyBindingMap.TryGetValue(bindings[i].id, out var path))
                    map.ApplyBindingOverride(i, new InputBinding { overridePath = path });
            }
        }


    }

    public void SaveBinding()
    {
        SaveBinding(inputAsset);
    }

    public void SaveBinding(InputActionAsset sourcemap)
    {
        foreach( InputActionMap map in sourcemap.actionMaps)
        {
            foreach(InputBinding binding in map.bindings)
            {
                if(!string.IsNullOrEmpty(binding.overridePath))
                {
                    keyBindingConfig.keyBindingMap[binding.id] = binding.overridePath;
                }
            }
        }

        SaveLoadManager.Save(keyBindingConfig, "keyBindingConfig");
    }

    public void ResetKeyBindings()
    {
        LoadBinding();
    }
}
