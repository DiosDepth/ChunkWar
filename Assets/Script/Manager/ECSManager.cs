using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ECSManager : Singleton<ECSManager>
{

    public PlayerJobController playerJobController;
    public AIJobController AIJobController;
    public override void Initialization()
    {
        base.Initialization();
        playerJobController = new PlayerJobController();
        AIJobController = new AIJobController();
        MonoManager.Instance.AddUpdateListener(Update);
        MonoManager.Instance.AddFixedUpdateListener(FixedUpdate);
    }


    public virtual void Update()
    {
        playerJobController.UpdateJobs();
        AIJobController.UpdateJobs();
    }

    public virtual void FixedUpdate()
    {
        playerJobController.FixedUpdateJobs();
        AIJobController.FixedUpdateJobs();
    }

}
