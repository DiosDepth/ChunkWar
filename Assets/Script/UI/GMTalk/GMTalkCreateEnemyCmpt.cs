using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMTalkCreateEnemyCmpt : MonoBehaviour
{
    private InputField _input;
    private Dropdown _enemyDropDown;
    private Dropdown _hardLevelDropDown;

    private int _currentShipID;

    private void Awake()
    {
        _enemyDropDown = transform.Find("Content/Input/Enemy").SafeGetComponent<Dropdown>();
        _hardLevelDropDown = transform.Find("Content/Input/HardLevel").SafeGetComponent<Dropdown>();
        _input = transform.Find("Content/Input/Count").SafeGetComponent<InputField>();
        transform.Find("Content/Create").SafeGetComponent<Button>().onClick.AddListener(OnCreateButtonClick);
    }

    private void Start()
    {
        _input.text = "1";
        InitData();
    }

    private void InitData()
    {
        List<Dropdown.OptionData> enemyOptions = new List<Dropdown.OptionData>();
        var allEnemy = DataManager.Instance.GetAllAIShips();
        for(int i = 0; i < allEnemy.Count; i++)
        {
            var enemy = allEnemy[i];
            var enemyName = LocalizationManager.Instance.GetTextValue(enemy.GeneralConfig.Name);
            var dropDownName = string.Format("[{0}] {1}", enemy.ID, enemyName);

            Dropdown.OptionData data = new Dropdown.OptionData
            {
                text = dropDownName
            };
            enemyOptions.Add(data);
        }

        ///SelectFirst
        _enemyDropDown.options.Clear();
        _enemyDropDown.AddOptions(enemyOptions);
        _enemyDropDown.onValueChanged.AddListener(OnEnemySelectChange);
        OnEnemySelectChange(0);
    }

    private void OnEnemySelectChange(int dt)
    {
        _hardLevelDropDown.ClearOptions();
        var index = _enemyDropDown.value;
        
        var allEnemy = DataManager.Instance.GetAllAIShips();
        var targetEnemy = allEnemy[index];
        _currentShipID = targetEnemy.ID;
        var hardLevelGruopID = targetEnemy.HardLevelGroupID;
        var hardLevels = DataManager.Instance.GetEnemyHardLevelData(hardLevelGruopID);
        if (hardLevels == null || hardLevels.Items.Count <= 0)
            return;

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < hardLevels.Items.Count; i++)
        {
            var item = hardLevels.Items[i];

            Dropdown.OptionData data = new Dropdown.OptionData
            {
                text = item.ID.ToString()
            };
            options.Add(data);
        }
        _hardLevelDropDown.AddOptions(options);
    }

    private void OnCreateButtonClick()
    {
        var index = _hardLevelDropDown.value;
        int.TryParse(_hardLevelDropDown.options[index].text, out int currentHardLevelID);
        if (_currentShipID == 0 || currentHardLevelID == 0)
            return;

        int count = int.Parse(_input.text);
        count = Mathf.Clamp(count, 1, 10);

        GMTalkManager.Instance.CreateEnemy(_currentShipID, currentHardLevelID, count);
        GMTalkManager.Instance.CloseGMTalkPage();
    }
}
