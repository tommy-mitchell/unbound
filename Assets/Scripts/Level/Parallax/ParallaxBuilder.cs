using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using LevelBuilder;

public class ParallaxBuilder : MonoBehaviour
{
    public void BuildPrefabs(Level level)
    {
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        Parallax  _prefab = (UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Parallax Sprite.prefab", (typeof(GameObject))) as GameObject).GetComponent<Parallax>();
        Transform _player = GameObject.FindGameObjectWithTag("Player").transform;
        Camera    _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        List<Layer> layers = level.Layers.Values.Where(layer => layer.Name != "Ground" && layer.Name != "Collision").ToList();

        foreach(Layer layer in layers)
        {
            // create new parallax layer and cache it's SpriteRenderer
            Parallax       newLayer = Instantiate(_prefab);
            SpriteRenderer renderer = newLayer.GetComponent<SpriteRenderer>();

            Sprite sprite = layer.ToSprites()[0];

            // set attributes
            newLayer.name         =  layer.Name;
            renderer.sprite       =  sprite;
            newLayer.subject      = _player;
            newLayer.cam          = _camera;
            newLayer.infiniteLoop =  false;

            // add animations, if available
            if(layer.Frames.Count > 1) // multiple frames
            {
                AnimatedSprite animation = new AnimatedSprite(renderer);

                foreach(var texture in layer.Frames)
                    animation.Add(texture, new Rect(0, 0, texture.width, texture.height));

                level.AddAnimation(animation);
            }
            
            // set distance
            newLayer.transform.position = new Vector3(0, 0, layer.Distance);

            // in front of ground
            if(layer.Name.Contains("Foreground"))
                renderer.sortingLayerName = "Foreground";
            
            // set parent
            newLayer.transform.SetParent(this.transform);
        }
    }
}