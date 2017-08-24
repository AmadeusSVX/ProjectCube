using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class VoxelProcessor : MonoBehaviour {

	public int voxel_resolution = 64;
	public int vertex_threshold = 10;
	public int neighbor_threshold = 4;
	public MeshFilter[] mesh_filters = null;

	private MarchingCubesCPU marching_cube;
	private List<Vector3>	mesh_vertices;
	private List<Color32>	mesh_colors;
	private List<int> mesh_indices;

	private Color32[] colors;

	// thread related function
	private Thread voxel_thread;
	private bool is_thread = false;
	private bool is_process = false;
	private bool is_update = false;

	// need to be multithreading
	public void SetVoxel()
	{
		// disable rendering while processing
		for (int i = 0; i < mesh_filters.Length; i++)
		{
			mesh_filters[i].GetComponent<Renderer>().enabled = false;
			mesh_filters[i].mesh.Clear();
		}

		// start processing on thread
		is_process = true;
	}

	public void ClearVoxel()
	{
		for(int i=0; i<colors.Length; i++) { colors[i] = new Color32(0, 0, 0, 0); }
	}

	public void NoiseReduction()
	{
		for (int z = 1; z < voxel_resolution - 1; z++)
		{
			for (int y = 1; y < voxel_resolution - 1; y++)
			{
				for (int x = 1; x < voxel_resolution - 1; x++)
				{
					int target_index = (voxel_resolution * voxel_resolution * z) + (voxel_resolution * y) + x;
					int neighbor_counter = 0;
					if (colors[target_index].a == 0) { continue; }

					if (colors[target_index + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - 1].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution - 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution - 1].a != 0) { neighbor_counter++; }

					if (colors[target_index + voxel_resolution * voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution - 1].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution + voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution + voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution + voxel_resolution - 1].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution - voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution - voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index + voxel_resolution * voxel_resolution - voxel_resolution - 1].a != 0) { neighbor_counter++; }

					if (colors[target_index - voxel_resolution * voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution - 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution + voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution + voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution + voxel_resolution - 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution - voxel_resolution + 1].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution - voxel_resolution].a != 0) { neighbor_counter++; }
					if (colors[target_index - voxel_resolution * voxel_resolution - voxel_resolution - 1].a != 0) { neighbor_counter++; }

					if(neighbor_counter < neighbor_threshold) { colors[target_index] = new Color32(0, 0, 0, 0); }
				}
			}
		}
	}

	public void NoiseReduction2()
	{
		for (int i = 0; i < colors.Length; i++)
		{
			if(colors[i].a < vertex_threshold){ colors[i] = new Color32(0, 0, 0, 0);   }
			else { colors[i].a = 255; }
		}
	}


	public void SetVertices(Vector3[] in_vertices, Color32[] in_colors)
	{
		if(is_process) { return; }

		for(int i=0; i<in_vertices.Length; i++)
		{
			Vector3 local_vertex = transform.InverseTransformPoint(in_vertices[i]) + Vector3.one * 0.5f;

			float voxel_pitch = 1.0f / voxel_resolution;
			int x_index = (int)(local_vertex.x / voxel_pitch + 0.5f);
			int y_index = (int)(local_vertex.y / voxel_pitch + 0.5f);
			int z_index = (int)(local_vertex.z / voxel_pitch + 0.5f);

			if (x_index < 0 || x_index >= voxel_resolution
			|| y_index < 0 || y_index >= voxel_resolution
			|| z_index < 0 || z_index >= voxel_resolution) { continue; }

			int target_index = (voxel_resolution * voxel_resolution * z_index) + (voxel_resolution * y_index) + x_index;

			if(colors[target_index].a < 255 && in_colors[i].a > 0)
			{
				colors[target_index].a = (byte)(colors[target_index].a + 1);
				colors[target_index] = new Color32((byte)((colors[target_index].r + in_colors[i].r) * 0.5f),
												   (byte)((colors[target_index].g + in_colors[i].g) * 0.5f),
												   (byte)((colors[target_index].b + in_colors[i].b) * 0.5f),
												   colors[target_index].a);
			}
		}

		NoiseReduction();	// not working nicely...
	}

	// Use this for initialization
	void OnEnable () 
	{
		marching_cube = new MarchingCubesCPU();
		marching_cube._gridSize = voxel_resolution;
//		density_texture = new Texture3D(voxel_resolution, voxel_resolution, voxel_resolution, TextureFormat.RGBA32, false);
//		density_texture.wrapMode = TextureWrapMode.Clamp;
		colors = new Color32[voxel_resolution * voxel_resolution * voxel_resolution];

		mesh_vertices = new List<Vector3>();
		mesh_colors = new List<Color32>();
		mesh_indices = new List<int>();


		is_process = false;
		is_update = false;
		is_thread = true;
		voxel_thread = new Thread(VoxelThread);
		voxel_thread.Start();
	}

	// Update is called once per frame
	void OnDisable() 
	{
		is_thread = false;
		voxel_thread.Join();
		// no need to texture release?
	}

	private void Update()
	{
		if(!is_process && is_update)
		{
			int current_index = 0;

			for (int i = 0; i < mesh_filters.Length; i++)
			{
				if (mesh_vertices.Count - current_index > 60000)
				{
					Debug.Log("Mode 1 Index:" + i + " from:" + current_index + " to:" + current_index + 60000);
					mesh_filters[i].mesh.vertices = mesh_vertices.GetRange(current_index, 60000).ToArray();
					mesh_filters[i].mesh.colors32 = mesh_colors.GetRange(current_index, 60000).ToArray();
					mesh_indices.Clear();
					for (int j = 0; j < 60000; j++) { mesh_indices.Add(j); }
					mesh_filters[i].mesh.triangles = mesh_indices.ToArray();

					mesh_filters[i].GetComponent<Renderer>().enabled = true;
					current_index += 60000;
				}
				else
				{
					Debug.Log("Mode 2 Index:" + i + " from:" + current_index + " to:" + (mesh_vertices.Count - current_index));
					mesh_filters[i].mesh.vertices = mesh_vertices.GetRange(current_index, mesh_vertices.Count - current_index).ToArray();
					mesh_filters[i].mesh.colors32 = mesh_colors.GetRange(current_index, mesh_vertices.Count - current_index).ToArray();
					mesh_indices.Clear();
					for (int j = 0; j < mesh_vertices.Count - current_index; j++) { mesh_indices.Add(j); }
					mesh_filters[i].mesh.triangles = mesh_indices.ToArray();

					mesh_filters[i].GetComponent<Renderer>().enabled = true;
					current_index += mesh_vertices.Count;
					break;
				}
			}

			is_update = false;
		}
	}

	private void VoxelThread()
	{ 
		while(is_thread)
		{
			if(!is_process) 
			{
				Thread.Sleep(10);
				continue;
			}

			NoiseReduction2();
			marching_cube.SetVoxels(colors, ref mesh_vertices, ref mesh_colors, ref mesh_indices);
			is_update = true;
			is_process = false;
		}
	}
}
