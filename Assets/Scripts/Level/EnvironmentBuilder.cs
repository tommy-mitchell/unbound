using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using LevelBuilder;

[ExecuteInEditMode]
public class EnvironmentBuilder : MonoBehaviour
{
    private string UnityPath => "G:/Unity/ld49/unbound/";

    [SerializeField]
    private UnityEditor.DefaultAsset _asepriteFile;
    private string asePath;
    private string aseName;

    [SerializeField]
    private UnityEditor.DefaultAsset _batchFile;
    private string batPath;

    private FileSystemWatcher watcher;

    [ShowInInspector, ReadOnly]
    private bool buildIsReady = false;

    [SerializeField, Tooltip("Disable to only update textures.")]
    private bool RebuildOnSave = true;

    [SerializeField]
    private Level level;

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

    private void OnEnable()
    {
        if(watcher == null)
            SetUpFileWatcher();
    }

    private void OnDisable()
    {
        watcher?.Dispose();
        Debug.Log($"stopped watching {aseName}");
    }

    private void Update()
    {
        if(buildIsReady && RebuildOnSave) // serves as a main-thread callback
        {
            Debug.Log("building");

            // reset level if rebuilding
            level = ScriptableObject.CreateInstance<Level>();

            SortLayers();
            BuildLevel();

            // save level to folder
            string folder = asePath.Substring(asePath.IndexOf("Assets"));

            AssetDatabase.CreateAsset(level, $"{folder}/level.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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

    private void SortLayers()
    {
        string subPath = asePath.Substring(asePath.IndexOf("Assets"));
        // remove trailing '/'
        string folder = string.Join("/", subPath.Split('/'), 0, subPath.Split('/').Length - 1);

        // get dictionary of layer distances
        var distances = GetDistancesFromJSON(folder);

        // add each texture in the folder to the level
        foreach(var layer in AssetDatabase.FindAssets("t:texture2D", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(layer);
            // gets '0X name_X.png' from path, gets '0X name_X', gets 'name_X', gets 'name'
            string name = path.Split('/')[path.Split('/').Length - 1].Split('.')[0].Substring(3).Split('_')[0];

            var frame = AssetDatabase.LoadMainAssetAtPath(path) as Texture2D;

            level.AddFrame(name, frame, distances[name]);
        }
    }

    private void BuildLevel()
    {
        // reset
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        // create game objects
        GameObject parallax = new GameObject("Parallax", typeof(ParallaxBuilder));
        GameObject   ground = new GameObject(  "Ground", typeof(Rigidbody2D), typeof(CompositeCollider2D), typeof(SplitGround));

        // set up each
        //SetUpParallax(parallax.GetComponent<ParallaxBuilder>());
        parallax.GetComponent<ParallaxBuilder>().BuildPrefabs(level);
        SetUpGround(ground.GetComponent<SplitGround>());

        // set parent
        parallax.transform.SetParent(this.transform);
          ground.transform.SetParent(this.transform);
    }

    private void SetUpGround(SplitGround ground)
    {
        // set attributes
        ground.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        ground.gameObject.layer = LayerMask.NameToLayer("Ground");
        
        // build ground and collision pieces
        ground.Split(level);
    }
}