using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public float stepRate = 1f;
    public float stepCooldown;
    public float range = .2f;
    public AudioSource footsteps;

    private bool IsMoving => Mathf.Abs(PlayerStateManager.p.MoveInput.x) > 0;
    private bool IsRunning => PlayerStateManager.p.CurrentState == PlayerStateManager.PLAYER_RUN;

    private void Update()
    {
        stepCooldown -= Time.deltaTime;

        // walking
        if(IsMoving && IsRunning && stepCooldown < 0f)
        {
            footsteps.pitch = 1f + Random.Range (-range, range);
            footsteps.PlayOneShot(footsteps.clip);
            stepCooldown = stepRate;
        }
    }
}
