using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CubePositioner : MonoBehaviour {

	public Transform dummy_cube;
	public InfoOutput info_output;

	private Vector3 current_scale;
	private Vector3 current_position;
	private Vector3 begin_pos;
	private float tap_timer = 0.0f;

	void OnManipulationEvent(Vector3 cumulativeDelta)
	{
//		Vector3 localDelta = Camera.current.transform.InverseTransformVector(cumulativeDelta);
		Vector3 localDelta = cumulativeDelta;

		transform.localScale = current_scale * (1.0f + localDelta.x * 3.0f);
		if (transform.localScale.x < 0.08f) { transform.localScale = Vector3.one * 0.08f; }
		if (transform.localScale.x > 2.0f) { transform.localScale = Vector3.one * 2.0f; }

		dummy_cube.transform.localPosition = new Vector3(current_position.x, current_position.y, current_position.z + localDelta.y * 3.0f);

		Debug.Log("localDelta" + localDelta);
		Debug.Log("Scale:" + transform.localScale.x);
		Debug.Log("Z:" + transform.localPosition.z);
	}

	void OnManupulationStartEvent()
	{
		current_scale = transform.localScale;
		current_position = dummy_cube.transform.localPosition;
	}

	void OnTapEvent()
	{
		if (EventSystem.current.IsPointerOverGameObject())	return;

		// disable my own component
		this.enabled = false;
	}

	private void OnEnable()
	{

	}

	// Update is called once per frame
	void Update () 
	{
		TouchInfo current_touch = AppUtil.GetTouch();
		Vector3 touch_pos = AppUtil.GetTouchPosition();

		if(current_touch == TouchInfo.Began) { tap_timer = 0.0f; begin_pos = touch_pos; OnManupulationStartEvent();  }
		else if (current_touch == TouchInfo.Moved) { tap_timer += Time.deltaTime; OnManipulationEvent((touch_pos - begin_pos) * 0.0001f); }
		else if (current_touch == TouchInfo.Ended) { if (tap_timer + Time.deltaTime < 0.2f) OnTapEvent(); }
		else if(current_touch == TouchInfo.Stationary) { tap_timer += Time.deltaTime; }

		transform.position = dummy_cube.transform.position;
		Vector3 dummy_eular = dummy_cube.rotation.eulerAngles;
		transform.rotation = Quaternion.Euler(new Vector3(0, dummy_eular.y, 0));

		info_output.info_text = "Scale:" + transform.localScale.x.ToString("F2") + " Distance:" + transform.localPosition.z.ToString("F2") + " m";
	}
}
