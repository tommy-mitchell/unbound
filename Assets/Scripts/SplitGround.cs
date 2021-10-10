using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LevelBuilder;

public class SplitGround : MonoBehaviour
{
    [Tooltip("Width of each split texture in pixels.")]
    public int pixelWidth = 16;

    public void Split(Dictionary<string, Layer> layers, AnimationClip animClip)
    {
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

        Texture2D _ground    = layers[   "Ground"].Frames[0];
        Texture2D _collision = layers["Collision"].Frames[0];

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
            slice.AddComponent<SpriteRenderer>().sprite = groundSprite;
            slice.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
            slice.GetComponent<SpriteRenderer>().sortingOrder = 10;

            // add animations, if available
            if(layers["Ground"].Frames.Count > 1) // multiple frames
                AddAnimations(animClip, layers["Ground"].ToSprites(textureRect));

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
            
            collision.transform.position -= new Vector3(i, 0, 0); // TEMP

            // set parent objects
            collision.transform.SetParent(slice.transform);
            slice.transform.SetParent(this.transform);

            slice.transform.position = new Vector2(i - numberOfSlices / 2, 0);
        }

        // TODO: temp saving
        AssetDatabase.CreateAsset(animClip, "Assets/Animation/Level/temp.anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void AddAnimations(AnimationClip animClip, List<Sprite> sprites)
    {
        // http://answers.unity.com/answers/1084464/view.html

        // set up sprite binding
        var spriteBinding = new EditorCurveBinding {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        // assign key frames
        var spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Count];

        for(int index = 0; index < sprites.Count; index++)
        {
            //AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(sprites[index]));
            Debug.Log(sprites[index].GetInstanceID());
            spriteKeyFrames[index] = new ObjectReferenceKeyframe();
            spriteKeyFrames[index].time = index;
            spriteKeyFrames[index].value = sprites[index];
        }

        // assign to animation clip
        AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);
    }
}