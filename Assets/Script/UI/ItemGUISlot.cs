using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemGUISlot : MonoBehaviour
{ 
    public int slotIndex;
    public Image Icon; 
    public Button slotbtn;
    public InventoryItem item;
    public RectTransform canvas;
  
    private Vector2 _originalLocalPointerPosition;
    private Vector3 _originalPanelLocalPosition;
    private Vector2 _originalPosition;
    // Start is called before the first frame update


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Initialization(int m_index, RectTransform m_canvas, InventoryItem m_item = null)
    {

        slotIndex = m_index;
        canvas = m_canvas;

        item = m_item;

        Icon.sprite = item.itemconfig.Icon;
        slotbtn.onClick.AddListener(OnInventoryBuildingBtnClicked);
    }

    public void OnInventoryBuildingBtnClicked()
    {
        InventoryOperationEvent.Trigger(InventoryOperationType.ItemSelect, slotIndex, item.itemconfig.UnitName);
    }
    /* 
     public void OnBeginDrag(PointerEventData eventData)
     {
         _originalPanelLocalPosition = dragElement.localPosition;
         RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, eventData.position, eventData.pressEventCamera, out _originalLocalPointerPosition);
     }

     public void OnDrag(PointerEventData eventData)
     {
         Vector2 localPointPosition;
         if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, eventData.position, eventData.pressEventCamera, out localPointPosition))
         {
             Vector3 offsetToOriginal = localPointPosition - _originalLocalPointerPosition;
             dragElement.localPosition = _originalPanelLocalPosition + offsetToOriginal;
         }
     }

     public void OnEndDrag(PointerEventData eventData)
     {
         dragElement.localPosition = _originalPanelLocalPosition;
     }
     */

}
