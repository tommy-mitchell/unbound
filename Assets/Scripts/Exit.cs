using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Player")
			GameEngine.e.OnExit();
	}
}