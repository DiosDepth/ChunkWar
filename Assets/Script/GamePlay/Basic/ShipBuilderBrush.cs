using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBuilderBrush : MonoBehaviour
{
    public enum BrushState
    {
        Vaild,
        Error
    }

    private SpriteRenderer brushSprite;

    private Color validColor = Color.white;
    private Color invalidColor = new Color (186,0,0,116);

    private void Awake()
    {
        brushSprite = transform.Find("Sprite").SafeGetComponent<SpriteRenderer>();
        brushSprite.color = validColor;
    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public void SetDirection(int dir)
    {
        transform.rotation = Quaternion.Euler(0, 0, -90 * dir);
    }

    public void Initialization()
    {
    }

    public void SetBrushState(BrushState state)
    {
        if(state == BrushState.Error)
        {
            brushSprite.color = invalidColor;
        }
        else if (state == BrushState.Vaild)
        {
            brushSprite.color = validColor;
        }
    }

    public void ActiveBrush(bool active)
    {
        transform.SafeSetActive(active);
    }

    public void ResetBrush()
    {
        brushSprite.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void ChangeBurshSprite(Sprite m_sprite, int defaultRotation = 0)
    {
        brushSprite.transform.SetLocalRotationZ(defaultRotation);
        if(m_sprite != null)
        {
            brushSprite.sprite = m_sprite;
        }
    }

}
