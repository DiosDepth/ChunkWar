using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeBehavior : SteeringBehavior
{
    [SerializeField] public float maxPrediction = 2f;



    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    Vector3 direction = target.position - transform.position;
    //    float distance = direction.magnitude;
    //    float speed = GetComponent<IBoid>().GetVelocity().magnitude;
    //    float prediction;
    //    if (speed <= (distance / maxPrediction)) prediction = maxPrediction;
    //    else prediction = distance / speed;
    //    //������Ҫ��ȡTarget�ĵ�ǰ�ƶ����� Ҳ����Velocity,����ͨ��target��Rigibody��ȡ����ʹ��������������
    //    //��ʱʹ��Vector3.up �����������

    //    Vector3 predictedTarget = target.position + (new Vector3(target.GetComponent<IBoid>().GetVelocity().x, target.GetComponent<IBoid>().GetVelocity().y, 0) * prediction);
    //    steering.linear = transform.position - predictedTarget ;
    //    steering.linear.Normalize();
    //    steering.linear *= steeringcontroller.maxAcceleration;
    //    steering.angular = 0;
    //    return steering;
    //}

}
