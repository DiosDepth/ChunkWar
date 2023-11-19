using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ShowOdinSerializedPropertiesInInspector]
public class AIShip : BaseShip,IPoolable, IDropable
{
    public struct AIShipCoreHPChangeInfo
    {
        public int currentValue;
        public int totalValue;
        public float percent;
    }


    public AIShipConfig AIShipCfg;

    public int AITypeID;

    /// <summary>
    /// 覆盖难度参数
    /// </summary>
    public int OverrideHardLevelID = -1;

    private bool _isShowingOutLine = false;

    public override void Initialization()
    {
        base.Initialization();

        CreateShip();
    }

    protected override void Awake()
    {
        base.Awake();
        if (_appearMat == null)
        {
            _appearMat = Instantiate(_sharedMat);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
        for (int i = 0; i < _unitList.Count; i++)
        {
            if (_unitList[i].damageState != DamagableState.Destroyed && _unitList[i].isActiveAndEnabled)
            {
                _unitList[i].Death(info);
            }
        }
        OnDeath();
    }

    public override void ForceKill()
    {
        base.ForceKill();
        OnDeath();
    }

    private void OnDeath()
    {
        OnRemove();
        LevelManager.Instance.pickupList.AddRange(Drop());
        if (!string.IsNullOrEmpty(AIShipCfg.DieAudio))
        {
            SoundManager.Instance.PlayBattleSound(AIShipCfg.DieAudio, transform);
        }
        ECSManager.Instance.UnRegisterJobData(OwnerType.AI, this);
        EffectManager.Instance.CreateEffect(AIShipCfg.DieEffect, transform.position);
        PoolableDestroy();
    }

    protected virtual void OnRemove()
    {
        DestroyAIShipBillBoard();
        ResetAllAnimation();
        ///Set All Units
        for (int i = 0; i < _unitList.Count; i++)
        {
            _unitList[i].SetDisable();
        }

        //if (AIShipCfg.AppearChangeCameraOrthographicSize)
        //{
        //    CameraManager.Instance.SetZoom(GameGlobalConfig.CameraDefault_OrthographicSize);
        //}
    }

    public override void InitProperty()
    {
        base.InitProperty();
    }
    public override void CreateShip()
    {
        base.CreateShip();


        //初始化
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        //处理Chunk

        AIShipConfig aishipconfig =  DataManager.Instance.GetAIShipConfig(AITypeID);
        AIShipCfg = aishipconfig;
        baseShipCfg = aishipconfig;
        Vector2Int pos;

        for (int row = 0; row < aishipconfig.Map.GetLength(0); row++)
        {
            for (int colume = 0; colume < aishipconfig.Map.GetLength(1); colume++)
            {
                if (aishipconfig.Map[row, colume] == 0)
                {
                    continue;
                }
                pos = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                _chunkMap[row, colume] = new Chunk();
                _chunkMap[row, colume].shipCoord = pos;
                _chunkMap[row, colume].state = DamagableState.Normal;
                _chunkMap[row, colume].isBuildingPiovt = false;
                _chunkMap[row, colume].isOccupied = false;
            }
        }
         //处理unit
        _unitList = buildingsParent.GetComponentsInChildren<Unit>(true).ToList<Unit>();
        BaseUnitConfig unitconfig;
        for (int i = 0; i < _unitList.Count; i++)
        {
            var unit = _unitList[i];
            unitconfig = DataManager.Instance.GetUnitConfig(_unitList[i].UnitID);
            unit.gameObject.SetActive(true);
            unit.UID = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.AIShipUnit, unit);
            unit.Initialization(this, unitconfig);
            unit.SetUnitProcess(true);
            if (unit.IsCoreUnit && !CoreUnits.Contains(unit))
            {
                CoreUnits.Add(unit);
            }
        }
        ///Do Spawn
        DoSpawnEffect();
        SpawnSpecial();
    }

