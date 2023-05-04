using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ParticleSystemJobs;
public class Projectile : PoolableObject
{
    public Rigidbody2D rb;
    public float lifeTime = 10;
    public float speed = 2.5f;
    public Vector3 moveDirection = Vector3.right;

    private Coroutine startmovingCoroutine;
    private float _timestamp;
    private Vector3 _move;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator StartMoving(UnityAction callback)
    {
        _timestamp = Time.time + lifeTime;

        while(Time.time < _timestamp)
        {
            Vector2 move = transform.position + (moveDirection * speed * Time.deltaTime);
          
            rb.MovePosition(move);
            yield return null;
        }

        callback?.Invoke();
    }

    public override void ResetSelf()
    {
        base.ResetSelf();
    }

    public override void SetActive(bool isactive = true)
    {
        base.SetActive(isactive);

        startmovingCoroutine = StartCoroutine(StartMoving(() =>
        {
            Destroy();
        }));

    }

    public override void Destroy()
    {
        base.Destroy();
        StopCoroutine(startmovingCoroutine);

    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        PoolManager.Instance.GetObject(PoolManager.Instance.VFXPath + "DestroyVFX", true, (obj) => 
        {
            obj.transform.position = this.transform.position;
            obj.GetComponent<ParticleController>().SetActive();
            obj.GetComponent<ParticleController>().PlayVFX();

        });
        Destroy();
    }
}
