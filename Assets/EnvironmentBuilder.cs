using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class EnvironmentBuilder : MonoBehaviour
{
    [SerializeField]
    private UnityEditor.DefaultAsset _asepriteFile;
    private string asePath;
    private string aseName;

    [SerializeField]
    //private string _batchFilePath = "Assets/example.bat";
    private UnityEditor.DefaultAsset _batchFile;
    private string batPath;

    private FileSystemWatcher watcher;

    private string UnityPath => "G:/Unity/ld49/unbound/";

    private Dictionary<string, List<Texture2D>> layers;

    public bool buildIsReady = false;

    [Button] // too lazy to auto create in editor -> button
    private void SetUpFileWatcher()
    {
        // make sure only one watcher is running at a time
        watcher?.Dispose();
        buildIsReady = false;

        // get directory of aseprite file
        string[] splitPath = AssetDatabase.GetAssetPath(_asepriteFile).Split('/');
        string path = UnityPath; // string.Join?
        aseName = splitPath[splitPath.Length - 1]; // file name and extension are at the end

        for(int i = 0; i < splitPath.Length - 1; i++) // exclude the file name
            path += splitPath[i] + '/';

        asePath = path;

        Debug.Log($"watching {aseName}");

        batPath = UnityPath + AssetDatabase.GetAssetPath(_batchFile);

        // set up watcher for file change
        watcher = new FileSystemWatcher(@path);

        watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.LastWrite;

        watcher.Changed += OnChanged;

        watcher.Filter = aseName; // only look at aseprite file
        watcher.EnableRaisingEvents = true;
    }
    
    [ShowInInspector]
    private Dictionary<string, Texture2D> tempLayers;

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if(e.ChangeType != WatcherChangeTypes.Changed)
            return;

        // file has changed
        Debug.Log($"file changed: {aseName}");

        // export layers
        System.Diagnostics.Process.Start(@batPath, $"\"{asePath}{aseName}\"");

        // set 'OnValidate()' "callback"
        buildIsReady = true;
    }

    private void OnValidate()
    {
        if(watcher == null)
            SetUpFileWatcher();
    }

    private void Update()
    {
        if(buildIsReady) // serves as a main-thread callback
        {
            Debug.Log("building");

            SortLayers();
            BuildLevel();

            Debug.Log("built");

            buildIsReady = false;
        }
    }

    private void SortLayers()
    {
        // gather layers into a dictionary
        //layers = new Dictionary<string, List<Texture2D>>();
        tempLayers = new Dictionary<string, Texture2D>();

        string subPath = asePath.Substring(asePath.IndexOf("Assets"));
        string folder = string.Join("/", subPath.Split('/'), 0, subPath.Split('/').Length - 1);

        foreach(var layer in AssetDatabase.FindAssets("t:texture2D", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(layer);
            // gets '0X name.png' from path, gets '0X name', gets 'name'
            string name = path.Split('/')[path.Split('/').Length - 1].Split('.')[0].Substring(3);

            //layers.Add(name, (Texture2D) AssetDatabase.LoadMainAssetAtPath(path));
            tempLayers.Add(name, (Texture2D) AssetDatabase.LoadMainAssetAtPath(path));
        }

        // TODO: sort w/ frames, create animator
    }

    private void BuildLevel()
    {
        // reset
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        // create game objects
        GameObject parallax = new GameObject("Parallax", typeof(ParallaxBuilder));
        GameObject   ground = new GameObject(  "Ground", typeof(Rigidbody2D), typeof(CompositeCollider2D), typeof(SplitGround));

        // set up
        SetUpParallax(parallax.GetComponent<ParallaxBuilder>());
        SetUpGround(ground.GetComponent<SplitGround>());

        // set parent
        parallax.transform.SetParent(this.transform);
          ground.transform.SetParent(this.transform);
    }

    private void SetUpParallax(ParallaxBuilder parallax)
    {
        // convert Texture2D to Sprite
        var sprites = new List<Sprite>();

        foreach(var layer in tempLayers)
        {
            // ignore non-parallaxed layers
            if(layer.Key.Contains("Ground") || layer.Key.Contains("Collision"))
                continue;

            // create sprite
            Sprite sprite = Sprite.Create(layer.Value, new Rect(0, 0, layer.Value.width, layer.Value.height), new Vector2(.5f, .5f), 16f);
            sprite.name = layer.Key;
            
            // add created sprite
            sprites.Add(sprite);
        }

        parallax.sprites = sprites;
        parallax.BuildPrefabs();
    }

    private void SetUpGround(SplitGround ground)
    {
        ground.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        ground.gameObject.layer = LayerMask.NameToLayer("Ground");
        
        Texture2D    groundTexture = tempLayers["Ground"];//layers[   "Ground"]?[0];
        Texture2D collisionTexture = tempLayers["Collision"];//layers["Collision"]?[0];

        ground._ground    =    groundTexture;
        ground._collision = collisionTexture;

        ground.Split();
    }
}
