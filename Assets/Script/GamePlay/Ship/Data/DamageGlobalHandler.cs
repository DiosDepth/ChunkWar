using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 处理全局伤害的显示
 */
public class DamageGlobalHandler 
{
    /// <summary>
    /// Key = attackerUID
    /// </summary>
    private Dictionary<uint, DamageDisplayData> displayData = new Dictionary<uint, DamageDisplayData>();

    /// <summary>
    /// 更新伤害显示
    /// </summary>
    public void OnUpdate()
    {
        foreach(var item in displayData.Values)
        {
            item.OnUpdate();
        }
    }

    /// <summary>
    /// 显示伤害
    /// </summary>
    /// <param name="targetWeapon"></param>
    /// <param name="targetUID"></param>
    /// <param name="damageValue"></param>
    /// <param name="hitPoint"></param>
    /// <param name="critical"></param>
    public void ShowDamage(Unit targetWeapon , uint targetUID, int damageValue, Vector2 hitPoint, bool critical)
    {
        if(targetWeapon._baseUnitConfig is WeaponConfig)
        {
            var weaponCfg = targetWeapon._baseUnitConfig as WeaponConfig;
            if(weaponCfg != null && weaponCfg.PersistentDamageDisplay)
            {
                ShowDamage(targetWeapon.UID, targetUID, damageValue, hitPoint, critical);
                return;
            }
        }

        ShowDamageSimple(damageValue, hitPoint, critical);
    }

    public void ShowPlayerTakeDamage(int damageValue, Vector2 hitPoint)
    {
        var rowScreenPos = UIManager.Instance.GetUIposBWorldPosition(hitPoint);
        UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Bot, this, (panel) =>
        {
            panel.Initialization();
            panel.SetPlayerTakeDamageText(Mathf.Abs(damageValue), rowScreenPos);
        });
    }

    /// <summary>
    /// 单次跳字
    /// </summary>
    /// <param name="damageValue"></param>
    /// <param name="hitPoint"></param>
    /// <param name="critical"></param>
    public void ShowDamageSimple(int damageValue, Vector2 hitPoint, bool critical)
    {
        var rowScreenPos = UIManager.Instance.GetUIposBWorldPosition(hitPoint);
        UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Bot, this, (panel) =>
        {
            panel.Initialization();
            panel.SetText(Mathf.Abs(damageValue), critical, rowScreenPos);
        });
    }

    public void ShowDamage(uint attackerUID, uint targetUID, int damageValue, Vector2 hitPoint, bool critical)
    {
        var displayData = GetOrCreateDisplayData(attackerUID);
        displayData.ShowDamage(targetUID, damageValue, hitPoint, critical);
    }

    /// <summary>
    /// 获取展示信息
    /// </summary>
    /// <param name="attackerUID"></param>
    /// <returns></returns>
    private DamageDisplayData GetOrCreateDisplayData(uint attackerUID)
    {
        if (displayData.ContainsKey(attackerUID))
            return displayData[attackerUID];

        DamageDisplayData newData = new DamageDisplayData();
        displayData.Add(attackerUID, newData);
        return newData;
    }
}

public class DamageDisplayData
{
    public Dictionary<uint, FloatingText> TextData = new Dictionary<uint, FloatingText>();

    private List<FloatingText> textDatas = new List<FloatingText>();

    public void OnUpdate()
    {
        for (int i = textDatas.Count - 1; i >= 0; i--) 
        {
            var data = textDatas[i];
            data.OnUpdate();
            if (data.IsNeedToRemove)
            {
                TextData.Remove(data.TargetUID);
                textDatas.RemoveAt(i);
                data.OnRemove();
            }
        }
    }

    public void ShowDamage(uint targetUID, int damageValue, Vector2 hitPoint, bool critical)
    {
        var rowScreenPos = UIManager.Instance.GetUIposBWorldPosition(hitPoint);
        var text = GetFloatingText(targetUID);
        if(text != null)
        {
            ///延长时长
            text.ProlongTextDuration(damageValue, critical, rowScreenPos);
        }
        else
        {
            UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Bot, this, (panel) =>
            {
                panel.Initialization();
                panel.SetText(Mathf.Abs(damageValue), critical, rowScreenPos, false);
                panel.TargetUID = targetUID;
                
                if (!TextData.ContainsKey(targetUID))
                {
                    textDatas.Add(panel);
                    TextData.Add(targetUID, panel);
                }
            });
        }
    }

    private FloatingText GetFloatingText(uint targetUID)
    {
        if (TextData.ContainsKey(targetUID))
            return TextData[targetUID];

        return null;
    }
}
