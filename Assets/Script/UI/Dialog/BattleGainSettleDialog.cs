using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleGainSettleDialog : GUIBasePanel
{
    private Transform _contentTrans;


    private static float _protectTime = 2.5f;
    private bool _canClick = false;
    private bool _isShowing = false;

    private List<WreckageItemInfo> allWreckages;
    private List<GeneralPreviewItemSlot> _slotItems = new List<GeneralPreviewItemSlot>();

    private const string SlotPrefabPath = "Prefab/GUIPrefab/Weiget/GeneralPreviewItemSlot";

    protected override void Awake()
    {
        base.Awake();
        _contentTrans = transform.Find("Content/Info/Content/Viewport/Content");
        GetGUIComponent<Button>("BGMain").onClick.AddListener(Click);
        GetGUIComponent<Button>("BG").onClick.AddListener(Click);
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        allWreckages = (List<WreckageItemInfo>)param[0];
        _canClick = false;
        SetUp();
        Wait();
    }


    private void SetUp()
    {
        if (allWreckages == null || allWreckages.Count <= 0)
            return;

        for(int i = 0; i < allWreckages.Count; i++)
        {
            var wreckageInfo = allWreckages[i];

            PoolManager.Instance.GetObjectSync(SlotPrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<GeneralPreviewItemSlot>();
                cmpt.SetUp(GoodsItemType.ShipUnit, wreckageInfo.UnitID);
                cmpt.SetHide();
                _slotItems.Add(cmpt);
            }, _contentTrans);
        }

        DoShow();
    }

    private async void DoShow()
    {
        _isShowing = true;
        await UniTask.Delay(500);
        SoundManager.Instance.PlayUISound("BattleGainSettle");
        for (int i = 0; i < _slotItems.Count; i++) 
        {
            SoundManager.Instance.PlayUISound("BattleGain_ItemShow");
            await _slotItems[i].DoShow();
        }
        await UniTask.Delay(500);
        _isShowing = false;
    }

    private async void Wait()
    {
        await UniTask.Delay((int)(_protectTime * 1000));
        _canClick = true;
    }

    void Click()
    {
        if (!_canClick)
            return;

        if (_isShowing)
        {
            for (int i = 0; i < _slotItems.Count; i++)
            {
                _slotItems[i].DoShow();
            }
            _isShowing = false;
        }
        else
        {
            UIManager.Instance.HiddenUI("BattleGainSettleDialog");
        }
        
    }
}
