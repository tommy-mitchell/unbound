using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LevelBuilder;

public class SplitGround : MonoBehaviour
{
    [Tooltip("Width of each split texture in pixels.")]
    public int pixelWidth = 16;

    public void Split(Level level)
    {
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        Texture2D _ground    = level.Layers[   "Ground"].Frames[0];
        Texture2D _collision = level.Layers["Collision"].Frames[0];

        int numberOfSlices = _ground.width / pixelWidth;

        for(int i = 0; i < numberOfSlices; i++)
        {
            // get rect of slice
            Rect textureRect = new Rect(i * pixelWidth, 0.0f, pixelWidth, _ground.height);

            // create new GameObject
            GameObject slice = new GameObject();
            slice.transform.name = $"Ground Slice {i}";

            // create ground sprite
            Sprite groundSprite = Sprite.Create(_ground, textureRect, new Vector2(.5f, .5f), 16f);
            groundSprite.name = $"Ground {i}";

            // set new sliced sprite
            SpriteRenderer _groundRenderer = slice.AddComponent<SpriteRenderer>();
            _groundRenderer.sprite           = groundSprite;
            _groundRenderer.sortingLayerName = "Background";
            _groundRenderer.sortingOrder     = 10;

            // add animations, if available
            if(level.Layers["Ground"].Frames.Count > 1) // multiple frames
            {
                AnimatedSprite groundAnimation = new AnimatedSprite(_groundRenderer);

                //int count = 0;
                //sprite.name = $"Ground {i}_{count}";
                //count++;

                foreach(var texture in level.Layers["Ground"].Frames)
                    groundAnimation.Add(texture, textureRect);

                level.AddAnimation(groundAnimation);
            }

            // create collision GameObject
            GameObject collision = new GameObject();
            collision.transform.name = "Collision";

            // create collision sprite
            Sprite collisionSprite = Sprite.Create(_collision, textureRect, new Vector2(.5f, .5f), 16f);
            collisionSprite.name = $"Collision {i}";

            // set sprite and collision
            collision.AddComponent<SpriteRenderer>().sprite = collisionSprite;
            collision.GetComponent<SpriteRenderer>().enabled = false;
            collision.AddComponent<PolygonCollider2D>().usedByComposite = true;
            collision.AddComponent<PixelPerfectCollider2D>().Regenerate();
            
            // move collider over to align with ground
            collision.transform.position -= new Vector3(i, 0, 0);

            // set parent objects
            collision.transform.SetParent(slice.transform);
            slice.transform.SetParent(this.transform);

            slice.transform.position = new Vector2(i - numberOfSlices / 2, 0);
        }
    }
}