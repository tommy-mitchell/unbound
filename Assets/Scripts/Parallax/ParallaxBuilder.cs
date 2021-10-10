using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ParallaxBuilder : MonoBehaviour
{
    public List<Sprite> sprites;

    [Button]
    public void BuildPrefabs(float distance = 38f)
    {
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        Parallax  _prefab = (UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Parallax Sprite.prefab", (typeof(GameObject))) as GameObject).GetComponent<Parallax>();
        Transform _player = GameObject.FindGameObjectWithTag("Player").transform;
        Camera    _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        int foregroundCount = 0;

        foreach(Sprite sprite in sprites)
        {
            // create new parallax layer and cache it's SpriteRenderer
            Parallax       newLayer = Instantiate(_prefab);
            SpriteRenderer renderer = newLayer.GetComponent<SpriteRenderer>();

            // set attributes
            newLayer.name         =  sprite.name;
            renderer.sprite       =  sprite;
            newLayer.subject      = _player;
            newLayer.cam          = _camera;
            newLayer.infiniteLoop =  false;
            
            // set distance
            if(sprite.name.Contains("Foreground")) // in front of camera
            {
                newLayer.transform.position = new Vector3(0, 0, -1 - foregroundCount++);
                renderer.sortingLayerName = "Foreground";
            }
            else // away from camera
            {
                newLayer.transform.position = new Vector3(0, 0, distance);
                distance /= 2.0f; // get closer on each layer
                //distance /= 2.718f;
                //distance = Mathf.Sqrt(distance);
            }

            // set parent
            newLayer.transform.SetParent(this.transform);
        }
    }
}
