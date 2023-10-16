using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class AIJobController : JobController
{

    public AIJobController()
    {

    }

    ~ AIJobController()
    {

    }

    public override void Initialization()
    {
        base.Initialization();
    }

    public virtual void UpdateAIAgentMovement(ref AgentData activeAIAgentData, ref IBoid playerBoid)
    {
        if (activeAIAgentData.shipList == null || activeAIAgentData.shipList.Count == 0)
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

        activeAIAgentData.rv_evade_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeAIAgentData.shipList.Count, Allocator.TempJob);

        activeAIAgentData.rv_arrive_isVelZero = new NativeArray<bool>(activeAIAgentData.shipList.Count, Allocator.TempJob);
        activeAIAgentData.rv_arrive_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeAIAgentData.shipList.Count, Allocator.TempJob);

        activeAIAgentData.rv_face_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeAIAgentData.shipList.Count, Allocator.TempJob);
        activeAIAgentData.rv_cohesion_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeAIAgentData.shipList.Count, Allocator.TempJob);
        activeAIAgentData.rv_separation_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeAIAgentData.shipList.Count, Allocator.TempJob);
        activeAIAgentData.rv_alignment_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeAIAgentData.shipList.Count, Allocator.TempJob);
        activeAIAgentData.rv_collisionavoidance_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeAIAgentData.shipList.Count, Allocator.TempJob);



        EvadeBehavior.EvadeBehaviorJob evadeBehaviorJob = new EvadeBehavior.EvadeBehaviorJob
        {
            job_boidData = activeAIAgentData.boidAgentJobData,
            job_steeringControllerData = activeAIAgentData.steeringControllerJobDataNList,
            job_evadeTargetPos = playerBoid.GetPosition(),
            job_evadeTargetVel = playerBoid.GetVelocity(),

            rv_steering = activeAIAgentData.rv_evade_steeringInfo,

        };
        jobhandle_evadebehavior = evadeBehaviorJob.ScheduleBatch(activeAIAgentData.shipList.Count, 2);
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
            job_maxAngularVelocity = aiSteeringBehaviorController_aiShipMaxAngularVelocity,
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


        SteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob calculateSteeringTargetsPosByRadiusJob = new SteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob
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
        SteeringBehaviorController.CalculateDeltaMovePosJob calculateDeltaMovePosJob = new SteeringBehaviorController.CalculateDeltaMovePosJob
        {
            job_aiShipMaxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_aiShipMaxVelocity = aiStearingBehaviorController_aiShipMaxVelocity,
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
            activeSelfSteeringAgentControllerList[i].UpdateIBoid();

            activeSelfSteeringAgentControllerList[i].Move(rv_deltaMovement[i].linear);
            activeSelfSteeringAgentControllerList[i].transform.rotation = Quaternion.Euler(0, 0, rv_deltaMovement[i].angular);
        }

        playerBoid.UpdateIBoid();

        activeAIAgentData.DisposeReturnValue();
    }


}
