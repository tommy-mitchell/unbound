using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ParallaxController : MonoBehaviour
{
    [SerializeField]
    private Transform _player;
    
    private List<Parallax> backgrounds => new List<Parallax>(gameObject.GetComponentsInChildren<Parallax>());

    public List<Sprite> sprites;

    public Parallax _prefab;

    [Button]
    public void BuildPrefabs()
    {
        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);

        float distance = 38f;

        foreach(Sprite sprite in sprites)
        {
            Parallax newLayer = Instantiate(_prefab);
            SpriteRenderer _renderer = newLayer.GetComponent<SpriteRenderer>();

            newLayer.name = sprite.name;
            newLayer.transform.SetParent(this.transform);
            _renderer.sprite = sprite;
            newLayer.cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            newLayer.subject = _player;
            newLayer.infiniteLoop = false;
            
            if(sprite.name.Contains("Collision"))
            {
                //newLayer.transform.position.Set(0, 0, 0);
                _renderer.sortingLayerName = "Default";
                newLayer.tag = "Ground";
                newLayer.gameObject.layer = 3; // ground
                newLayer.gameObject.AddComponent<PolygonCollider2D>().usedByComposite = true;
                newLayer.gameObject.AddComponent<CompositeCollider2D>();
                newLayer.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                newLayer.gameObject.AddComponent<PixelPerfectCollider2D>().Regenerate();
                newLayer.GetComponent<Parallax>().enabled = false;

                _renderer.enabled = false;
            }
            else if(sprite.name.EndsWith("Ground"))
                newLayer.GetComponent<Parallax>().enabled = false;
            else if(sprite.name.Contains("Foreground"))
            {
                newLayer.transform.localPosition.Set(0, 0, -1);
                _renderer.sortingLayerName = "Foreground";
            }
            else
            {
                newLayer.transform.position = new Vector3(0, 0, distance);
                //newLayer.transform.localPosition.Set(0, 0, distance);
                distance /= 2.0f;
                //distance /= 2.718f;
                //distance = Mathf.Sqrt(distance);
            }
        }
    }
}
