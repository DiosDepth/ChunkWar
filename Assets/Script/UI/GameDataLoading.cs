using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class LoadingCompletedEvent : UnityEvent
{

}

public class GameDataLoading : MonoBehaviour
{
   
    public Slider progressBar;
    public bool loadingCompleted = false;
    [SerializeField]
    public LoadingCompletedEvent OnLoadingCompleted;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(DataLoading());
    }

    // Update is called once per frame
    void Update()
    {

    }
    /*IEnumerator DataLoading()
    {
        StartCoroutine(DataManager.Instance.LoadAllData());
        while(!loadingCompleted)
        {
            progressBar.value = DataManager.Instance.GetLoadingRate();
            //progressBar.value += Time.deltaTime * 0.1f;
            if(progressBar.value >= 1)
            {
                loadingCompleted = true;
                yield return new WaitForSeconds(1);
                OnLoadingCompleted.Invoke();
            }
            yield return null;
        }
        loadingCompleted = false;
        yield return null;
    }*/

}
