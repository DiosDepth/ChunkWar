using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuButton : MonoBehaviour, IHoverUIItem
{
    public enum MainMenuButtonType
    {
        Play,
        Option,
        Quit,
        Load,
        Collection,
        Achievement,
        Camp,
        Continue,
        Restart,
        MainMenu
    }

    public MainMenuButtonType Type = MainMenuButtonType.Play;

    private Transform selectedTrans;
    private TextMeshProUGUI _text;

    private static string MainMenu_Play = "MainMenu_Play";
    private static string MainMenu_Option = "MainMenu_Option";
    private static string MainMenu_Quit = "MainMenu_Quit";
    private static string MainMenu_Load = "MainMenu_Load";
    private static string MainMenu_Collection = "MainMenu_Collection";
    private static string MainMenu_Achievement = "MainMenu_Achievement";
    private static string MainMenu_Camp = "MainMenu_Camp";
    private static string MainMenu_MainMenu = "MainMenu_MainMenu";
    private static string MainMenu_Restart = "MainMenu_Restart";
    private static string MainMenu_Continue = "MainMenu_Continue";


    private static Color SelectedColor = new Color32(88, 151, 250, 255);
    private static Color QuitColor = new Color32(194, 136, 136, 255);
    private static Color NormalColor = new Color32(72, 186, 187, 255);

    public void Awake()
    {
        selectedTrans = transform.Find("Hover");
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
        selectedTrans.SafeSetActive(false);
        transform.SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
        _text = transform.Find("Text").SafeGetComponent<TextMeshProUGUI>();
        _text.text = GetButtonText();

        if(Type == MainMenuButtonType.Quit)
        { 
            _text.color = QuitColor;
        }
        else
        {
            _text.color = NormalColor;
        }
    }

    public void OnHoverEnter()
    {
        selectedTrans.SafeSetActive(true);
        if(Type != MainMenuButtonType.Quit)
        {
            _text.color = SelectedColor;
        }
        
        LeanTween.moveLocalX(gameObject, -18, 0.1f);
        SoundManager.Instance.PlayUISound("ui_button_mouseover");
    }

    public void OnHoverExit()
    {
        selectedTrans.SafeSetActive(false);
        if (Type != MainMenuButtonType.Quit)
        {
            _text.color = NormalColor;
        }

        LeanTween.moveLocalX(gameObject, 0, 0.1f);
    }

    private string GetButtonText()
    {
        switch (Type)
        {
            case MainMenuButtonType.Achievement:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Achievement);
            case MainMenuButtonType.Camp:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Camp);
            case MainMenuButtonType.Collection:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Collection);
            case MainMenuButtonType.Load:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Load);
            case MainMenuButtonType.Option:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Option);
            case MainMenuButtonType.Play:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Play);
            case MainMenuButtonType.Quit:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Quit);
            case MainMenuButtonType.Continue:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Continue);
            case MainMenuButtonType.Restart:
                return LocalizationManager.Instance.GetTextValue(MainMenu_Restart);
            case MainMenuButtonType.MainMenu:
                return LocalizationManager.Instance.GetTextValue(MainMenu_MainMenu);
            default:
                return string.Empty;
        }
    }

    private void OnButtonClick()
    {
        if(Type == MainMenuButtonType.Play)
        {
            GameStateTransitionEvent.Trigger(EGameState.EGameState_ShipSelection);
            UIManager.Instance.HiddenUI("MainMenu");
        }
        else if (Type == MainMenuButtonType.Achievement)
        {
            UIManager.Instance.ShowUI<AchievementPage>("AchievementPage", E_UI_Layer.Mid, this, (panel) =>
            {
                panel.Initialization();
            });
            UIManager.Instance.HiddenUI("MainMenu");
        }
        else if (Type == MainMenuButtonType.Camp)
        {
            UIManager.Instance.ShowUI<CampSelectPage>("CampSelectPage", E_UI_Layer.Mid, this, (panel) =>
            {
                panel.Initialization();
            });
            UIManager.Instance.HiddenUI("MainMenu");
        }
        else if (Type == MainMenuButtonType.Collection)
        {
            UIManager.Instance.ShowUI<CollectionMainPage>("CollectionMainPage", E_UI_Layer.Mid, this, (panel) =>
            {
                panel.Initialization();
            });
            UIManager.Instance.HiddenUI("MainMenu");
        }
        else if (Type == MainMenuButtonType.Load)
        {
            UIManager.Instance.ShowUI<GameSavePage>("GameSavePage", E_UI_Layer.Mid, this, (panel) =>
            {
                panel.Initialization();
            });
            UIManager.Instance.HiddenUI("MainMenu");
        }
        else if (Type == MainMenuButtonType.Option)
        {
            UIManager.Instance.ShowUI<Options>("Options", E_UI_Layer.Mid, this, (panel) =>
            {
                panel.Initialization();
            });
        }
        else if (Type == MainMenuButtonType.Quit)
        {
            Application.Quit();
        }
        else if (Type == MainMenuButtonType.MainMenu)
        {
            UIManager.Instance.HiddenUI("Pause");
            GameManager.Instance.ClearBattle();

            GameStateTransitionEvent.Trigger(EGameState.EGameState_MainMenu);
        }
        else if (Type == MainMenuButtonType.Restart)
        {

        }
        else if (Type == MainMenuButtonType.Continue)
        {
            UIManager.Instance.HiddenUI("Pause");
            UIManager.Instance.ShowUI<ShipHUD>("ShipHUD", E_UI_Layer.Mid, null, (panel) =>
            {
                panel.Initialization();
            });
            InputDispatcher.Instance.ChangeInputMode("Player");
            GameManager.Instance.UnPauseGame();
        }

        SoundManager.Instance.PlayUISound("ui_button_pressed");
    }
}
