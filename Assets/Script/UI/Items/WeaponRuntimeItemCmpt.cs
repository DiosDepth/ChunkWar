using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponRuntimeItemCmpt : MonoBehaviour, IPoolable
{
    public Weapon _targetWeapon;

    private Image _hpImage;
    private Image _cdImage;
    private TextMeshProUGUI _cdText;
    private TextMeshProUGUI _armorText;

    private List<ArmorItemCmpt> armorCmpts = new List<ArmorItemCmpt>();

    private static string ArmorItem_Prefab = "Prefab/GUIPrefab/CmptItems/ArmorItem";

    public void Awake()
    {
        _hpImage = transform.Find("HPInfo/HPFill").SafeGetComponent<Image>();
        _cdImage = transform.Find("Icon/CD").SafeGetComponent<Image>();
        _cdText = transform.Find("Icon/CD/Value").SafeGetComponent<TextMeshProUGUI>();
        _armorText = transform.Find("Info/ArmorInfo/Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUp(Weapon weapon)
    {
        _cdImage.transform.SafeSetActive(false);
        this._targetWeapon = weapon;
        if (weapon == null)
            return;

        var generalCfg = _targetWeapon._baseUnitConfig.GeneralConfig;
        transform.Find("Icon/Image").SafeGetComponent<Image>().sprite = generalCfg.IconSprite;
        var nameCmpt = transform.Find("Info/Name").SafeGetComponent<TextMeshProUGUI>();
        nameCmpt.text = LocalizationManager.Instance.GetTextValue(generalCfg.Name);
        nameCmpt.color = GameHelper.GetRarityColor(generalCfg.Rarity);
        InitMagazine();

        weapon.HpComponent.BindHPChangeAction(UpdateWeaponHP, true);
        weapon.OnReloadCDUpdate += UpdateWeaponCD;
        weapon.OnMagazineChange += OnUpdateArmor;

    }

    public void StartReloadCDTimer()
    {
        _cdImage.transform.SafeSetActive(true);
    }

    public void EndReloadCDTimer()
    {
        _cdImage.transform.SafeSetActive(false);
        armorCmpts.ForEach(x => x.SetActive(true));
        _armorText.text = _targetWeapon.magazine.ToString();
    }


    public void PoolableDestroy()
    {
        _targetWeapon.HpComponent.UnBindHPChangeAction(UpdateWeaponHP);
        _targetWeapon.OnReloadCDUpdate -= UpdateWeaponCD;
        _targetWeapon.OnMagazineChange -= OnUpdateArmor;
    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {

    }

    private void InitMagazine()
    {
        var root = transform.Find("Info/ArmorInfo/Content");

        var magazine = _targetWeapon.magazine;
        _armorText.text = magazine.ToString();
        for (int i = 0; i < magazine; i++) 
        {
            PoolManager.Instance.GetObjectSync(ArmorItem_Prefab, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<ArmorItemCmpt>();
                cmpt.SetActive(true);
                armorCmpts.Add(cmpt);
            }, root);
        }
    }

    private void UpdateWeaponHP()
    {
        var hpRatio = _targetWeapon.HpComponent.HPPercent;
        _hpImage.fillAmount = hpRatio / 100f;
    }

    private void UpdateWeaponCD(float cd)
    {
        _cdImage.fillAmount = _targetWeapon.ReloadTimePercent;
        _cdText.text = string.Format("{0:F1}", cd);
    }

    private void OnUpdateArmor(int magazine)
    {
        _armorText.text = magazine.ToString();
        for (int i = armorCmpts.Count - 1; i >= 0; i--) 
        {
            armorCmpts[i].SetActive(i < magazine);
        }
    }
}
