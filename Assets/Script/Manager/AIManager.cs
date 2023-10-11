using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.U2D.Common;
using UnityEngine;
using UnityEngine.U2D;
using static GameHelper;

public enum FindCondition
{
    MaximumHP,
    MinimumHP,
    ClosestDistance,
    LongestDistance,
}

public struct UnitReferenceInfo
{
    public TargetInfo info;
    public Unit reference;
}

public class AIManager : Singleton<AIManager>, IPauseable
{
    public const int MaxAICount = 300;
    public bool ProcessAI = false;
    public int ShipCount { get { return aiShipList.Count; } }
    public int BulletCount { get { return aiProjectileList.Count; } }


    //���Ship��ΪĿ��洢��������Ҫ����Unit�Ƿ񼤻�������targetActiveUnitList��ͬʱ����targetActiveUnitPos
    public NativeList<float3> avoidanceCollisionPos;
    public NativeList<float> avoidanceCollisionRadius;
    public NativeList<float3> avoidanceCollisionVel;

    public IBoid playerBoid;
    public List<Unit> playerActiveUnitList = new List<Unit>();
    public NativeList<float3> playerActiveUnitPos;


    //AI list Info
    public List<AIShip> aiShipList = new List<AIShip>();
    public List<Projectile> aiProjectileList = new List<Projectile>();
    private List<Projectile> _aiProjectileDamageList = new List<Projectile>();
    private List<int> _aiProjectileDeathIndex = new List<int>();
    private List<Projectile> _aiProjectileDeathList = new List<Projectile>();
    public NativeList<ProjectileJobInitialInfo> aiProjectile_JobInfo;

    public NativeArray<ProjectileJobRetrunInfo> rv_aiProjectile_jobUpdateInfo;
    public NativeList<ProjectileJobInitialInfo> aiDamageProjectile_JobInfo;
    public NativeArray<int> rv_aiProjectileDamageTargetIndex;
    public NativeArray<int> rv_aiProjectileDamageTargetCountPre;



    public List<AISteeringBehaviorController> aiSteeringBehaviorControllerList = new List<AISteeringBehaviorController>();
    public List<IBoid> aiShipBoidList = new List<IBoid>();

    //��Ҫ��ÿ��AI��Behavior���ݵ����������� ����ShipCount
    //public List<ArriveBehavior> arriveBehavior = new List<ArriveBehavior>();
    //public List<FaceBehavior> faceBehavior = new List<FaceBehavior>();
    //public List<CohesionBehavior> cohesionBehavior = new List<CohesionBehavior>();
    //public List<SeparationBehavior> separationBehavior = new List<SeparationBehavior>();
    //public List<AlignmentBehavior> aligmentBehavior = new List<AlignmentBehavior>();




    public List<Unit> aiActiveUnitList = new List<Unit>();
    //��Ҫ����aiship Unit���������µ�Jobdata
    public NativeList<float3> aiActiveUnitPos;
    public NativeList<float> aiActiveUnitAttackRange;
    public NativeList<int> aiActiveUnitMaxTargetsCount;


  


    //ȫ�ֵ�Jobdata�� ��������ShipCount ------------------------------------------------
    public NativeList<float3> steeringBehaviorJob_aiShipPos;
    public NativeList<float3> steeringBehaviorJob_aiShipVelocity;
    public NativeList<float> steeringBehaviorJob_aiShipRotationZ;
    public NativeList<float> steeringBehaviorJob_aiShipRadius;
    public NativeList<float> aiSteeringBehaviorController_aiShipMaxAcceleration;
    public NativeList<float> aiSteeringBehaviorController_aiShipMaxAngularAcceleration;
    public NativeList<float> aiSteeringBehaviorController_aiShipTargetSerchingRadius;
    public NativeList<float> aiSteeringBehaviorController_aiShipDrag;



    public NativeArray<SteeringBehaviorInfo> rv_evade_steeringInfo;
    public NativeList<float> evade_maxPrediction;
    public NativeList<float> evade_weight;
    public NativeList<bool> evade_isActive;


    //arrivebehavior return data
    public NativeArray<SteeringBehaviorInfo> rv_arrive_steeringInfo;
    public NativeArray<bool> rv_arrive_isVelZero;
    //arrivebehavior job input data
    public NativeList<float> arrive_arriveRadius;
    public NativeList<float> arrive_slowRadius;
    public NativeList<float> arrive_weight;
    public NativeList<bool> arrive_isActive;

    //facebehavior return data
    public NativeArray<SteeringBehaviorInfo> rv_face_steeringInfo;
    //facebehavior job input data
    public NativeList<float> face_facetargetRadius;
    public NativeList<float> face_weight;
    public NativeList<bool> face_isActive;



    public NativeArray<SteeringBehaviorInfo> rv_cohesion_steeringInfo;
    public NativeList<float> cohesion_viewAngle;
    public NativeList<float> cohesion_weight;
    public NativeList<bool> cohesion_isActive;

    public NativeArray<SteeringBehaviorInfo> rv_separation_steeringInfo;
    public NativeList<float> separation_threshold;
    public NativeList<float> separation_decayCoefficient;
    public NativeList<float> separation_weight;
    public NativeList<bool> separation_isActive;

    public NativeArray<SteeringBehaviorInfo> rv_alignment_steeringInfo;
    public NativeList<float> alignment_weight;
    public NativeList<float> alignment_alignDistance;
    public NativeList<bool> alignment_isActive;

    public NativeArray<SteeringBehaviorInfo> rv_collisionavoidance_steeringInfo;
    public NativeList<float> collisionavoidance_weight;
    public NativeList<bool> collisionavoidance_isActive;


    public NativeArray<SteeringBehaviorInfo> rv_deltaMovement;

    public NativeArray<float3> rv_serchingTargetsPosPerShip;
    public NativeArray<float3> rv_serchingTargetsVelPerShip;
    public NativeArray<int> rv_serchingTargetsCountPerShip;




    //weapon Update job result data
    NativeArray<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfo;

