using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PolygonPositioner : MonoBehaviour
{
	public Transform camera_transform;
	public Transform voxel_cube;
	public CopyCube[] copy_cubes;
	public int current_object_id = 0;

	private Vector3 current_position;

	private float tap_timer = 0.0f;
	private Vector3 begin_pos;

	void OnManipulationEvent(Vector3 cumulativeDelta)
	{
//		transform.position = current_position + camera_transform.TransformVector(cumulativeDelta);
	}

	void OnManupulationEndEvent()
	{
//		current_position = transform.position;
	}

	void OnTapEvent()
	{
		if (EventSystem.current.IsPointerOverGameObject()) return;
		//		Debug.Log("Positioned...");

		// here to duplicate objects
		/*
				transform.position = new Vector3(voxel_cube.position.x + Random.Range(-0.2f, 0.2f), voxel_cube.position.y + Random.Range(0.1f, 0.2f), voxel_cube.position.z + Random.Range(0.1f, 0.3f));
				transform.rotation = Quaternion.Euler(new Vector3(voxel_cube.rotation.eulerAngles.x + Random.Range(-10.0f, 10.0f), voxel_cube.rotation.eulerAngles.y + Random.Range(-30.0f, 30.0f), voxel_cube.rotation.eulerAngles.z + Random.Range(-10.0f, 10.0f)));

				this.GetComponent<Rigidbody>().velocity = new Vector3();
				this.GetComponent<Rigidbody>().angularVelocity = new Vector3();
				this.GetComponent<Rigidbody>().isKinematic = false;
		*/

		// enable copy objects
		Vector3 local_pos = camera_transform.InverseTransformPoint(voxel_cube.transform.position) +
							Vector3.forward * Random.Range(transform.localScale.x, transform.localScale.x * 2.0f) +
							Vector3.right * Random.Range(-transform.localScale.x * 2.0f, transform.localScale.x * 2.0f);

		Vector3 next_pos = camera_transform.TransformPoint(local_pos);


		copy_cubes[current_object_id].transform.position = new Vector3(next_pos.x, 
																      voxel_cube.position.y + Random.Range(transform.localScale.x * 1.0f, transform.localScale.x * 2.0f), 
																	  next_pos.z);
		copy_cubes[current_object_id].transform.rotation = Quaternion.Euler(new Vector3(voxel_cube.rotation.eulerAngles.x + Random.Range(-10.0f, 10.0f), 
																						voxel_cube.rotation.eulerAngles.y + Random.Range(-30.0f, 30.0f), 
																						voxel_cube.rotation.eulerAngles.z + Random.Range(-10.0f, 10.0f)));
		copy_cubes[current_object_id].transform.localScale = transform.localScale;
		copy_cubes[current_object_id].EnableCopyCube();
		current_object_id = (current_object_id + 1) % copy_cubes.Length;

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
		else if (current_touch == TouchInfo.Ended) { if (tap_timer + Time.deltaTime < 0.2f) OnTapEvent(); }
		else if (current_touch == TouchInfo.Stationary) { tap_timer += Time.deltaTime; }

		transform.localScale = voxel_cube.localScale;
	}
}