    public void PoolableReset()
    {
        OnRemove();
        CoreUnits.Clear();
        _isShowingOutLine = false;
        _render.material = _sharedMat;
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    /// <summary>
    /// 单位血量变化
    /// </summary>
    public AIShipCoreHPChangeInfo OnCoreHPChange()
    {
        AIShipCoreHPChangeInfo info = new AIShipCoreHPChangeInfo();

        int currentValue = 0;
        int totalValue = 0;
        for (int i = 0; i < CoreUnits.Count; i++) 
        {
            var core = CoreUnits[i];
            currentValue += core.HpComponent.GetCurrentHP;
            totalValue += core.HpComponent.MaxHP;
        }

        info.currentValue = currentValue;
        info.totalValue = totalValue;
        info.percent = currentValue / (float)totalValue;
        LevelManager.Instance.EnemyCoreHPPercentChange(info.percent, currentValue, totalValue);
        return info;
    }

    private void InitAIShipBillBoard()
    {
        if (!AIShipCfg.ShowHPBillboard)
            return;

        if (conditionState.CurrentState == ShipConditionState.Death)
            return;

        if(AIShipCfg.BillboardType == EnemyHPBillBoardType.Boss_UI)
        {
            RogueEvent.Trigger(RogueEventType.RegisterBossHPBillBoard, this);
        }
        else if (AIShipCfg.BillboardType == EnemyHPBillBoardType.Elite_Scene)
        {
            ///注册所有Unit的血条显示
            RegisterAllUnitSceneHPBar();
        }
    }

    private void DestroyAIShipBillBoard()
    {
        if (!AIShipCfg.ShowHPBillboard)
            return;

        if(AIShipCfg.BillboardType == EnemyHPBillBoardType.Boss_UI)
        {
            RogueEvent.Trigger(RogueEventType.RemoveBossHPBillBoard, this);
        }
        else if(AIShipCfg.BillboardType == EnemyHPBillBoardType.Elite_Scene)
        {

        }
    }

    public override void GameOver()
    {
        base.GameOver();
        PoolableDestroy();
    }
    public override void PauseGame()
    {
        base.PauseGame();
    }

    public override void UnPauseGame()
    {
        base.UnPauseGame();
    }

    /// <summary>
    /// 出生特殊效果
    /// </summary>
    private void SpawnSpecial()
    {
        //if (AIShipCfg.AppearChangeCameraOrthographicSize)
        //{
        //    CameraManager.Instance.SetZoom(AIShipCfg.CameraTargetOrthographicSize);
        //}

        if (AIShipCfg.AppearWarning)
        {
            UIManager.Instance.CreatePoolerUI<EnemyWarningLabel>("EnemyWarningLabel", true, E_UI_Layer.Bot, null, (panel) =>
            {
                panel.SetUp(LocalizationManager.Instance.GetTextValue(AIShipCfg.AppearWarningText));
            });

            if (!string.IsNullOrEmpty(AIShipCfg.AppearWarningAudio))
            {
                SoundManager.Instance.PlayUISound(AIShipCfg.AppearWarningAudio);
            }
        }
    }

    #region Drop

    public virtual List<PickableItem> Drop()
    {
        List<PickableItem> itemlist = new List<PickableItem>();
        var lst = AIShipCfg.DropList;
        if (lst == null || lst.Count <= 0)
        {
            return null;
        }
        for (int i = 0; i < lst.Count; i++)
        {
            var dropInfo = lst[i];
            var dropRate = GameHelper.CalculateDropRate(dropInfo.dropRate);
            bool isDrop = Utility.RandomResultWithOne(0, dropRate);
            if (!isDrop)
                continue;

            if (dropInfo.pickuptype == AvaliablePickUp.WastePickup)
            {
                itemlist.AddRange(HandleWasteDropPickUp(dropInfo));
            }
            else if (dropInfo.pickuptype == AvaliablePickUp.Wreckage)
            {
                var wreckageDrop = HandleWreckageDropPickUp(dropInfo);
                if (wreckageDrop != null)
                {
                    itemlist.Add(wreckageDrop);
                }
            }
        }
        return itemlist;
    }

    /// <summary>
    /// 装备残骸掉落
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private PickableItem HandleWreckageDropPickUp(DropInfo info)
    {
        var dropRateCfg = AIShipCfg.WreckageRarityCfg;
        if (dropRateCfg == null || dropRateCfg.Count <= 0)
            return null;

        List<GeneralRarityRandomItem> randomLst = new List<GeneralRarityRandomItem>();
        foreach (var item in dropRateCfg)
        {
            GeneralRarityRandomItem random = new GeneralRarityRandomItem
            {
                Rarity = item.Key,
                Weight = GetWeightByRarityAndWaveIndex(item.Key)
            };
            randomLst.Add(random);
        }

        var randomResult = Utility.GetRandomList<GeneralRarityRandomItem>(randomLst, 1);
        if (randomResult.Count == 1)
        {
            var result = randomResult[0];

            var pickUpData = DataManager.Instance.GetWreckagePickUpData(result.Rarity);
            if (pickUpData == null)
                return null;

            PickUpWreckage item = null;

            PoolManager.Instance.GetObjectSync(pickUpData.PrefabPath, true, (obj) =>
            {
                obj.transform.position = GetDropPosition();
                item = obj.GetComponent<PickUpWreckage>();
                item.Initialization(pickUpData);
            });
            return item;
        }
        return null;
    }

    private int GetWeightByRarityAndWaveIndex(GoodsItemRarity rarity)
    {
        var rarityMap = AIShipCfg.WreckageRarityCfg;
        if (!rarityMap.ContainsKey(rarity))
            return 0;

        var rarityCfg = rarityMap[rarity];
        var waveIndex = RogueManager.Instance.GetCurrentWaveIndex;
        if (waveIndex < rarityCfg.MinAppearEneterCount)
            return 0;

        float luckRate = 1;
        if (waveIndex >= rarityCfg.LuckModifyMinEnterCount)
        {
            var playerLuck = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Luck);
            luckRate = (1 + playerLuck / 100f);
        }

        var result = rarityCfg.WeightAddPerEnterCount * waveIndex  + rarityCfg.BaseWeight * luckRate;
        result = Mathf.Clamp(result, 0, rarityCfg.WeightMax);
        return Mathf.RoundToInt(result * 10);
    }

