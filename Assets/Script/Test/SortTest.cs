using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortTest : MonoBehaviour
{
    public string[] targetString = new string[10];
    public int[] targetInt;
    // Start is called before the first frame update
    void Start()
    {


        for (int i = 0; i < targetString.Length; i++)
        {
            Debug.Log(targetString[i]);
        }

        QuickSort(ref targetString);

        for (int i = 0; i < targetString.Length; i++)
        {
            Debug.Log("Sort : " + targetString[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Sort(ref int[] arr, int startindex, int endindex)
    {
        if(startindex < endindex)
        {
            int mid = Partition(ref arr, startindex, endindex);
            Sort(ref arr, startindex, mid - 1);
            Sort(ref arr, mid + 1, endindex);
        }
    }


    public void Sort(ref string[] arr, int startindex, int endindex)
    {
        if (startindex < endindex)
        {
            int mid = Partition(ref arr, startindex, endindex);
            Sort(ref arr, startindex, mid - 1);
            Sort(ref arr, mid + 1, endindex);
        }
    }




    public void QuickSort(ref int[] arr)
    {
        Sort(ref arr, 0, arr.Length - 1);
    }

    public void QuickSort(ref string[] arr)
    {
        Sort(ref arr, 0, arr.Length - 1);
    }


    public int Partition(ref string[] arr, int startindex, int endindex)
    {
         int pivot;
         int left;
         int right;

        pivot = endindex;
        right = endindex - 1;
        left = startindex;

        while(true)
        {
            while (string.Compare(arr[left], arr[pivot]) < 0) { left++; }
            while (string.Compare(arr[right], arr[pivot]) > 0)
            {
                right--;
                if (right <= left)
                {
                    break;
                }
            }
            if(right > left)
            {
                Swap(ref arr[right], ref arr[left]);
            }
            else
            {
                break;
            }
        }
        Swap(ref arr[left], ref arr[pivot]);
        return left;
    }

    public int Partition(ref int[] arr, int startindex, int endindex)
    {
        int pivot;
        int left;
        int right;

        pivot = endindex;
        right = endindex - 1;
        left = startindex;

        while (true)
        {
            while (arr[left] < arr[pivot]) { left++; }
            while (arr[right] > arr[pivot])
            {
               
                right--;
                if(right <=left)
                {
                    break;
                }
            }
            if (right > left)
            {
                Swap(ref arr[right], ref arr[left]);
            }
            else
            {
                break;
            }
        }
        Swap(ref arr[left], ref arr[pivot]);
        return left;
       
    }


    private void Swap(ref string a, ref string b)
    {
        string temp;
        temp = a;
        a = b;
        b = temp;
    }

    private void Swap(ref int a, ref int b)
    {
        int temp;
        temp = a;
        a = b;
        b = temp;
    }


}
