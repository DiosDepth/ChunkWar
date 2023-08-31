using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class AIRuntimeData 
{
    public int Count { get { return aiShipList.Count; } }
    public List<AIShip> aiShipList ;
    public List<AISteeringBehaviorController> aiSteeringBehaviorControllerList;
    public List<IBoid> aiShipBoidList;

    public List<ArriveBehavior> arriveBehavior;
    public List<FaceBehavior> faceBehavior;
    public List<CohesionBehavior> cohesionBehavior;
    public List<SeparationBehavior> separationBehavior;
    public List<AlignmentBehavior> aligmentBehavior;

    //public List<NativeArray<float3>> arrivebehavior_steeringlinear;
    //public List<NativeArray<float3>> facebehavior_steeringangle;
    //public List<NativeArray<float3>> cohesionbehavior_steeringlinear;
    //public List<NativeArray<float3>> separationbehavior_steeringlinear;
    //public List<NativeArray<float3>> alignmentbehavior_steeringlinear;


    public AIRuntimeData()
    {
        aiShipList = new List<AIShip>();
        aiShipBoidList = new List<IBoid>();
        aiSteeringBehaviorControllerList = new List<AISteeringBehaviorController>();
        arriveBehavior = new List<ArriveBehavior>();
        faceBehavior = new List<FaceBehavior>();
        cohesionBehavior = new List<CohesionBehavior>();
        separationBehavior = new List<SeparationBehavior>();
        aligmentBehavior = new List<AlignmentBehavior>();

        //arrivebehavior_steeringlinear = new List<NativeArray<float3>>();
        //facebehavior_steeringangle = new List<NativeArray<float3>>();
        //cohesionbehavior_steeringlinear = new List<NativeArray<float3>>();
        //separationbehavior_steeringlinear = new List<NativeArray<float3>>();
        //alignmentbehavior_steeringlinear = new List<NativeArray<float3>>();
    }

    ~AIRuntimeData()
    {
        ClearAIData();
    }

    public void AddAIDataRange(List<AIShip> list)
    {

        for (int i = 0; i < list.Count; i++)
        {
            AddAIData(list[i]);
            //aiShipBoidList.Add(list[i].GetComponent<IBoid>());
            //AISteeringBehaviorController controller = list[i].GetComponent<AISteeringBehaviorController>();
            //controller.Initialization();
            //aiSteeringBehaviorControllerList.Add(controller);
            //arriveBehavior.Add(list[i].GetComponent<ArriveBehavior>());
            //faceBehavior.Add(list[i].GetComponent<FaceBehavior>());
            //cohesionBehavior.Add(list[i].GetComponent<CohesionBehavior>());
            //separationBehavior.Add(list[i].GetComponent<SeparationBehavior>());
            //aligmentBehavior.Add(list[i].GetComponent<AlignmentBehavior>());

        
           // arrivebehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));
           // facebehavior_steeringangle.Add(new NativeArray<float3>(1, Allocator.Persistent));
           //cohesionbehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));
           // separationbehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));
           // alignmentbehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));

        }
        //aiShipList.AddRange(list);
        
    }
    public void AddAIData(AIShip ship)
    {

        aiShipList.Add(ship);
        aiShipBoidList.Add(ship.GetComponent<IBoid>());
        AISteeringBehaviorController controller = ship.GetComponent<AISteeringBehaviorController>();
        controller.Initialization();
        aiSteeringBehaviorControllerList.Add(controller);
        arriveBehavior.Add(ship.GetComponent<ArriveBehavior>());
        faceBehavior.Add(ship.GetComponent<FaceBehavior>());
        cohesionBehavior.Add(ship.GetComponent<CohesionBehavior>());
        separationBehavior.Add(ship.GetComponent<SeparationBehavior>());
        aligmentBehavior.Add(ship.GetComponent<AlignmentBehavior>());


        //arrivebehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));
        //facebehavior_steeringangle.Add(new NativeArray<float3>(1, Allocator.Persistent));
        //cohesionbehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));
        //separationbehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));
        //alignmentbehavior_steeringlinear.Add(new NativeArray<float3>(1, Allocator.Persistent));

    }

    public void RemoveAIData(AIShip ship)
    {
        int index = aiShipList.IndexOf(ship);

        RemoveAIData(index);

    }

    public void RemoveAIData(int index)
    {
        aiShipList.RemoveAt(index);
        aiShipBoidList.RemoveAt(index);
        aiSteeringBehaviorControllerList.RemoveAt(index);
        arriveBehavior.RemoveAt(index);
        faceBehavior.RemoveAt(index);
        cohesionBehavior.RemoveAt(index);
        separationBehavior.RemoveAt(index);
        aligmentBehavior.RemoveAt(index);


        //arrivebehavior_steeringlinear[index].Dispose();
        //arrivebehavior_steeringlinear.RemoveAt(index);

        //facebehavior_steeringangle[index].Dispose();
        //facebehavior_steeringangle.RemoveAt(index);

        //cohesionbehavior_steeringlinear[index].Dispose();
        //cohesionbehavior_steeringlinear.RemoveAt(index);

        //separationbehavior_steeringlinear[index].Dispose();
        //separationbehavior_steeringlinear.RemoveAt(index);

        //alignmentbehavior_steeringlinear[index].Dispose();
        //alignmentbehavior_steeringlinear.RemoveAt(index);

    }

    public void ClearAIData()
    {
       
        aiShipList.Clear();
        aiShipBoidList.Clear();
        aiSteeringBehaviorControllerList.Clear();
        arriveBehavior.Clear();
        faceBehavior.Clear();
        cohesionBehavior.Clear();
        separationBehavior.Clear();
        aligmentBehavior.Clear();

        //for (int i = 0; i < arrivebehavior_steeringlinear.Count; i++)
        //{
        //    arrivebehavior_steeringlinear[i].Dispose();
        //    facebehavior_steeringangle[i].Dispose();
        //    cohesionbehavior_steeringlinear[i].Dispose();
        //    separationbehavior_steeringlinear[i].Dispose();
        //    alignmentbehavior_steeringlinear[i].Dispose();
        //}
        //arrivebehavior_steeringlinear.Clear();
        //facebehavior_steeringangle.Clear();
        //cohesionbehavior_steeringlinear.Clear();
        //separationbehavior_steeringlinear.Clear();
        //alignmentbehavior_steeringlinear.Clear();
    }


}