    /// <summary>
    /// 处理残骸掉落
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private List<PickableItem> HandleWasteDropPickUp(DropInfo info)
    {
        List<PickableItem> outLst = new List<PickableItem>();

        ///Calculate DropCount
        var dropCountAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EnemyDropCountPercent);
        var dropRatio = Mathf.Clamp(dropCountAdd / 100f + 1, 0, float.MaxValue);
        float dropCount = info.count * dropRatio;
        var dropResult = GameHelper.GeneratePickUpdata(dropCount);

        foreach (var result in dropResult)
        {
            int count = result.Value;
            var data = result.Key;
            ///Drop
            for (int i = 0; i < count; i++)
            {
                PoolManager.Instance.GetObjectSync(data.PrefabPath, true, (obj) =>
                {
                    obj.transform.position = GetDropPosition();
                    PickUpWaste item = obj.GetComponent<PickUpWaste>();
                    item.Initialization(data);
                    outLst.Add(item);
                }, LevelManager.PickUpPool);
            }
        }
        return outLst;
    }

    private Vector2 GetDropPosition()
    {
        float MaxSize = Mathf.Max(baseShipCfg.MapSize.Lager(), 2);
        Vector2 shipPos = transform.position.ToVector2();
        return MathExtensionTools.GetRadomPosFromOutRange(0.5f, MaxSize, shipPos);
    }



    #endregion

    #region Anim

    protected const string AnimTrigger_Spawn = "Spawn";
    protected const string AnimTrigger_DeSpawn = "DeSpawn";

    /// <summary>
    /// 强化描边
    /// </summary>
    public void ShowEnhanceOutline()
    {
        if (_isShowingOutLine)
            return;

        _isShowingOutLine = true;
        _render.material = _appearMat;
        _appearMat.SetFloat(Mat_Shader_ProeprtyKey_OUTBASE_ON, 1);
    }

    /// <summary>
    /// 去除强化描边
    /// </summary>
    public void HideEnhanceOutLine()
    {
        _appearMat.SetFloat(Mat_Shader_ProeprtyKey_OUTBASE_ON, 0);
        _isShowingOutLine = false;
        _render.material = _sharedMat;
    }

    private async void DoSpawnEffect()
    {
        _render.material = _appearMat;
        SetAnimatorTrigger(AnimTrigger_Spawn);
        ///UnitSpawn
        for (int i = 0; i < _unitList.Count; i++) 
        {
            if (_unitList[i].isActiveAndEnabled)
            {
                _unitList[i].DoSpawnEffect();
            }
        }

        var length = GameHelper.GetAnimatorClipLength(_spriteAnimator, "EnemyShip_Spawn");
        await UniTask.Delay((int)(length * 1000));
        _appearMat.SetFloat(Mat_Shader_PropertyKey_HOLOGRAM_ON, 0);
        ///Spawn Effect Finish
        _render.material = _sharedMat;
        InitAIShipBillBoard();
    }

    /// <summary>
    /// 消失动画
    /// </summary>
    public void DoDeSpawnEffect()
    {
        _render.material = _appearMat;
        SetAnimatorTrigger(AnimTrigger_DeSpawn);

        ///UnitDeSpawn
        for (int i = 0; i < _unitList.Count; i++)
        {
            if (_unitList[i].isActiveAndEnabled)
            {
                _unitList[i].DoDeSpawnEffect();
            }
        }
    }

    protected override void ResetAllAnimation()
    {
        base.ResetAllAnimation();
        _appearMat.SetFloat(Mat_Shader_PropertyKey_HOLOGRAM_ON, 0);
        _appearMat.SetFloat(Mat_Shader_ProeprtyKey_OUTBASE_ON, 0);
        _spriteAnimator.ResetTrigger(AnimTrigger_Spawn);
    }

    #endregion

    #region UIDisplay

    private void RegisterAllUnitSceneHPBar()
    {
        for(int i = 0; i < _unitList.Count; i++)
        {
            if (_unitList[i].isActiveAndEnabled)
            {
                _unitList[i].RegisterHPBar();
            }
        }
    }

    #endregion
}
