using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PoolableObject : MonoBehaviour
{
    //public ObjectPoolerHolder ownerPooler;
    //public string targetPoolerName;
    // Start is called before the first frame update
    private void OnEnable()
    {
       
    }
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public virtual void ResetSelf()
    {

    }

    public virtual void StartSelf()
    {

    }

    public virtual void Destroy()
    {
        ResetSelf();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public virtual void SetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }
}
