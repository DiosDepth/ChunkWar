using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if GMDEBUG
public class GMTalkSaveCmpt : MonoBehaviour
{
    private Dropdown _saveDropDown;

    private List<SaveData> allSaves = new List<SaveData>();

    public void Awake()
    {
        _saveDropDown = transform.Find("SaveDropDown").SafeGetComponent<Dropdown>();
        transform.Find("LoadGame").SafeGetComponent<Button>().onClick.AddListener(OnLoadSaveClick);
    }

    private void Start()
    {
        InitSaves();
    }

    private void InitSaves()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        allSaves = SaveLoadManager.Instance.LoadAllGMSaveData();
        for(int i = 0; i < allSaves.Count; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData()
            {
                text = allSaves[i].SaveName
            };
            options.Add(data);
        }

        _saveDropDown.ClearOptions();
        _saveDropDown.AddOptions(options);
    }

    private void OnLoadSaveClick()
    {
        var value = _saveDropDown.value;
        if (value < 0)
            return;

        var savData = allSaves[value];
        RogueManager.Instance.LoadSaveData(savData);
    }
}
#endif