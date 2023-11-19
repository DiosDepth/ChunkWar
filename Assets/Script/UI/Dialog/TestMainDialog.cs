using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if GMDEBUG 
public class TestMainDialog : GUIBasePanel
{
    private TMP_Dropdown _plugDropDown;
    private TMP_Dropdown _shipDropDown;
    private TMP_Dropdown _hardLevelDropDown;
    private TMP_Dropdown _waveDropDown;

    private ShipPresetTestData ShipPreset;
    private ShipPlugPresetTestData ShipPlugPreset;
    private int hardLevelID;
    private int WaveIndex;

    protected override void Awake()
    {
        base.Awake();
        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(CloseDialog);
        GetGUIComponent<Button>("Start").onClick.AddListener(StartTest);
        _plugDropDown = transform.Find("Content/PlugPreset/Dropdown").SafeGetComponent<TMP_Dropdown>();
        _shipDropDown = transform.Find("Content/ShipPreset/Dropdown").SafeGetComponent<TMP_Dropdown>();
        _hardLevelDropDown = transform.Find("Content/StagePreset/HardLevel").SafeGetComponent<TMP_Dropdown>();
        _waveDropDown = transform.Find("Content/StagePreset/Wave").SafeGetComponent<TMP_Dropdown>();
    }


    public override void Initialization()
    {
        base.Initialization();
        SetUpDropdown();
    }

    private void SetUpDropdown()
    {
        ///Plug
        List<TMP_Dropdown.OptionData> plugOptions = new List<TMP_Dropdown.OptionData>();
        var allPlugs = TestDataManager.Instance.PlugPresetDatas;
        for(int i = 0; i < allPlugs.Count; i++)
        {
            var preset = allPlugs[i];
            var dropDownName = string.Format("[{0}]-{1}-{2}",preset.ID, preset.Name, preset.Comment);

            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData
            {
                text = dropDownName
            };
            plugOptions.Add(data);
        }

        ///SelectFirst
        _plugDropDown.options.Clear();
        _plugDropDown.AddOptions(plugOptions);
        _plugDropDown.onValueChanged.AddListener(OnPlugPresetSelect);
        OnPlugPresetSelect(0);

        ///Ship
        List<TMP_Dropdown.OptionData> shipOptions = new List<TMP_Dropdown.OptionData>();
        var allShips = TestDataManager.Instance.ShipPresetDatas;
        for (int i = 0; i < allShips.Count; i++)
        {
            var preset = allShips[i];
            var shipCfg = DataManager.Instance.GetShipConfig(preset.ShipID);
            var dropDownName = string.Format("[{0}]-{1}-{2}_{3}", preset.ID, preset.Name,
                shipCfg == null ? string.Empty : LocalizationManager.Instance.GetTextValue(shipCfg.GeneralConfig.Name), preset.Comment);

            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData
            {
                text = dropDownName
            };
            shipOptions.Add(data);
        }

        ///SelectFirst
        _shipDropDown.options.Clear();
        _shipDropDown.AddOptions(shipOptions);
        _shipDropDown.onValueChanged.AddListener(OnShipPresetSelect);
        OnShipPresetSelect(0);

        InitStageDropDown();
    }

    private void InitStageDropDown()
    {
        ///Ship
        List<TMP_Dropdown.OptionData> hardLevelOptions = new List<TMP_Dropdown.OptionData>();
        var allHardLevels = DataManager.Instance.battleCfg.HardLevels;
        for (int i = 0; i < allHardLevels.Count; i++)
        {
            var preset = allHardLevels[i];
            var dropDownName = string.Format("[{0}]-{1}", preset.HardLevelID, LocalizationManager.Instance.GetTextValue(preset.Name));

            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData
            {
                text = dropDownName
            };
            hardLevelOptions.Add(data);
        }

        ///SelectFirst
        _hardLevelDropDown.options.Clear();
        _hardLevelDropDown.AddOptions(hardLevelOptions);
        _hardLevelDropDown.onValueChanged.AddListener(OnHardLevelIDSelect);
        OnHardLevelIDSelect(0);
    }

    private void OnPlugPresetSelect(int id)
    {
        ShipPlugPreset = TestDataManager.Instance.PlugPresetDatas[id];
    }

    private void OnShipPresetSelect(int id)
    {
        ShipPreset = TestDataManager.Instance.ShipPresetDatas[id];
    }

    private void OnHardLevelIDSelect(int id)
    {
        var levelData = DataManager.Instance.battleCfg.HardLevels[id];
        hardLevelID = levelData.HardLevelID;
        var stageCfg = DataManager.Instance.GetLevelSpawnConfig(levelData.LevelPresetID);
        if (stageCfg == null)
            return;

        List<TMP_Dropdown.OptionData> waveOptions = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < stageCfg.WaveConfig.Count; i++)
        {
            var preset = stageCfg.WaveConfig[i];
            var dropDownName = preset.WaveIndex.ToString();

            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData
            {
                text = dropDownName
            };
            waveOptions.Add(data);
        }
        _waveDropDown.options.Clear();
        _waveDropDown.AddOptions(waveOptions);
        _waveDropDown.onValueChanged.AddListener(OnWaveCfgChange);
        OnWaveCfgChange(0);
    }

    private void OnWaveCfgChange(int id)
    {
        WaveIndex = id + 1;
    }

    private void CloseDialog()
    {
        UIManager.Instance.HiddenUI("TestMainDialog");
    }

    private void StartTest()
    {
        RogueManager.Instance.TryStartBattleTest(ShipPreset, ShipPlugPreset, hardLevelID, WaveIndex);
    }
}
#endif