using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class MonoBehaviorExtention
{
    #region Tranfrom
    public static float MagnitudeXZ(this Vector3 v3)
    {
        v3.y = 0;
        return v3.magnitude;
    }

    public static Vector3 NormalizedXZ(this Vector3 v3)
    {
        v3.y = 0;
        return v3.normalized;
    }

    public static void SetLocalPositionXY(this Transform trans, float x, float y)
    {
        Vector3 newPos = new Vector3(x, y, trans.localPosition.z);
        trans.localPosition = newPos;
    }

    public static void SetLocalPositionX(this Transform trans, float x)
    {
        Vector3 newPos = new Vector3(x, trans.localPosition.y, trans.localPosition.z);
        trans.localPosition = newPos;
    }

    public static void SetLocalPositionY(this Transform trans, float y)
    {
        Vector3 newPos = new Vector3(trans.localPosition.x, y, trans.localPosition.z);
        trans.localPosition = newPos;
    }

    public static void SetPositionX(this Transform trans, float x)
    {
        Vector3 newPos = new Vector3(x, trans.position.y, trans.position.z);
        trans.position = newPos;
    }

    public static void SetPositionXZ(this Transform trans, Vector3 pos)
    {
        var y = trans.position.y;
        Vector3 newPos = new Vector3(pos.x, y, pos.z);
        trans.position = newPos;
    }

    public static void SetPositionY(this Transform trans, float y)
    {
        Vector3 newPos = new Vector3(trans.position.x, y, trans.position.z);
        trans.position = newPos;
    }

    public static void SetLocalRotationY(this Transform trans, float y)
    {
        Vector3 q = new Vector3(trans.localEulerAngles.x, y, trans.localEulerAngles.z);
        trans.localEulerAngles = q;
    }

    public static void SetLocalScaleX(this Transform trans, float x)
    {
        Vector3 s = new Vector3(x, trans.localScale.y, trans.localScale.z);
        trans.localScale = s;
    }

    public static void ChangeQuaternion(this Quaternion q, Vector3 v3)
    {
        var offset = Quaternion.Euler(v3);
        q.x = q.x + offset.x;
        q.y = q.y + offset.y;
        q.z = q.z + offset.z;
        q.w = q.w + offset.w;
    }

    public static Vector3 GetTransformScreenPos(this Transform trans)
    {
        return Camera.main.WorldToScreenPoint(trans.position);
    }

    public static T SafeGetComponent<T>(this Transform trans) where T : class
    {
        T result = null;
        try
        {
            result = trans.GetComponent<T>();
        }
        catch
        {
            if (result == null && trans != null)
            {
                Debug.LogError("Get Component is null , trans=" + trans.name);
            }
            else if (trans == null)
            {
                Debug.LogError("Transform is null");
            }
        }
        return result;
    }

    public static Transform FindTransfrom(this Transform transfrom, string name)
    {
        var trans = transfrom.Find(name);
        if (trans == null)
        {
            Debug.LogError("Find Transfrom is null !  name= " + name);
        }
        return trans;
    }

    public static T AttachCmpt<T>(this Transform transform) where T : Component
    {
        T result = null;
        if (transform != null)
        {
            if (transform.SafeGetComponent<T>() == null)
            {
                result = transform.gameObject.AddComponent<T>();
                return result;
            }
            else
            {
                return transform.SafeGetComponent<T>();
            }
        }
        return null;
    }

    public static bool SafeSetActive(this Transform trans, bool active)
    {
        if (trans != null)
        {
            if (trans.gameObject.activeSelf != active)
            {
                trans.gameObject.SetActive(active);
            }
            return true;
        }
        return false;
    }

    public static void SafeSetActiveAllChild(this Transform trans, bool active)
    {
        if (trans != null)
        {
            foreach (Transform t in trans)
            {
                t.SafeSetActive(active);
            }
        }
    }

    /// <summary>
    /// Reset Transform
    /// </summary>
    /// <param name="trans"></param>
    public static void TransIdentity(this Transform trans)
    {
        trans.localPosition = Vector3.zero;
        trans.localScale = Vector3.one;
        trans.localRotation = Quaternion.identity;
    }

    public static void TransIdentityUI(this Transform trans)
    {
        var rect = trans.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector3.zero;
            rect.position = Vector3.zero;
        }
    }

    #endregion
    #region Rect

    public static Vector2 GetContentSizeFitterPreferredSize(this RectTransform rect, ContentSizeFitter fitter)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        return new Vector2(HandleSelfFittingAlongAxis(0, rect, fitter), HandleSelfFittingAlongAxis(1, rect, fitter));
    }

    private static float HandleSelfFittingAlongAxis(int axis, RectTransform rect, ContentSizeFitter fitter)
    {
        ContentSizeFitter.FitMode fitting = (axis == 0 ? fitter.horizontalFit : fitter.verticalFit);
        if (fitting == ContentSizeFitter.FitMode.MinSize)
        {
            return LayoutUtility.GetMinSize(rect, axis);
        }
        else
        {
            return LayoutUtility.GetPreferredSize(rect, axis);
        }
    }



    public static void SetRectWidth(this RectTransform rect, float width)
    {
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
        }
    }

    public static void SetRectWidth(this Transform trans, float width)
    {
        var rect = trans.SafeGetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
        }
    }

    public static void SetRectHeight(this RectTransform rect, float height)
    {
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        }
    }
    public static void SetRectWidthAndHeight(this RectTransform rect, float width, float height)
    {
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(width, height);
        }
    }

    public static Vector2 TopLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }
    public static Rect ScaleSizeBy(this Rect rect, float scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }
    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale.x;
        result.xMax *= scale.x;
        result.yMin *= scale.y;
        result.yMax *= scale.y;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    #endregion

    #region Canvas  
    public static void ActiveCanvasGroup(this CanvasGroup group, bool active)
    {
        if (group == null)
        {
            Debug.LogError("CanvasGroup is Null!");
            return;
        }
        if (active)
        {
            group.alpha = 1;
        }
        else
        {
            group.alpha = 0;
        }
        group.interactable = active;
        group.blocksRaycasts = active;
    }
    #endregion

    public static void Pool_BackAllChilds(this Transform trans, string key)
    {
        foreach(Transform child in trans)
        {
            PoolManager.Instance.BackObject(key, child.gameObject);
        }
    }

}