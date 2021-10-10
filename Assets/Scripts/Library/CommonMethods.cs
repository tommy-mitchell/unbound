using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonLibrary
{
    public static class CommonMethods
    {
        public static bool IsInRange(float distance, float range)
        {
            return distance <= range;
        }

        public static void Move(Transform transform, Vector3 position, float moveSpeed)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
            RotateTowardsPosition(transform, position);
        }

        public static void Jump(float verticalStrength, float horizontalStrength, Rigidbody2D rb)
        {
            rb.velocity = (Vector2.up * verticalStrength) + (Vector2.right * horizontalStrength);
        }

        public static void RotateTowardsPosition(Transform transform, Vector3 position)
        {
            int rotationValue = (position.x > transform.position.x) ? 0 : -180; // facing right if position is to the right of current position

            transform.rotation = Quaternion.Euler(0, rotationValue, 0);
        }

        public static bool CursorInRange(Vector2 cursorScreenPos, Vector2 targetScreenPos)
        {
            float cursorToTargetX = cursorScreenPos.x - targetScreenPos.x;

            return cursorToTargetX > -50f && cursorToTargetX < 50f; // Adjust how close mouse has to be to target by changing float value
        }

        public static float CheckHeightOfLocation(Vector2 location)
        {
            const float MAXIMUM_HEIGHT = 30; // height to "timeout" search

            ContactFilter2D hitFilter = new ContactFilter2D();
                            hitFilter.SetLayerMask(LayerMask.GetMask("Ground"));

            RaycastHit2D[] results = new RaycastHit2D[1];

            //float distanceAbove = Physics2D.Raycast(location, Vector2.up,   MAXIMUM_HEIGHT, LayerMask.NameToLayer("Ground")).distance;
            //float distanceBelow = Physics2D.Raycast(location, Vector2.down, MAXIMUM_HEIGHT, LayerMask.NameToLayer("Ground")).distance;

            Physics2D.Raycast(location, Vector2.up,   hitFilter, results, MAXIMUM_HEIGHT);
            RaycastHit2D aboveHit = results[0];
            float distanceAbove = aboveHit.distance;

            Physics2D.Raycast(location, Vector2.down, hitFilter, results, MAXIMUM_HEIGHT);
            RaycastHit2D belowHit = results[0];
            float distanceBelow = belowHit.distance;

            return distanceAbove + distanceBelow;
        }

        public static bool IsOverlapping(Collider2D thisCollider, Collider2D otherCollider) => thisCollider.Distance(otherCollider).isOverlapped;

        // in seconds
        public static async void WaitForSeconds(int time) => await System.Threading.Tasks.Task.Delay(time * 1000);

        public static IEnumerator WaitForSecondsCoroutine(float time)
        {
            yield return new WaitForSecondsRealtime(time);
        }

        public static void DestroyAllChildren(Transform transform)
        {
            for(int i = transform.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}