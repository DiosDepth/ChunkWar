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




    // Start is called before the first frame update
    void Start()
    {
 

    }


    public void ValidState()
    {

    }
    public void InvalidState()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
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


    public void ChangeBurshSprite(Sprite m_sprite)
    {
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
