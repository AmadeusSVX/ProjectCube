using System;
using System.Collections.Generic;
using UnityEngine;

public class DensityFieldGeneratorCPU : MonoBehaviour
{
	public int Resolution;

	private MarchingCubesCPU mc;

	private Mesh _mesh;
	private List<Vector3> mesh_vertices;
	private List<Color32> mesh_colors;
	private List<int> mesh_indices;

	private Color32[] colors;

	private void Awake()
	{
		_mesh = GetComponent<MeshFilter>().mesh;
		colors = new Color32[Resolution * Resolution * Resolution];

		mesh_vertices = new List<Vector3>();
		mesh_colors = new List<Color32>();
		mesh_indices = new List<int>();

		mc = new MarchingCubesCPU();
		mc._gridSize = Resolution;
	}

	private void Start()
	{
		GenerateSoil();
	}

	private void Update()
	{
		GenerateSoil();
		UpdateSoil();
	}

	private void GenerateSoil()
	{
		var idx = 0;
		for (var z = 0; z < Resolution; ++z)
		{
			for (var y = 0; y < Resolution; ++y)
			{
				for (var x = 0; x < Resolution; ++x, ++idx)
				{
					var amount = Mathf.Pow(x - Resolution / 2, 2) + Mathf.Pow(y - Resolution / 2, 2) + Mathf.Pow(z - Resolution / 2, 2)
								<= Mathf.Pow((Resolution - 2) / 2 * Mathf.Sin(0.25f * Time.time), 2) ? 1 : 0;
					colors[idx] = new Color32((byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)(amount * 255));
					//						colors[idx] = new Color32((byte)255, 0, 0, (byte)(amount * 255));
				}
			}
		}
	}

	private void UpdateSoil()
	{
		mc.SetVoxels(colors, ref mesh_vertices, ref mesh_colors, ref mesh_indices);

		_mesh.triangles = null;
		_mesh.vertices = mesh_vertices.ToArray();
		_mesh.triangles = mesh_indices.ToArray();
		_mesh.colors32 = mesh_colors.ToArray();
	}
}
