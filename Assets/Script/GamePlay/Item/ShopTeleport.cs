using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTeleport : TriggerOptionItem
{
    private bool _hasEnterShop = false;

    private const string BattleOption_EnterShop = "BattleOption_EnterShop";
    private SpriteRenderer m_sprite;
    protected Material _sharedMat;
    protected static Material _appearMat;
    protected Animator _animator;

    protected override void Awake()
    {
        base.Awake();
        m_sprite = transform.Find("Sprite").SafeGetComponent<SpriteRenderer>();
        _animator = m_sprite.transform.SafeGetComponent<Animator>();
        _sharedMat = m_sprite.sharedMaterial;
        if(_appearMat == null)
        {
            _appearMat = Instantiate(_sharedMat);
        }
    }

    public override void Init()
    {
        base.Init();
        Option = new BattleOptionItem()
        {
            OptionName = LocalizationManager.Instance.GetTextValue(BattleOption_EnterShop),
            OptionSprite = OptionSprite
        };
        _hasEnterShop = false;
        SoundManager.Instance.PlayBattleSound("Ship/Shop_Appear", transform);
        DoSpawnEffect();
        DelayDestroy();
    }

    private async void DoSpawnEffect()
    {
        m_sprite.material = _appearMat;
        _animator.SetTrigger(AnimTrigger_Spawn);
        var length = GameHelper.GetAnimatorClipLength(_animator, "ShopTeleportTrigger_Spawn");
        await UniTask.Delay((int)(length * 1000));
        _appearMat.SetFloat(Mat_Shader_PropertyKey_HOLOGRAM_ON, 0);
        m_sprite.material = _sharedMat;
    }

    public void DoDeSpawnEffect()
    {
        m_sprite.material = _appearMat;
        _animator.SetTrigger(AnimTrigger_DeSpawn);
    }

    public override void PoolableReset()
    {
        base.PoolableReset();
        _appearMat.SetFloat(Mat_Shader_PropertyKey_HOLOGRAM_ON, 0);
        _animator.ResetTrigger(AnimTrigger_Spawn);
        _animator.ResetTrigger(AnimTrigger_DeSpawn);
    }

    protected override void OnTrigger()
    {
        base.OnTrigger();
        OnEnterShop();
    }

    private async void DelayDestroy()
    {
        var refreshCfg = DataManager.Instance.gameMiscCfg.RefreshConfig;
        int delta = 1000 * (refreshCfg.Shop_Teleport_StayTime - refreshCfg.Shop_Teleport_WarningTime);
        delta = Mathf.Clamp(delta, 0, delta);

        await UniTask.Delay(refreshCfg.Shop_Teleport_WarningTime * 1000);
        if (!Vaild())
            return;

        RogueEvent.Trigger(RogueEventType.ShopTeleportWarning);

        await UniTask.Delay(delta);
        if (!Vaild())
            return;

        DoDeSpawnEffect();
        var length = GameHelper.GetAnimatorClipLength(_animator, "ShopTeleportTrigger_DeSpawn");
        await UniTask.Delay((int)(length * 1000));

        PoolableDestroy();
    }

    private void OnEnterShop()
    {
        if (_hasEnterShop)
            return;

        _hasEnterShop = true;
        RogueManager.Instance.EnterShop();
        PoolableDestroy();
    }

    private bool Vaild()
    {
        return gameObject != null && !_hasEnterShop;
    }
}
