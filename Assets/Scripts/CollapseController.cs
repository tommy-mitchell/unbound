using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CollapseController : MonoBehaviour
{
	// moves when game has started
	[ShowInInspector]
	private bool isMoving = true;
	[SerializeField]
	private float movementSpeed = 1f;

	private Vector2 _startPosition;
	[ShowInInspector]
	private bool isToggled = true;

	private void Start()
	{
		_startPosition = transform.position;
		InputController.i.Collapse_onToggle += () => {
			if(isToggled) // disable
				gameObject.SetActive(false);
			else // reset
			{
				transform.position = _startPosition;
				transform.Find("Rectangle").localScale = new Vector3(1, 1, 1);
				gameObject.SetActive(true);
			}

			isToggled = !isToggled; // flip toggle bool
		};

		//GameEngine.e.Engine_onStart += () => isMoving = true;
		//ResetState();
		//GameEngine.e.Engine_onNewLevel += ResetState();
	}

	private void ResetState()
	{
		isMoving = false;
		// position = level.startPos
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Player")
			GameEngine.e.OnCollapseTrigger();
	}

	private void Update()
	{
		if(isMoving)
		{
			float speedStep = movementSpeed * Time.deltaTime;

			transform.position += new Vector3(speedStep, 0, 0);
			transform.Find("Rectangle").localScale += new Vector3(.25f * speedStep, 0, 0);
		}
	}
}
