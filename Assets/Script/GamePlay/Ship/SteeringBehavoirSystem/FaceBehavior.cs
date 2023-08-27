using System.Collections;
using System.Collections.Generic;
using UnityEngine;

  public class FaceBehavior : SteeringBehavior
    {
        public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
        {
            SteeringData steering = new SteeringData();
            Vector3 velocity = GetComponent<IBoid>().GetVelocity();
            float angle;
        if (velocity.magnitude == 0)
        {
            angle = (Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg) - 90;
            steering.angular = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, steeringcontroller.maxAngularAcceleration * Time.deltaTime);
            return steering;
        }
    

            angle = (Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg)- 90;
            steering.angular = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, steeringcontroller.maxAngularAcceleration * Time.deltaTime);
            steering.linear = Vector3.zero;
            return steering;
        }
    
    }
