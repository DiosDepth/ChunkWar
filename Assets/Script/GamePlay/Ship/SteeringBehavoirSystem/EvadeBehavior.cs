using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeBehavior : SteeringBehavior
{
    [SerializeField] private float maxPrediction = 2f;

    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        float speed = GetComponent<IBoid>().GetVelocity().magnitude;
        float prediction;
        if (speed <= (distance / maxPrediction)) prediction = maxPrediction;
        else prediction = distance / speed;
        //这里需要获取Target的当前移动速率 也就是Velocity,可以通过target的Rigibody获取或者使用其他方法计算
        //暂时使用Vector3.up 方向替代计算

        Vector3 predictedTarget = target.position + (new Vector3(target.GetComponent<IBoid>().GetVelocity().x, target.GetComponent<IBoid>().GetVelocity().y, 0) * prediction);
        steering.linear = transform.position - predictedTarget ;
        steering.linear.Normalize();
        steering.linear *= steeringcontroller.maxAcceleration;
        steering.angular = 0;
        return steering;
    }

}
