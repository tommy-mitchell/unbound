using UnityEngine;
using Sirenix.OdinInspector;

public class SplitGround : MonoBehaviour
{
    public Texture2D _ground;

    public Texture2D _collision;

    [Tooltip("Width of each split texture in pixels.")]
    public int pixelWidth = 16;

    //private int numberOfSlices = ;

    [Button]
    public void Split()
    {
        CommonLibrary.CommonMethods.DestroyAllChildren(this.transform);

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
    }
}