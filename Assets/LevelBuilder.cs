using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LevelBuilder
{
    [System.Serializable]
    public class Layer
    {
        [ShowInInspector]
        public string Name { get; }

        [ShowInInspector]
        public List<Texture2D> Frames { get; }

        [ShowInInspector]
        public float Distance { get; private set; }

        public Layer(string name, Texture2D firstFrame, float distance = 0f)
        {
            Name = name;

            Frames = new List<Texture2D>();
            Frames.Add(firstFrame);

            Distance = distance;
        }

        public Layer() => Frames = new List<Texture2D>();

        public void AddFrame(Texture2D frame) => Frames.Add(frame);

        public List<Sprite> ToSprites(Rect? textureRect = null)
        {
            var sprites = new List<Sprite>();

            foreach(var frame in Frames)
            {
                // use either provided Rect or full size Rect
                textureRect ??= new Rect(0, 0, frame.width, frame.height);

                // create sprite
                Sprite sprite = Sprite.Create(frame, (Rect) textureRect, new Vector2(.5f, .5f), 16f);
                sprite.name = Name;

                // make sure sprite is loaded
                UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(UnityEditor.AssetDatabase.GetAssetPath(sprite));
                
                // add created sprite
                sprites.Add(sprite);
            }

            UnityEditor.AssetDatabase.Refresh();

            return sprites;
        }
    }
}