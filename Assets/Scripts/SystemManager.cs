using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AppMode
{
	OffMode,
	PositioningMode,
	ScanMode,
	PolygonizeMode,
	PhysicsMode
}

public class SystemManager : MonoBehaviour {

	public AppMode current_mode = AppMode.PositioningMode;
	public TangoColorizePointCloud tango_point_cloud = null;

	public GameObject positioning_mode_button = null;
	public GameObject polygonize_mode_button = null;
	
	public GameObject voxel_cube;
	public GameObject polygon_cube;

	public TextMesh mode_display;

	// Use this for initialization
	void Start () 
	{
		current_mode = AppMode.OffMode;
	}
	
	// Update is called once per frame
	void Update () {
		if (current_mode == AppMode.OffMode)
		{
			positioning_mode_button.SetActive(false);
			polygonize_mode_button.SetActive(false);

			voxel_cube.SetActive(false);
			polygon_cube.SetActive(false);

			mode_display.text = "OFF";

			if (!voxel_cube.GetComponent<CubePositioner>().enabled) { current_mode = AppMode.ScanMode; }
		}
		else if (current_mode == AppMode.PositioningMode)
		{
			positioning_mode_button.SetActive(false);
			polygonize_mode_button.SetActive(false);

			voxel_cube.SetActive(true);
			polygon_cube.SetActive(false);

			mode_display.text = "POSITIONING MODE";

			if (!voxel_cube.GetComponent<CubePositioner>().enabled) { current_mode = AppMode.ScanMode; }
		}
		else if (current_mode == AppMode.ScanMode)
		{
			positioning_mode_button.SetActive(true);
			polygonize_mode_button.SetActive(true);

			voxel_cube.SetActive(true);
			polygon_cube.SetActive(false);

			voxel_cube.GetComponent<VoxelProcessor>().SetVertices(tango_point_cloud.m_nrPoints.ToArray(), tango_point_cloud.m_nrColor.ToArray());

			mode_display.text = "SCAN MODE";
		}
		else if (current_mode == AppMode.PolygonizeMode)
		{
			positioning_mode_button.SetActive(true);
			polygonize_mode_button.SetActive(false);

			voxel_cube.SetActive(true);
			polygon_cube.SetActive(true);

			mode_display.text = "POLYGONIZE MODE";

			if (!polygon_cube.GetComponent<PolygonPositioner>().enabled) { current_mode = AppMode.PhysicsMode; }
		}
	}

	public void OnClickPositioningMode()
	{
		current_mode = AppMode.PositioningMode;
		voxel_cube.GetComponent<CubePositioner>().enabled = true;
		voxel_cube.GetComponent<VoxelProcessor>().ClearVoxel();
	}

	public void OnClickPolygonizeMode()
	{
		polygon_cube.SetActive(true);
		polygon_cube.transform.SetPositionAndRotation(voxel_cube.transform.position, voxel_cube.transform.rotation);
		polygon_cube.GetComponent<PolygonPositioner>().enabled = true;

		polygon_cube.GetComponent<Rigidbody>().isKinematic = true;
		voxel_cube.GetComponent<VoxelProcessor>().SetVoxel();

		current_mode = AppMode.PolygonizeMode;
	}

	public void OnClickSwitch()
	{
		if(current_mode == AppMode.OffMode)
		{
			current_mode = AppMode.PositioningMode;
		}
		else
		{
			OnClickPositioningMode();
			current_mode = AppMode.OffMode;
		}
	}
}
