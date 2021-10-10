using System.Collections.Generic;
using UnityEngine;
//using Sirenix.OdinInspector;
using LevelBuilder;

public class ParallaxBuilder : MonoBehaviour
{
    public void BuildPrefabs(List<Layer> layers)
    {
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        Parallax  _prefab = (UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Parallax Sprite.prefab", (typeof(GameObject))) as GameObject).GetComponent<Parallax>();
        Transform _player = GameObject.FindGameObjectWithTag("Player").transform;
        Camera    _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        foreach(Layer layer in layers)
        {
            // create new parallax layer and cache it's SpriteRenderer
            Parallax       newLayer = Instantiate(_prefab);
            SpriteRenderer renderer = newLayer.GetComponent<SpriteRenderer>();

            Sprite sprite = layer.ToSprites()[0]; // TODO: temp

            // set attributes
            newLayer.name         =  sprite.name;
            renderer.sprite       =  sprite;
            newLayer.subject      = _player;
            newLayer.cam          = _camera;
            newLayer.infiniteLoop =  false;
            
            // set distance
            newLayer.transform.position = new Vector3(0, 0, layer.Distance);

            // in front of ground
            if(sprite.name.Contains("Foreground"))
                renderer.sortingLayerName = "Foreground";
            
            // set parent
            newLayer.transform.SetParent(this.transform);
        }
    }
}
