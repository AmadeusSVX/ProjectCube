using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PolygonPositioner : MonoBehaviour
{
	public Transform camera_transform;
	public Transform voxel_cube;
	public GameObject[] copy_objects;
	public int current_object_id = 0;

	private Vector3 current_position;

	private float tap_timer = 0.0f;
	private Vector3 begin_pos;

	void OnManipulationEvent(Vector3 cumulativeDelta)
	{
		transform.position = current_position + camera_transform.TransformVector(cumulativeDelta);
	}

	void OnManupulationEndEvent()
	{
		current_position = transform.position;
	}

	void OnTapEvent()
	{
		if (EventSystem.current.IsPointerOverGameObject()) return;
		//		Debug.Log("Positioned...");

		// here to duplicate objects

		transform.position = new Vector3(voxel_cube.position.x + Random.Range(-0.3f, 0.3f), voxel_cube.position.y + Random.Range(0.1f, 0.3f), voxel_cube.position.z + Random.Range(0.1f, 0.3f));
		transform.rotation = Quaternion.Euler(new Vector3(voxel_cube.rotation.eulerAngles.x + Random.Range(-10.0f, 10.0f), voxel_cube.rotation.eulerAngles.y + Random.Range(-30.0f, 30.0f), voxel_cube.rotation.eulerAngles.z + Random.Range(-10.0f, 10.0f)));

		this.GetComponent<Rigidbody>().velocity = new Vector3();
		this.GetComponent<Rigidbody>().angularVelocity = new Vector3();
		this.GetComponent<Rigidbody>().isKinematic = false;

		// enable copy objects
		//		copy_objects[current_object_id] = GameObject.Instantiate(this.gameObject);
		//		copy_objects[current_object_id].GetComponent<PolygonPositioner>().enabled = false;
		//		copy_objects[current_object_id].GetComponent<Rigidbody>().isKinematic = false;

		// disable my own component
		//		this.enabled = false;
	}

	// Use this for initialization
	void OnEnable()
	{
		current_object_id = 0;
	}

	// Update is called once per frame
	void Update()
	{
		TouchInfo current_touch = AppUtil.GetTouch();
		Vector3 touch_pos = AppUtil.GetTouchPosition();

		if (current_touch == TouchInfo.Began) { tap_timer = 0.0f; begin_pos = touch_pos; }
		else if (current_touch == TouchInfo.Moved) { tap_timer += Time.deltaTime; OnManipulationEvent((touch_pos - begin_pos) * 0.00001f); }
		else if (current_touch == TouchInfo.Ended) { if (tap_timer + Time.deltaTime < 0.1f) OnTapEvent(); }
		else if (current_touch == TouchInfo.Stationary) { tap_timer += Time.deltaTime; }

		transform.localScale = voxel_cube.localScale;
	}
}
