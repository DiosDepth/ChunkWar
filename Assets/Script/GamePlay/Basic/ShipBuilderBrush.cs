using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBuilderBrush : MonoBehaviour
{

    public SpriteRenderer[,] shadows = new SpriteRenderer[GameGlobalConfig.BuildingShadowMaxSize,GameGlobalConfig.BuildingShadowMaxSize];
    public SpriteRenderer brushSprite;


    public Transform ShadowGroup;
    public Sprite shadowSprite;


    public Color defaultColor;
    public Color validColor;
    public Color invalidColor;

    public void ValidState()
    {

    }
    public void InvalidState()
    {

    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public void Initialization()
    {
        GameObject obj;
        for (int x = 0; x < shadows.GetLength(0); x++)
        {
            for (int y = 0; y < shadows.GetLength(1); y++)
            {
                obj = new GameObject("Shadow_");
                obj.transform.parent = ShadowGroup;
                obj.transform.localPosition = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.BuildingShadowMapSize).ToVector3();
                obj.name += obj.transform.localPosition.ToString();
                shadows[x, y] = obj.AddComponent<SpriteRenderer>();
                shadows[x, y].sprite = shadowSprite;
                shadows[x, y].color = defaultColor;
                shadows[x, y].sortingLayerName = "Sprite";
                shadows[x, y].sortingOrder = 100;
                obj.SetActive(false);
            }
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

    public void ChangeShadowColor(Color color)
    {
        for (int x = 0; x < shadows.GetLength(0); x++)
        {
            for (int y = 0; y < shadows.GetLength(1); y++)
            {
                shadows[x, y].color = color;
            }
        }
    }
    public void UpdateShadows(Vector2Int[] map)
    {
        Vector2Int coord;

        for (int x = 0; x < shadows.GetLength(0); x++)
        {
            for (int y = 0; y < shadows.GetLength(1); y++)
            {
                shadows[x, y].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < map.Length; i++)
        {
            coord =  GameHelper.CoordinateMapToArray(map[i], GameGlobalConfig.BuildingShadowMapSize);
            shadows[coord.x, coord.y].gameObject.SetActive(true);
        }
      
    }
}
