using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralHoverItemControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public IHoverUIItem item;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null)
            return;
        item.OnHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (item == null)
            return;
        item.OnHoverExit();
    }
}


public interface IHoverUIItem
{
    void OnHoverEnter();

    void OnHoverExit();
}
