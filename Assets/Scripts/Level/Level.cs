using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace LevelBuilder
{
    [System.Serializable]
    public class Layer
    {
        [SerializeField]
        private string _name;
        public string Name => _name;

        [OdinSerialize]
        public List<Texture2D> Frames { get; private set; }

        [SerializeField]
        private float _distance;
        public float Distance => _distance;

        public Layer(string name, Texture2D firstFrame, float distance = 0f)
        {
            _name = name;

            Frames = new List<Texture2D>();
            Frames.Add(firstFrame);

            _distance = distance;
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
                
                // add created sprite
                sprites.Add(sprite);
            }

            return sprites;
        }
    }

    [System.Serializable]
    public class AnimatedSprite
    {
        [System.Serializable]
        private struct TexturePair {
            [SerializeField]
            private Texture2D _texture;
            public Texture2D Texture => _texture;

            [SerializeField]
            private Rect _rect;
            public Rect Rect => _rect;

            public TexturePair(Texture2D texture, Rect textureRect)
            {
                _texture = texture;
                _rect = textureRect;
            }
        }

        [SerializeField]
        private SpriteRenderer _renderer;

        [OdinSerialize]
        private List<TexturePair> texturePairs;

        private LinkedList<Sprite> sprites;

        private LinkedListNode<Sprite> currentSprite;

        [ShowInInspector]
        public Sprite Sprite => currentSprite?.Value;

        [ShowInInspector]
        public LinkedList<Sprite> m_Sprites => sprites;

        public AnimatedSprite(SpriteRenderer renderer)
        {
            _renderer = renderer;
            texturePairs = new List<TexturePair>();
        }

        public void Add(Texture2D texture, Rect textureRect) => texturePairs.Add(new TexturePair(texture, textureRect));

        // create all sprites at start
        public void CreateSprites()
        {
            sprites = new LinkedList<Sprite>();

            foreach(var texturePair in texturePairs)
            {
                Sprite newSprite = Sprite.Create(texturePair.Texture, texturePair.Rect, new Vector2(.5f, .5f), 16f);
                newSprite.name = texturePair.Texture.name;

                // empty list
                if(sprites.Count == 0)
                {
                    sprites.AddFirst(newSprite);
                    currentSprite = sprites.First;
                }
                else
                    sprites.AddLast(newSprite);
            }
        }

        public void AdvanceSprite()
        {
            // advance sprite if next is available, otherwise circle back around
               currentSprite = currentSprite.Next ?? sprites.First;
            _renderer.sprite = currentSprite.Value;
        }
    }

    public class Level : SerializedScriptableObject
    {
        [OdinSerialize] // all of the layers in the level
        public Dictionary<string, Layer> Layers { get; private set; }

        [OdinSerialize] // a collection of each sprite instance to be animated
        public List<AnimatedSprite> AnimatedSprites { get; private set; }

        public Level()
        {
            Layers = new Dictionary<string, Layer>();
            AnimatedSprites = new List<AnimatedSprite>();
        }

        // add frame to the layer specificed by name
        public void AddFrame(string name, Texture2D frame, float distance = 0)
        {
            if(Layers.ContainsKey(name))
                Layers[name].AddFrame(frame);
            else
                Layers.Add(name, new Layer(name, frame, distance));
        }

        public void AddAnimation(AnimatedSprite sprite) => AnimatedSprites.Add(sprite);

        // advance each sprite
        public void Animate()
        {
            foreach(var sprite in AnimatedSprites)
                sprite.AdvanceSprite();
        }

        public void BuildSpriteInstances()
        {
            foreach(var sprite in AnimatedSprites)
                sprite.CreateSprites();
        }
    }
}