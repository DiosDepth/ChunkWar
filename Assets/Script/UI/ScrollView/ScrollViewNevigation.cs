using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(ScrollView))]
public class ScrollViewNevigation : MonoBehaviour
{
    public float _speed = 0.3f;
    private ScrollRect _rect;
    private RectTransform viewport;
    private RectTransform content;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        if (_rect == null)
        {
            _rect = GetComponent<ScrollRect>();
        }

        if (viewport == null)
        {
            viewport = transform.Find("ViewPort").GetComponent<RectTransform>();
        }
        if (content == null)
        {
            content = transform.Find("ViewPort/Content").GetComponent<RectTransform>();
        }
    }

    public void Nevigate(RectTransform item)
    {
        Vector3 itemCurrentLocalPostion = _rect.GetComponent<RectTransform>().InverseTransformVector(ConvertLocalPosToWorldPos(item));
        Vector3 itemTargetLocalPos = _rect.GetComponent<RectTransform>().InverseTransformVector(ConvertLocalPosToWorldPos(viewport));
        Vector3 diff = itemTargetLocalPos - itemCurrentLocalPostion;

        if (diff == Vector3.zero)
            return;

        diff.z = 0.0f;
        var newNormalizedPosition = new Vector2(
          diff.x / (content.GetComponent<RectTransform>().rect.width - viewport.rect.width),
          diff.y / (content.GetComponent<RectTransform>().rect.height - viewport.rect.height)
          );

        newNormalizedPosition = _rect.GetComponent<ScrollRect>().normalizedPosition - newNormalizedPosition;
        newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);

        newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

        //DOTween.To(() => _rect.normalizedPosition, x => _rect.normalizedPosition = x, newNormalizedPosition, _speed);

    }

    Vector3 ConvertLocalPosToWorldPos(RectTransform target)
    {

        var pivotOffset = new Vector3(
          (0.5f - target.pivot.x) * target.rect.size.x,
          (0.5f - target.pivot.y) * target.rect.size.y,
          0f);
        var localPosition = target.localPosition + pivotOffset;
        return target.parent.TransformPoint(localPosition);

    }
}