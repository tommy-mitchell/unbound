using UnityEngine;

public class AnimateLevel : MonoBehaviour
{
    [SerializeField]
    private LevelBuilder.Level level;

    [SerializeField]
    private float framesPerSecond = 2f;

    private float frameTimer;

    // load in all sprite instances on startup
    private void Awake() => level.BuildSpriteInstances();

    private void Update()
    {
        // new frame
        if(frameTimer <= 0f)
        {
            // advance frames
            level.Animate();

            // reset timer
            frameTimer = framesPerSecond;

            return;
        }

        // decrement timer if not changing frames
        frameTimer -= Time.deltaTime;
    }
}
