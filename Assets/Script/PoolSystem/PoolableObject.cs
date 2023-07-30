using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PoolableObject : MonoBehaviour
{
    //public ObjectPoolerHolder ownerPooler;
    //public string targetPoolerName;
    // Start is called before the first frame update
    protected virtual void OnEnable()
    {
       
    }
    protected virtual void Start()
    {
       
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }


    public virtual void Reset()
    {

    }

    public virtual void Initialization()
    {

    }

    public virtual void Destroy()
    {
        Reset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public virtual void SetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }
}