    // Start is called before the first frame update
    public AIManager()
    {
        GameManager.Instance.RegisterPauseable(this);
        MonoManager.Instance.AddUpdateListener(Update);
        MonoManager.Instance.AddLaterUpdateListener(LaterUpdate);
        MonoManager.Instance.AddFixedUpdateListener(FixedUpdate);
    }
    ~AIManager()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        MonoManager.Instance.RemoveUpdateListener(Update);
        MonoManager.Instance.RemoveUpdateListener(LaterUpdate);
        MonoManager.Instance.RemoveFixedUpdateListener(FixedUpdate);
    }

    public override void Initialization()
    {
        base.Initialization();
        GameManager.Instance.RegisterPauseable(this);
        AllocateAIJobData();

        playerBoid = RogueManager.Instance.currentShip.GetComponent<IBoid>();
        playerActiveUnitList.AddRange(RogueManager.Instance.currentShip.UnitList);

        avoidanceCollisionPos.Add(playerBoid.GetPosition());
        avoidanceCollisionRadius.Add(playerBoid.GetRadius());
        avoidanceCollisionVel.Add(playerBoid.GetVelocity());



        ProcessAI = true;
        // allocate all job data 
    

    }


    public virtual List<UnitReferenceInfo> GetActiveUnitReferenceByTargetInfo(TargetInfo[] info)
    {
        List<UnitReferenceInfo> result = new List<UnitReferenceInfo>();
        UnitReferenceInfo tempinfo;
        for (int i = 0; i < info.Length; i++)
        {
            tempinfo.info = info[i];
            tempinfo.reference = aiActiveUnitList[info[i].index];
            result.Add(tempinfo);
        }
        return result;
    }

    public virtual void AllocateAIJobData()
    {
        playerActiveUnitPos = new NativeList<float3>(Allocator.Persistent);
        avoidanceCollisionPos = new NativeList<float3>(Allocator.Persistent);
        avoidanceCollisionRadius = new NativeList<float>(Allocator.Persistent);
        avoidanceCollisionVel = new NativeList<float3>(Allocator.Persistent);

        aiActiveUnitPos = new NativeList<float3>(Allocator.Persistent);

        aiActiveUnitAttackRange = new NativeList<float>(Allocator.Persistent);
        aiActiveUnitMaxTargetsCount = new NativeList<int>(Allocator.Persistent);


        aiProjectile_JobInfo = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);
        aiDamageProjectile_JobInfo = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);

        steeringBehaviorJob_aiShipPos = new NativeList<float3>(Allocator.Persistent);
        steeringBehaviorJob_aiShipVelocity = new NativeList<float3>(Allocator.Persistent);
        steeringBehaviorJob_aiShipRotationZ = new NativeList<float>(Allocator.Persistent);
        steeringBehaviorJob_aiShipRadius = new NativeList<float>(Allocator.Persistent);

        aiSteeringBehaviorController_aiShipMaxAcceleration = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipDrag = new NativeList<float>(Allocator.Persistent);


        evade_maxPrediction = new NativeList<float>(Allocator.Persistent);
        evade_weight = new NativeList<float>(Allocator.Persistent);
        evade_isActive = new NativeList<bool>(Allocator.Persistent);
        //arrivebehavior return data
        //arrivebehavior job input data
        arrive_arriveRadius = new NativeList<float>(Allocator.Persistent);
        arrive_slowRadius = new NativeList<float>(Allocator.Persistent);
        arrive_weight = new NativeList<float>(Allocator.Persistent);
        arrive_isActive = new NativeList<bool>(Allocator.Persistent);
        //facebehavior return data
    
        //facebehavior job input data
        face_weight = new NativeList<float>(Allocator.Persistent);
        face_facetargetRadius = new NativeList<float>(Allocator.Persistent);
        face_isActive = new NativeList<bool>(Allocator.Persistent);


        cohesion_weight = new NativeList<float>(Allocator.Persistent);
        cohesion_viewAngle = new NativeList<float>(Allocator.Persistent);
        cohesion_isActive = new NativeList<bool>(Allocator.Persistent);

        separation_weight = new NativeList<float>(Allocator.Persistent);
        separation_threshold = new NativeList<float>(Allocator.Persistent);
        separation_decayCoefficient = new NativeList<float>(Allocator.Persistent);
        separation_isActive = new NativeList<bool>(Allocator.Persistent);


        alignment_weight = new NativeList<float>(Allocator.Persistent);
        alignment_alignDistance = new NativeList<float>(Allocator.Persistent);
        alignment_isActive = new NativeList<bool>(Allocator.Persistent);

        collisionavoidance_weight = new NativeList<float>(Allocator.Persistent);
        collisionavoidance_isActive = new NativeList<bool>(Allocator.Persistent);

    }
    public virtual void UpdateMoveJobData()
    {

        // ship Job data
        if (aiShipBoidList != null && aiShipBoidList.Count != 0)
        {
            for (int i = 0; i < ShipCount; i++)
            {
                steeringBehaviorJob_aiShipPos[i] = aiShipBoidList[i].GetPosition();
                steeringBehaviorJob_aiShipVelocity[i] = aiShipBoidList[i].GetVelocity();
                steeringBehaviorJob_aiShipRadius[i] = aiShipBoidList[i].GetRadius();
                steeringBehaviorJob_aiShipRotationZ[i] = aiShipBoidList[i].GetRotationZ();
            }
        }

        if(playerBoid != null)
        {
            avoidanceCollisionPos[0] = playerBoid.GetPosition();
            avoidanceCollisionRadius[0] = playerBoid.GetRadius();
            avoidanceCollisionVel[0] = playerBoid.GetVelocity();
        }
        //weapon job data
        if(playerActiveUnitList.Count >0)
        {
            for (int i = 0; i < playerActiveUnitList.Count; i++)
            {
                playerActiveUnitPos[i] = playerActiveUnitList[i].transform.position;
            }
        }

        if (aiActiveUnitList.Count > 0)
        {
            for (int i = 0; i < aiActiveUnitList.Count; i++)
            {
                aiActiveUnitPos[i] = aiActiveUnitList[i].transform.position;
            }
        }



    }
    public virtual void DisposeAIJobData()
    {
        if (playerActiveUnitPos.IsCreated) { playerActiveUnitPos.Dispose(); }
        if (avoidanceCollisionPos.IsCreated) { avoidanceCollisionPos.Dispose(); }
        if (avoidanceCollisionRadius.IsCreated) { avoidanceCollisionRadius.Dispose(); }
        if (avoidanceCollisionVel.IsCreated) { avoidanceCollisionVel.Dispose(); }

        if (aiActiveUnitPos.IsCreated) { aiActiveUnitPos.Dispose(); }
        if(aiActiveUnitAttackRange.IsCreated) { aiActiveUnitAttackRange.Dispose(); }
        if (aiActiveUnitMaxTargetsCount.IsCreated) { aiActiveUnitMaxTargetsCount.Dispose(); }
        if (aiProjectile_JobInfo.IsCreated) { aiProjectile_JobInfo.Dispose(); }
        if (aiDamageProjectile_JobInfo.IsCreated) { aiDamageProjectile_JobInfo.Dispose(); }

        //ai data dispose
        if (steeringBehaviorJob_aiShipPos.IsCreated) { steeringBehaviorJob_aiShipPos.Dispose(); }
        if (steeringBehaviorJob_aiShipVelocity.IsCreated) { steeringBehaviorJob_aiShipVelocity.Dispose(); }
        if (steeringBehaviorJob_aiShipRadius.IsCreated) { steeringBehaviorJob_aiShipRadius.Dispose(); }
        if (steeringBehaviorJob_aiShipRotationZ.IsCreated) { steeringBehaviorJob_aiShipRotationZ.Dispose(); }

        if (aiSteeringBehaviorController_aiShipMaxAcceleration.IsCreated) { aiSteeringBehaviorController_aiShipMaxAcceleration.Dispose(); }
        if (aiSteeringBehaviorController_aiShipMaxAngularAcceleration.IsCreated) { aiSteeringBehaviorController_aiShipMaxAngularAcceleration.Dispose(); }
        if (aiSteeringBehaviorController_aiShipTargetSerchingRadius.IsCreated) { aiSteeringBehaviorController_aiShipTargetSerchingRadius.Dispose(); }
        if (aiSteeringBehaviorController_aiShipDrag.IsCreated) { aiSteeringBehaviorController_aiShipDrag.Dispose(); }

        //dispose evade job data
        if (evade_maxPrediction.IsCreated) { evade_maxPrediction.Dispose(); }
        if (evade_weight.IsCreated) { evade_weight.Dispose(); }
        if(evade_isActive.IsCreated) { evade_isActive.Dispose(); }

        //dispose arrive job data
        if (arrive_arriveRadius.IsCreated) { arrive_arriveRadius.Dispose(); }
        if (arrive_slowRadius.IsCreated) { arrive_slowRadius.Dispose(); }
        if (arrive_weight.IsCreated) { arrive_weight.Dispose(); }
        if (arrive_isActive.IsCreated) { arrive_isActive.Dispose(); }

        //dispose face job data
        if (face_weight.IsCreated) { face_weight.Dispose(); }
        if (face_facetargetRadius.IsCreated) { face_facetargetRadius.Dispose(); }
        if (face_isActive.IsCreated) { face_isActive.Dispose(); }

        //dispose separation job data
        if (separation_weight.IsCreated) { separation_weight.Dispose(); }
        if (separation_threshold.IsCreated) { separation_threshold.Dispose(); }
        if (separation_decayCoefficient.IsCreated) { separation_decayCoefficient.Dispose(); }
        if (separation_isActive.IsCreated) { separation_isActive.Dispose(); }

        if (cohesion_weight.IsCreated) { cohesion_weight.Dispose(); }
        if (cohesion_viewAngle.IsCreated) { cohesion_viewAngle.Dispose(); }
        if(cohesion_isActive.IsCreated) { cohesion_isActive.Dispose(); }

        if (alignment_weight.IsCreated) { alignment_weight.Dispose(); }
        if (alignment_alignDistance.IsCreated) { alignment_alignDistance.Dispose(); }
        if (alignment_isActive.IsCreated) { alignment_isActive.Dispose(); }

        if (collisionavoidance_weight.IsCreated) { collisionavoidance_weight.Dispose(); }
        if (collisionavoidance_isActive.IsCreated) { collisionavoidance_isActive.Dispose(); }
        //weapon job data dispose
    }


    public void Unload()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        //clear all ai
        if ( aiShipList != null)
        {
            for (int i = 0; i < aiShipList.Count; i++)
            {
                aiShipList[i].PoolableDestroy();
            }
            if (aiShipList.Count > 0)
            {
               ClearAI();
            }
        }
        if(playerActiveUnitList != null)
        {
            if(playerActiveUnitList.Count >0)
            {
                ClearTarget();
            }
        }

        playerBoid = null;

        //clear all player
        //todo clear all ai weapon
        aiActiveUnitList.Clear();
       
        //todo clear all bullet
        for (int i = 0; i < aiProjectileList.Count; i++)
        {
            aiProjectileList[i].Death(null);
            
        }
        aiProjectileList.Clear();
        _aiProjectileDamageList.Clear();
        _aiProjectileDeathIndex.Clear();

        //Dispose all job data
        DisposeAIJobData();
    }
    public void GameOver()
    {
        ProcessAI = false;
        Stop();
        Unload();
    }
    public void Stop()
    {
        ProcessAI = false;
        for (int i = 0; i < aiShipList.Count; i++)
        {
            for (int n = 0; n < aiShipList[i].UnitList.Count; n++)
            {
                aiShipList[i].UnitList[n].GameOver();
            }
            aiShipList[i].controller.GameOver();
        }
    }
    private void Update()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessAI) { return; }
        //update weapon
        UpdateAIAdditionalWeapon();
        //UpdateAIBuilding();
        //UpdateAIDrone();
        UpdateProjectile();
    }

    private void LaterUpdate()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessAI) { return; }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessAI) { return; }
        UpdateMoveJobData();
        UpdateAIMovement();
    }


    public void AddAI(AIShip ship)
    {
        if(aiShipList.Contains(ship))
        {
            return;
        }

        aiShipList.Add(ship);
        OnEnemyCountChange();
        IBoid boid = ship.GetComponent<IBoid>();
        aiShipBoidList.Add(boid);

        AddUnit(ship);
        AISteeringBehaviorController controller = ship.GetComponent<AISteeringBehaviorController>();
        aiSteeringBehaviorControllerList.Add(controller);

        //����Job��Ϣ

        steeringBehaviorJob_aiShipPos.Add(boid.GetPosition());
        steeringBehaviorJob_aiShipRadius.Add(boid.GetRadius());
        steeringBehaviorJob_aiShipVelocity.Add(boid.GetVelocity());
        steeringBehaviorJob_aiShipRotationZ.Add(boid.GetRotationZ());

        aiSteeringBehaviorController_aiShipMaxAcceleration.Add(controller.maxAcceleration);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration.Add(controller.maxAngularAcceleration);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius.Add(controller.targetSerchingRadius);
        aiSteeringBehaviorController_aiShipDrag.Add(controller.drag);

        evade_maxPrediction.Add(controller.evadeBehaviorInfo.maxPrediction);
        evade_weight.Add(controller.evadeBehaviorInfo.GetWeight());
        evade_isActive.Add(controller.isActiveEvade);

        arrive_arriveRadius.Add(controller.arrivelBehaviorInfo.arriveRadius);
        arrive_slowRadius.Add(controller.arrivelBehaviorInfo.slowRadius);
        arrive_weight.Add(controller.arrivelBehaviorInfo.GetWeight());
        arrive_isActive.Add(controller.isActiveArrive);

        face_weight.Add(controller.faceBehaviorInfo.GetWeight());
        face_facetargetRadius.Add(controller.faceBehaviorInfo.facetargetRadius);
        face_isActive.Add(controller.isActiveFace);

        cohesion_weight.Add(controller.cohesionBehaviorInfo.GetWeight());
        cohesion_viewAngle.Add(controller.cohesionBehaviorInfo.viewAngle);
        cohesion_isActive.Add(controller.isActiveCohesion);

        separation_weight.Add(controller.separationBehaviorInfo.GetWeight());
        separation_threshold.Add(controller.separationBehaviorInfo.threshold);
        separation_decayCoefficient.Add(controller.separationBehaviorInfo.decayCoefficient);
        separation_isActive.Add(controller.isActiveSeparation);

        alignment_weight.Add(controller.alignmentBehaviorInfo.GetWeight());
        alignment_alignDistance.Add(controller.alignmentBehaviorInfo.alignDistance);
        alignment_isActive.Add(controller.isActiveAligment);

        collisionavoidance_weight.Add(controller.collisionAvoidanceBehaviorInfo.GetWeight());
        collisionavoidance_isActive.Add(controller.isActiveCollisionAvoidance);

    }

    public void AddAIRange(List<AIShip> shiplist)
    {
        for (int i = 0; i < shiplist.Count; i++)
        {
            AddAI(shiplist[i]);
        }

    }

    public void RemoveAI(AIShip ship)
    {
        int index = aiShipList.IndexOf(ship);
        RemoveReminedUnit(ship);
        RemoveAI(index);
        OnEnemyCountChange();
    }
    protected void RemoveAI(int index)
    {
        aiShipList.RemoveAt(index);
        aiShipBoidList.RemoveAt(index);
        aiSteeringBehaviorControllerList.RemoveAt(index);


        steeringBehaviorJob_aiShipPos.RemoveAt(index);
        steeringBehaviorJob_aiShipRadius.RemoveAt(index);
        steeringBehaviorJob_aiShipVelocity.RemoveAt(index);
        steeringBehaviorJob_aiShipRotationZ.RemoveAt(index);

        aiSteeringBehaviorController_aiShipMaxAcceleration.RemoveAt(index);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration.RemoveAt(index);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius.RemoveAt(index);
        aiSteeringBehaviorController_aiShipDrag.RemoveAt(index);

        evade_maxPrediction.RemoveAt(index);
        evade_weight.RemoveAt(index);
        evade_isActive.RemoveAt(index);

        arrive_arriveRadius.RemoveAt(index);
        arrive_slowRadius.RemoveAt(index);
        arrive_weight.RemoveAt(index);
        arrive_isActive.RemoveAt(index);


        face_weight.RemoveAt(index);
        face_facetargetRadius.RemoveAt(index);
        face_isActive.RemoveAt(index);

        cohesion_weight.RemoveAt(index);
        cohesion_viewAngle.RemoveAt(index);
        cohesion_isActive.RemoveAt(index);

        separation_weight.RemoveAt(index);
        separation_threshold.RemoveAt(index);
        separation_decayCoefficient.RemoveAt(index);
        separation_isActive.RemoveAt(index);

        alignment_weight.RemoveAt(index);
        alignment_alignDistance.RemoveAt(index);
        alignment_isActive.RemoveAt(index);

        collisionavoidance_weight.RemoveAt(index);
        collisionavoidance_isActive.RemoveAt(index);
    }


    public void ClearAI()
    {
        aiShipList.Clear();
        aiShipBoidList.Clear();
        aiActiveUnitList.Clear();
        aiSteeringBehaviorControllerList.Clear();
       
    }
    public void ClearTarget()
    {

        playerActiveUnitList.Clear();

    }
    public void  AddUnit(AIShip ship)
    {
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
                AddSingleUnit(ship.UnitList[i]);
        }
    }
    public void AddSingleUnit(Unit unit)
    {
        if(!aiActiveUnitList.Contains(unit) && unit.isActiveAndEnabled)
        {
            aiActiveUnitList.Add(unit);
            aiActiveUnitPos.Add(unit.transform.position);
            aiActiveUnitAttackRange.Add(unit.baseAttribute.WeaponRange);
            aiActiveUnitMaxTargetsCount.Add(unit.maxTargetCount);
        }
    }
    public void RemoveReminedUnit(AIShip ship)
    {
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
            if(ship.UnitList[i].isActiveAndEnabled)
            {
                RemoveSingleUnit(ship.UnitList[i]);
            }
        }
    }

    public void RemoveSingleUnit(Unit unit)
    {
        int index = aiActiveUnitList.IndexOf(unit);
        aiActiveUnitList.RemoveAt(index);
        aiActiveUnitPos.RemoveAt(index);
        aiActiveUnitAttackRange.RemoveAt(index);
        aiActiveUnitMaxTargetsCount.RemoveAt(index);
    }

    public void AddBullet(Bullet bullet )
    {
        if(bullet is Projectile)
        {
            AddProjectileBullet(bullet as Projectile);
        }
    }
    public void AddProjectileBullet(Projectile bullet)
    {
        if(!aiProjectileList.Contains(bullet))
        {
            aiProjectileList.Add(bullet);
            float3 initialtargetpos;
            if((bullet.Owner as Weapon).aimingtype == WeaponAimingType.Directional)
            {
                initialtargetpos = initialtargetpos = bullet.transform.position + bullet.transform.up * bullet.Owner.baseAttribute.WeaponRange;
            }
            else
            {
                initialtargetpos = bullet.initialTarget.transform.position;
            }
            aiProjectile_JobInfo.Add(new ProjectileJobInitialInfo
                (
                    bullet.initialTarget? bullet.initialTarget.transform.position : bullet.transform.up,
                    bullet.transform.position,
                    bullet.lifeTime,
                    bullet.maxSpeed,
                    bullet.initialSpeed,
                    bullet.acceleration,
                    bullet.rotSpeed,
                    bullet.InitialmoveDirection.ToVector3(),
                    initialtargetpos,
                   (int)bullet.movementType,
                   (int)bullet.damagePattern,
                   bullet.DamageRadiusBase
                ));
        }

    }

    public void RemoveProjectileBullet(Projectile bullet)
    {
        int index = aiProjectileList.IndexOf(bullet);
       aiProjectile_JobInfo.RemoveAt(index);
       aiProjectileList.RemoveAt(index);
    }
    public void RemoveBullet(Bullet bullet)
    {
        if(bullet is Projectile)
        {
            RemoveProjectileBullet(bullet as Projectile);
        }
        //todo �������Projectile������Ҫ�õ�������������Remove
    }

    public void AddTargetUnit(Unit unit)
    {
        if (!playerActiveUnitList.Contains(unit))
        {
            playerActiveUnitList.Add(unit);
            playerActiveUnitPos.Add(unit.transform.position);
            
        }
    }

    public void RemoveTargetUnit(Unit unit)
    {
        int index  = playerActiveUnitList.IndexOf(unit);
        playerActiveUnitList.RemoveAt(index);
        playerActiveUnitPos.RemoveAt(index);
    }
    public void UpdateAIMovement()
    {

        if (aiShipList == null || aiShipList.Count == 0)
        {
            return;
        }

        JobHandle jobhandle_evadebehavior;
        JobHandle jobhandle_arrivebehavior;
        JobHandle jobhandle_facebehavior;
        JobHandle jobhandle_behaviorTargets;
        JobHandle jobhandle_cohesionbehavior;
        JobHandle jobhandle_separationBehavior;
        JobHandle jobhandle_alignmentBehavior;
        JobHandle jobhandle_collisionAvoidanceBehavior;
        //jobhandleList_AI = new NativeList<JobHandle>(Allocator.TempJob);

        rv_evade_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        rv_arrive_isVelZero = new NativeArray<bool>(ShipCount, Allocator.TempJob);
        rv_arrive_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        rv_face_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_cohesion_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_separation_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_alignment_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_collisionavoidance_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);



        EvadeBehavior.EvadeBehaviorJob evadeBehaviorJob = new EvadeBehavior.EvadeBehaviorJob
        {
            job_aiShipPos= steeringBehaviorJob_aiShipPos,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_evadeTargetPos = playerBoid.GetPosition(),
            job_evadeTargetVel = playerBoid.GetVelocity(),
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_maxPrediction = evade_maxPrediction,

            rv_steering = rv_evade_steeringInfo,

        };
        jobhandle_evadebehavior = evadeBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_evadebehavior.Complete();


      

        ArriveBehavior.ArriveBehaviorJobs arriveBehaviorJobs = new ArriveBehavior.ArriveBehaviorJobs
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRadius = steeringBehaviorJob_aiShipRadius,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_arriveRadius = arrive_arriveRadius,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_slowRadius = arrive_slowRadius,
            job_targetPos = playerBoid.GetPosition(),
            job_targetRadius = playerBoid.GetRadius(),

            rv_isVelZero = rv_arrive_isVelZero,
            rv_Steerings = rv_arrive_steeringInfo,

        };
        jobhandle_arrivebehavior = arriveBehaviorJobs.ScheduleBatch(ShipCount, 2);
        jobhandle_arrivebehavior.Complete();

        ////FaceBehavior Job
        FaceBehavior.FaceBehaviorJob faceBehaviorJob = new FaceBehavior.FaceBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRotationZ = steeringBehaviorJob_aiShipRotationZ,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_facetagetRadius = face_facetargetRadius,
            job_maxAngularAcceleration = aiSteeringBehaviorController_aiShipMaxAngularAcceleration,
            job_targetPos = playerBoid.GetPosition(),
            job_deltatime = Time.fixedDeltaTime,

            rv_Steerings = rv_face_steeringInfo,
        };
        jobhandle_facebehavior = faceBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_facebehavior.Complete();

        //NativeList<JobHandle> behaviorTargetsList = new NativeList<JobHandle>();

        rv_serchingTargetsVelPerShip = new NativeArray<float3>(ShipCount * ShipCount, Allocator.TempJob);
        rv_serchingTargetsPosPerShip = new NativeArray<float3>(ShipCount * ShipCount, Allocator.TempJob);
        rv_serchingTargetsCountPerShip = new NativeArray<int>(ShipCount, Allocator.TempJob);


        AISteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob calculateSteeringTargetsPosByRadiusJob = new AISteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_SerchingRadius = aiSteeringBehaviorController_aiShipTargetSerchingRadius,
            job_threshold = 0.01f,
            

            rv_findedTargetCountPreShip = rv_serchingTargetsCountPerShip,
            rv_findedTargetVelPreShip = rv_serchingTargetsVelPerShip,
            rv_findedTargetsPosPreShip = rv_serchingTargetsPosPerShip,
        };
        jobhandle_behaviorTargets = calculateSteeringTargetsPosByRadiusJob.ScheduleBatch(ShipCount, 2);
        jobhandle_behaviorTargets.Complete();


        //Cohesion Job

        CohesionBehavior.CohesionBehaviorJob cohesionBehaviorJob = new CohesionBehavior.CohesionBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_viewAngle = cohesion_viewAngle,
            job_aiShipPosInRange = rv_serchingTargetsPosPerShip,
            job_InRangeLength = rv_serchingTargetsCountPerShip,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_shipcount = ShipCount,

            rv_Steerings = rv_cohesion_steeringInfo,
        };

        jobhandle_cohesionbehavior = cohesionBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_cohesionbehavior.Complete();

        //separation Job
        SeparationBehavior.SeparationBehaviorJob separationBehaviorJob = new SeparationBehavior.SeparationBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_decayCoefficient = separation_decayCoefficient,
            job_threshold = separation_threshold,
            job_aiShipPosInRange = rv_serchingTargetsPosPerShip,
            job_InRangeLength = rv_serchingTargetsCountPerShip,
            job_shipcount = ShipCount,

            rv_Steerings = rv_separation_steeringInfo,
        };
        jobhandle_separationBehavior = separationBehaviorJob.ScheduleBatch(ShipCount, 2);

        jobhandle_separationBehavior.Complete();

        //Alignment Job
        AlignmentBehavior.AlignmentBehaviorJob alignmentBehaviorJob = new AlignmentBehavior.AlignmentBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_alignDistance = alignment_alignDistance,
            job_aiShipPosInRange = rv_serchingTargetsPosPerShip,
            job_aiShipVelInRange = rv_serchingTargetsVelPerShip,
            job_lengthInRange = rv_serchingTargetsCountPerShip,

            job_shipcount = ShipCount,

            rv_Steerings = rv_alignment_steeringInfo,
        };

        jobhandle_alignmentBehavior = alignmentBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_alignmentBehavior.Complete();


        
        
        CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob collisionAvoidanceBehaviorJob = new CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRadius = steeringBehaviorJob_aiShipRadius,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,

            job_avoidenceTargetPos = avoidanceCollisionPos,
            job_avoidenceTargetRadius = avoidanceCollisionRadius,
            job_avoidenceTargetVel = avoidanceCollisionVel,

            rv_steering = rv_collisionavoidance_steeringInfo,

        };

        jobhandle_collisionAvoidanceBehavior = collisionAvoidanceBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_collisionAvoidanceBehavior.Complete();
        



        rv_deltaMovement = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        JobHandle jobhandle_deltamoveposjob;
        AISteeringBehaviorController.CalculateDeltaMovePosJob calculateDeltaMovePosJob = new AISteeringBehaviorController.CalculateDeltaMovePosJob
        {
            job_aiShipMaxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            Job_aiShipDrag = aiSteeringBehaviorController_aiShipDrag,
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipVelocity = steeringBehaviorJob_aiShipVelocity,

            job_evadeSteering = rv_evade_steeringInfo,
            job_evadeWeight = evade_weight,
            job_evadeIsActive = evade_isActive,

            job_arriveSteering = rv_arrive_steeringInfo,
            job_arriveWeight = arrive_weight,
            job_isVelZero = rv_arrive_isVelZero,
            job_arriveIsActive = arrive_isActive,

            job_faceSteering = rv_face_steeringInfo,
            job_faceWeight = face_weight,
            job_faceIsActive = face_isActive,

            job_alignmentSteering = rv_alignment_steeringInfo,
            job_alignmentWeight = alignment_weight,
            job_alignmentIsActive = alignment_isActive,

            job_cohesionSteering = rv_cohesion_steeringInfo,
            job_cohesionWeight = cohesion_weight,
            job_cohesionIsActive = cohesion_isActive,

            job_separationSteering = rv_separation_steeringInfo,
            job_separationWeight = separation_weight,
            job_separationIsActive = separation_isActive,

            job_collisionAvoidanceSteering = rv_collisionavoidance_steeringInfo,
            job_collisionAvoidanceWeight = collisionavoidance_weight,
            job_collisonAvidanceIsActive = collisionavoidance_isActive,


            job_deltatime = Time.fixedDeltaTime,

            rv_deltainfo = rv_deltaMovement,
        };

        jobhandle_deltamoveposjob = calculateDeltaMovePosJob.ScheduleBatch(ShipCount, 2);
        jobhandle_deltamoveposjob.Complete();
      

        for (int i = 0; i < ShipCount; i++)
        {
            aiSteeringBehaviorControllerList[i].UpdateIBoid();

            aiSteeringBehaviorControllerList[i].Move(rv_deltaMovement[i].linear);
            aiSteeringBehaviorControllerList[i].transform.rotation = Quaternion.Euler(0, 0,rv_deltaMovement[i].angular);
        }

        playerBoid.UpdateIBoid();

        rv_evade_steeringInfo.Dispose();

        rv_arrive_isVelZero.Dispose();
        rv_arrive_steeringInfo.Dispose();

        rv_face_steeringInfo.Dispose();
        rv_cohesion_steeringInfo.Dispose();
        rv_separation_steeringInfo.Dispose();
        rv_alignment_steeringInfo.Dispose();
        rv_collisionavoidance_steeringInfo.Dispose();

        rv_deltaMovement.Dispose();
        rv_serchingTargetsPosPerShip.Dispose();
        rv_serchingTargetsVelPerShip.Dispose();
        rv_serchingTargetsCountPerShip.Dispose();

    }

    public void UpdateAIAdditionalWeapon()
    {

        if( aiActiveUnitList == null || aiActiveUnitList.Count == 0)
        {
            return;
        }
        AIAdditionalWeapon weapon;

        int targetstotalcount = 0;
        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            targetstotalcount += aiActiveUnitList[i].maxTargetCount;
        }

        rv_weaponTargetsInfo = new NativeArray<Weapon.RV_WeaponTargetInfo>(targetstotalcount, Allocator.TempJob);

        // ���Process ��������Job�� ���ҽ����н����¼��targetsindex[]��
        //���Job����һ��JRD_targetsInfo ������Ѿ���������ݣ� ���� index�� Pos�� direction�� distance ����������


        Weapon.FindMutipleWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindMutipleWeaponTargetsJob
        {
            job_attackRange = aiActiveUnitAttackRange,
            job_selfPos = aiActiveUnitPos,
            job_targetsPos =playerActiveUnitPos,
            job_maxTargetCount = aiActiveUnitMaxTargetsCount,

            rv_targetsInfo = rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(aiActiveUnitList.Count, 2);


        jobHandle.Complete();
        //ִ��Job ���ҵȴ������



        int startindex;
        int targetindex;
        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            //��ȡFlat Array�е�startindex Ϊ�����Ĳ����׼��
            //��ǰ��ÿһ��unit�� maxtargetscountȫ������������FlatArray�еĵ�һ��index

            if (aiActiveUnitList[i] is AIAdditionalWeapon)
            {

                startindex = 0;
                for (int c = 0; c < i; c++)
                {
                    startindex += aiActiveUnitMaxTargetsCount[c];
                }


                weapon = aiActiveUnitList[i] as AIAdditionalWeapon;

                //���û���ڿ�������ڿ����Ъ�У�������ˢдweapon.targetlist
                if (weapon.weaponstate.CurrentState != WeaponState.Firing && weapon.weaponstate.CurrentState != WeaponState.BetweenDelay)
                {
                    weapon.targetList.Clear();
                    for (int n = 0; n < aiActiveUnitMaxTargetsCount[i]; n++)
                    {
                        targetindex = rv_weaponTargetsInfo[startindex + n].targetIndex;
                        if (targetindex == -1 || playerActiveUnitList == null || playerActiveUnitList.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            weapon.targetList.Add(new WeaponTargetInfo
                                (
                                    playerActiveUnitList[targetindex].gameObject,
                                    rv_weaponTargetsInfo[startindex + n].targetIndex,
                                    rv_weaponTargetsInfo[startindex + n].distanceToTarget,
                                    rv_weaponTargetsInfo[startindex + n].targetDirection
                                ));
                        }
                    }
                }
                if (weapon.targetList != null && weapon.targetList.Count != 0)
                {
                    weapon.WeaponOn();
                }
                else
                {
                    weapon.WeaponOff();
                }

                weapon.ProcessWeapon();
            }
        }

        rv_weaponTargetsInfo.Dispose();
        //Dispose all Job tempdata
    }



    public void UpdateProjectile()
    {
        if(aiProjectileList == null || aiProjectileList.Count == 0)
        {
            return;
        }
        rv_aiProjectile_jobUpdateInfo = new NativeArray<ProjectileJobRetrunInfo>(aiProjectileList.Count, Allocator.TempJob);
        // ��ȡ�ӵ�����
        // ��ȡ�ӵ���Target�����Ҹ���Targetλ�ã� �����
        JobHandle aiprojectilejobhandle;
        //�����ӵ���������Ƿ�targetbase��������Ӧ��JOB
        Projectile.CalculateProjectileMovementJobJob staightCalculateProjectileMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = aiProjectile_JobInfo,
            rv_bulletJobUpdateInfos = rv_aiProjectile_jobUpdateInfo,
        };

        aiprojectilejobhandle = staightCalculateProjectileMovementJobJob.ScheduleBatch(aiProjectileList.Count, 2);
        aiprojectilejobhandle.Complete();
        //�����ӵ���ǰ��JobData


        //�����ӵ����˺�List �Ͷ�Ӧ������List
        _aiProjectileDeathIndex.Clear();
        _aiProjectileDamageList.Clear();
        aiDamageProjectile_JobInfo.Clear();

        //Loop ���е��ӵ����Ҵ������ǵ��˺�������List��ע���˺���������List������һһ��Ӧ�ġ� ��Щ�ӵ��ڲ����˺�֮�󲢲������������紩͸�ӵ�
        for (int i = 0; i < aiProjectileList.Count; i++)
        {
            //�ƶ��ӵ�
            //�����ӵ���ת����
            if(!rv_aiProjectile_jobUpdateInfo[i].islifeended)
            {
                aiProjectileList[i].UpdateBullet();
            }
            //�ӵ����ƶ����˺��������б�ά����Ҫ�����ĸ���ͬ��������
            if (aiProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Collider)
            {
                //����Move �� Rotation
                if (!rv_aiProjectile_jobUpdateInfo[i].islifeended && !aiProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    aiProjectileList[i].Move(rv_aiProjectile_jobUpdateInfo[i].deltaMovement);
                    aiProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_aiProjectile_jobUpdateInfo[i].rotation);
                }
                else
                {
                    if (aiProjectileList[i].IsApplyDamageAtThisFrame)
                    {
                        _aiProjectileDamageList.Add(aiProjectileList[i]);
                        aiDamageProjectile_JobInfo.Add(aiProjectile_JobInfo[i]);
                    }
                    //Collide�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    _aiProjectileDeathIndex.Add(i);
                }
            }

            if (aiProjectileList[i].damageTriggerPattern == DamageTriggerPattern.PassTrough)
            {
                //Passthrough ���͵��ӵ��� ֮����������ĩβ
                if (!rv_aiProjectile_jobUpdateInfo[i].islifeended && aiProjectileList[i].PassThroughCount > 0)
                {
                    aiProjectileList[i].Move(rv_aiProjectile_jobUpdateInfo[i].deltaMovement);
                    aiProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_aiProjectile_jobUpdateInfo[i].rotation);
                }
                if (aiProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    _aiProjectileDamageList.Add(aiProjectileList[i]);
                    aiDamageProjectile_JobInfo.Add(aiProjectile_JobInfo[i]);
                }
                //PassTrough�������͵��ӵ��� ֻҪPass CountΪ0 ����LiftTimeΪ0 �ͻ�����
                if (aiProjectileList[i].PassThroughCount <= 0 || rv_aiProjectile_jobUpdateInfo[i].islifeended)
                {
                    _aiProjectileDeathIndex.Add(i);
                }
            }

            if (aiProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Point)
            {
                //����Move �� Rotation
                if (!rv_aiProjectile_jobUpdateInfo[i].islifeended && !aiProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    aiProjectileList[i].Move(rv_aiProjectile_jobUpdateInfo[i].deltaMovement);
                    aiProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_aiProjectile_jobUpdateInfo[i].rotation);
                }
                else
                {
                    //Point�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    if (rv_aiProjectile_jobUpdateInfo[i].islifeended)
                    {
                        _aiProjectileDamageList.Add(aiProjectileList[i]);
                        aiDamageProjectile_JobInfo.Add(aiProjectile_JobInfo[i]);
                        _aiProjectileDeathIndex.Add(i);
                    }
                }
            }

            if (aiProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Target)
            {
                //����Move �� Rotation
                if (!rv_aiProjectile_jobUpdateInfo[i].islifeended && !aiProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    aiProjectileList[i].Move(rv_aiProjectile_jobUpdateInfo[i].deltaMovement);
                    aiProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_aiProjectile_jobUpdateInfo[i].rotation);
                }
                else
                {
                    if (aiProjectileList[i].IsApplyDamageAtThisFrame)
                    {
                        _aiProjectileDamageList.Add(aiProjectileList[i]);
                        aiDamageProjectile_JobInfo.Add(aiProjectile_JobInfo[i]);
                    }
                    // Target�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    _aiProjectileDeathIndex.Add(i);
                }
            }
        }
        //���������ӵ����˺��߼�
        HandleProjectileDamage();
        //����ÿ���ӵ�����Ϣ����������LifeEnd���ӵ�
        UpdateProjectileJobData();

        rv_aiProjectile_jobUpdateInfo.Dispose();
        //dispose
    }

    public void HandleProjectileDamage()
    {

        //aiProjectileDamageList = aiProjectileList.FindAll(x => x.IsApplyDamageAtThisFrame);

        if(_aiProjectileDamageList.Count == 0)
        {
            return;
        }

        rv_aiProjectileDamageTargetIndex = new NativeArray<int>(_aiProjectileDamageList.Count * playerActiveUnitList.Count, Allocator.TempJob);
        rv_aiProjectileDamageTargetCountPre = new NativeArray<int>(_aiProjectileDamageList.Count, Allocator.TempJob);
        JobHandle jobHandle;

        //�ҵ������ӵ����˺�Ŀ�꣬�����ǵ������߶���� 
        //TODO ������Խ����Ż��������ӵ��Ƿ�Ϊ��Ŀ��������Job������Ŀǰ�ķ�ʽ��ͳһ���� ��Ҫ��������߼�֮��������
        Bullet.FindBulletDamageTargetJob findBulletDamageTargetJob = new Bullet.FindBulletDamageTargetJob
        {
            job_JobInfo = aiDamageProjectile_JobInfo,
            job_targesTotalCount = playerActiveUnitList.Count,
            job_targetsPos = playerActiveUnitPos,

            rv_findedTargetsCount = rv_aiProjectileDamageTargetCountPre,
            rv_findedTargetIndex = rv_aiProjectileDamageTargetIndex,

        };
        jobHandle = findBulletDamageTargetJob.ScheduleBatch(_aiProjectileDamageList.Count, 2);
        jobHandle.Complete();


        int damagetargetindex;
        IDamageble damageble;

        //Loop���л�����˺����ӵ�List��
        for (int i = 0; i < _aiProjectileDamageList.Count; i++)
        {
            //���ö�Ӧ�ӵ���prepareDamageTargetList�������ں���ʵ��Apply Damage��׼��
            for (int n = 0; n < rv_aiProjectileDamageTargetCountPre[i]; n++)
            {
                damagetargetindex = rv_aiProjectileDamageTargetIndex[i * playerActiveUnitList.Count + n];
                if (damagetargetindex < 0 || damagetargetindex >= playerActiveUnitList.Count)
                {
                    continue;
                }
                damageble = playerActiveUnitList[damagetargetindex].GetComponent<IDamageble>();
                if (damageble != null && _aiProjectileDamageList[i] != null)
                {
                    if (!_aiProjectileDamageList[i].prepareDamageTargetList.Contains(damageble))
                    {
                        _aiProjectileDamageList[i].prepareDamageTargetList.Add(damageble);
                    }
                }
            }
            //Apply Damage to every damage target
            _aiProjectileDamageList[i].ApplyDamageAllTarget();
        }

        //damageProjectile_JobInfo.Dispose();
        rv_aiProjectileDamageTargetCountPre.Dispose();
        rv_aiProjectileDamageTargetIndex.Dispose();
    }

    public void UpdateProjectileJobData()
    {
        //������Ҫ�ȸ����ӵ�����Ϣ��Ȼ���ڰ��������ӵ��Ƴ��� ����rv aibullet�ľ�̬���ݳ����޷�����
        //��Ȼ���˷������������ǿ��Ա�֤index�����λ
        ProjectileJobInitialInfo bulletJobInfo;
        for (int i = 0; i < aiProjectileList.Count; i++)
        {
            if (rv_aiProjectile_jobUpdateInfo[i].islifeended)
                continue;
            //bulletJobInfo ��һ��ֵ���͡� ֻ��ʹ�����ַ�ʽ���¡�
            bulletJobInfo = aiProjectile_JobInfo[i];

            bulletJobInfo.update_targetPos = aiProjectileList[i].initialTarget? aiProjectileList[i].initialTarget.transform.position : aiProjectileList[i].transform.up;
            bulletJobInfo.update_selfPos = aiProjectileList[i].transform.position;
            bulletJobInfo.update_lifeTimeRemain = rv_aiProjectile_jobUpdateInfo[i].lifeTimeRemain;
            bulletJobInfo.update_moveDirection = rv_aiProjectile_jobUpdateInfo[i].moveDirection;
            bulletJobInfo.update_currentSpeed = rv_aiProjectile_jobUpdateInfo[i].currentSpeed;

            aiProjectile_JobInfo[i] = bulletJobInfo;
        }

        //���һ�� ִ���ӵ�����
        _aiProjectileDeathList.Clear();
        int deathindex = 0;
        for (int i = 0; i < _aiProjectileDeathIndex.Count; i++)
        {
            deathindex = _aiProjectileDeathIndex[i];
            _aiProjectileDeathList.Add(aiProjectileList[deathindex]);
            //aiProjectileList[deathindex].Death(null);
        }
        for (int i = 0; i < _aiProjectileDeathList.Count; i++)
        {
            _aiProjectileDeathList[i].Death(null);
        }
    }

    public List<Unit> GetRandomAIUnitList( int count)
    {
        List<Unit> randomlist = new List<Unit>() ;
        System.Random rnd = new System.Random();
        int randIndex;
        for (int i = 0; i < count; i++)
        {
            randIndex = rnd.Next(aiActiveUnitList.Count);
            if (!randomlist.Contains(aiActiveUnitList[randIndex]))
            {
                randomlist.Add(aiActiveUnitList[randIndex]);
            }
            else
            {
                i--;
            }
        }
        return randomlist;
    }

    public Unit GetUnitByUID(uint uid)
    {
        return aiActiveUnitList.Find(x => x.UID == uid);
    }

    public Unit GetAIUnitWithCondition(FindCondition condition)
    {
        Unit unit;
        if( condition == FindCondition.MaximumHP)
        {
            unit = aiActiveUnitList.OrderBy(u => u.HpComponent.GetCurrentHP).Last();
            return unit;
        }
        if(condition == FindCondition.MinimumHP)
        {
            unit = aiActiveUnitList.OrderBy(u => u.HpComponent.GetCurrentHP).First();
            return unit;
        }
        return null;
    }

    public Unit GetAIUnitWithCondition(FindCondition condition, Vector3 referencePos)
    {
        Unit unit;

        if(condition == FindCondition.ClosestDistance)
        {
            unit = aiActiveUnitList.OrderBy(u => math.distance(u.transform.position, referencePos)).First();
            return unit;
        }
        if(condition == FindCondition.LongestDistance)
        {
            unit = aiActiveUnitList.OrderBy(u => math.distance(u.transform.position, referencePos)).Last();
            return unit;
        }

        return null;
    }

    public void PauseGame()
    {
        
    }

    public void UnPauseGame()
    {
        
    }

    private void OnEnemyCountChange()
    {
        LevelManager.Instance.EnemyShipCountChange(ShipCount);
    }
}
