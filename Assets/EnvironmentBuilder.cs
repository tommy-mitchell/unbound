using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using LevelBuilder;

[ExecuteInEditMode]
public class EnvironmentBuilder : SerializedMonoBehaviour
{
    [SerializeField]
    private UnityEditor.DefaultAsset _asepriteFile;
    private string asePath;
    private string aseName;

    [SerializeField]
    private UnityEditor.DefaultAsset _batchFile;
    private string batPath;

    private FileSystemWatcher watcher;

    private string UnityPath => "G:/Unity/ld49/unbound/";

    private bool buildIsReady = false;

    [SerializeField, Tooltip("Disable to only update textures.")]
    private bool RebuildOnSave = true;

    [OdinSerialize]
    private Dictionary<string, Layer> layers;

    [SerializeField, HideInInspector]
    private Animator _anim;

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

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if(e.ChangeType != WatcherChangeTypes.Changed)
            return;

        // file has changed
        Debug.Log($"file changed: {aseName}");

        // get file name without extension
        string name = aseName.Split('.')[0];

        // export layers
        System.Diagnostics.Process.Start(@batPath, $"\"{asePath}{aseName}\", \"{asePath}{name}.json\"");

        // set "callback"
        buildIsReady = true;
    }

    private void OnValidate()
    {
        if(watcher == null)
            SetUpFileWatcher();

        _anim ??= GetComponent<Animator>();
    }

    private void OnDisable() => watcher?.Dispose();

    private void Update()
    {
        if(buildIsReady && RebuildOnSave) // serves as a main-thread callback
        {
            Debug.Log("building");

            SortLayers();
            BuildLevel();

            Debug.Log("built");

            buildIsReady = false;
        }
    }

    private Dictionary<string, float> GetDistancesFromJSON(string folder)
    {
        // get json file name
        string jsonName = $"{aseName.Split('.')[0]}.json";

        // get json string from assets
        string json = (AssetDatabase.LoadAssetAtPath($"{folder}/{jsonName}", typeof(TextAsset)) as TextAsset).text;

        // get list of meta layer substrings
        List<string> jsonLayers = json.Substring(json.IndexOf("{ \"name")).Split(']')[0].Split('}').ToList();
        jsonLayers = jsonLayers.Take(jsonLayers.Count - 1).ToList(); // remove trailing empty list

        var distances = new Dictionary<string, float>();

        foreach(string layer in jsonLayers)
        {
            // name: get index in layer string, layer name starts at index 8, go until " mark
            string    name = layer.Substring(layer.IndexOf("name")).Substring(8).Split('\"')[0];
            // distance: get index in layer string, layer data starts at index 8, go until " mark
            float distance = float.Parse(layer.Substring(layer.IndexOf("data")).Substring(8).Split('\"')[0]);

            distances.Add(name, distance);
        }

        return distances;
    }

    [Button]
    private void SortLayers()
    {
        // gather layers into a dictionary
        layers = new Dictionary<string, Layer>();

        string subPath = asePath.Substring(asePath.IndexOf("Assets"));
        // remove trailing '/'
        string folder = string.Join("/", subPath.Split('/'), 0, subPath.Split('/').Length - 1);

        // get dictionary of layer distances
        var distances = GetDistancesFromJSON(folder);

        // add each texture in the folder to the dictionary
        foreach(var layer in AssetDatabase.FindAssets("t:texture2D", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(layer);
            // gets '0X name_X.png' from path, gets '0X name_X', gets 'name_X', gets 'name'
            string name = path.Split('/')[path.Split('/').Length - 1].Split('.')[0].Substring(3).Split('_')[0];

            var frame = AssetDatabase.LoadMainAssetAtPath(path) as Texture2D;

            if(layers.ContainsKey(name))
                layers[name].AddFrame(frame);
            else
                layers.Add(name, new Layer(name, frame, distances[name]));
        }

        // TODO: create animator
    }

    [Button]
    private void BuildLevel()
    {
        // reset
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        // create animation clip
        AnimationClip animClip = new AnimationClip();
        animClip.frameRate = 1;
        //_anim.GetCurrentAnimatorStateInfo(0).
        // TODO: set in Animator

        // create game objects
        GameObject parallax = new GameObject("Parallax", typeof(ParallaxBuilder));
        GameObject   ground = new GameObject(  "Ground", typeof(Rigidbody2D), typeof(CompositeCollider2D), typeof(SplitGround));

        // set up each
        SetUpParallax(parallax.GetComponent<ParallaxBuilder>(), animClip);
        SetUpGround(ground.GetComponent<SplitGround>(), animClip);

        // set parent
        parallax.transform.SetParent(this.transform);
          ground.transform.SetParent(this.transform);
    }

    private void SetUpParallax(ParallaxBuilder parallax, AnimationClip animClip)
    {
        // get layers to be parallaxed
        var parallaxLayers = new List<Layer>();

        foreach(var layer in layers)
        {
            // ignore non-parallaxed layers
            if(layer.Key.Contains("Ground") || layer.Key.Contains("Collision"))
                continue;

            // add parallaxed layers
            parallaxLayers.Add(layer.Value);
        }

        // build parallax layers
        parallax.BuildPrefabs(parallaxLayers);
    }

    private void SetUpGround(SplitGround ground, AnimationClip animClip)
    {
        // set attributes
        ground.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        ground.gameObject.layer = LayerMask.NameToLayer("Ground");
        
        // build ground and collision pieces
        ground.Split(layers.Where(layer => layer.Key.Equals("Ground") || layer.Key.Equals("Collision"))
            .ToDictionary(layer => layer.Key, layer => layer.Value), animClip);
    }
}