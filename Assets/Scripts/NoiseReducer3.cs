using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoiseReducer3 : MonoBehaviour
{
	public float focal_length = 120.0f;
	public int image_width = 160;
	public int image_height = 90;
	public float diff_threshold = 0.02f;
	public float modify_bias = 1.02f;

	public Texture2D depth_texture;
	private float[] depth_image;
	private float[] zero_image;
	private byte[] depth_byte;

	private byte[] mask_image;
	private byte[] depth_texture_image;

	private float correct_coeff = 0.0f;
	private Matrix4x4 debug_mat;

	// Use this for initialization
	void Start()
	{
		depth_texture = new Texture2D(image_width, image_height, TextureFormat.RGB24, false);
		depth_image = new float[image_width * image_height];
		zero_image = new float[image_width * image_height];
		depth_byte = new byte[image_width * image_height * 4];

		mask_image = new byte[image_width * image_height];
		depth_texture_image = new byte[image_width * image_height * 3];

		for (int i = 0; i < zero_image.Length; i++) { zero_image[i] = 0; }
		zero_image.CopyTo(depth_image, 0);

		debug_mat = new Matrix4x4();
	}

	public void PlotDepthImage(int in_num_vertices, Vector3[] in_vertices, Matrix4x4 current_mat)
	{
		// 1. Zero clear
		zero_image.CopyTo(depth_image, 0);
		debug_mat = current_mat;

		// 2. Plot previous point cloud
		for (int i = 0; i < in_num_vertices; i++)
		{
			// transform world from local
			Vector3 local_vertex = current_mat.inverse.MultiplyPoint(in_vertices[i]);
			//			Vector3 local_vertex = (current_mat.inverse * in_vertices[i]);
			if (local_vertex.z < 0.2f || local_vertex.z > 1.5f) continue;

			int x = (int)(local_vertex.x / local_vertex.z * focal_length) + image_width / 2;
			int y = (int)(local_vertex.y / local_vertex.z * focal_length) + image_height / 2;

			if (x < 0 || x >= image_width
			|| y < 0 || y >= image_height) { continue; }

			int target_index = image_width * y + x;

			if (depth_image[target_index] > 0) { depth_image[target_index] = depth_image[target_index] * 0.5f + local_vertex.z * 0.5f; }
			else { depth_image[target_index] = local_vertex.z; }
		}

		// for debug
		CopyToTexture();
	}

	public void ComputeCoeffcient(int in_num_vertices, Vector3[] in_vertices, List<Color32> in_colors, Matrix4x4 current_mat, ref List<Vector3> out_vertices, ref List<Color32> out_colors)
	{
		correct_coeff = 0.0f;
		int coeff_counter = 0;

		out_vertices.Clear();
		out_colors.Clear();

		for (int i = 0; i < in_num_vertices; i++)
		{
			// 3. transform new point cloud
			Vector3 local_vertex = current_mat.inverse.MultiplyPoint(in_vertices[i]);

			if (local_vertex.z < 0.2f || local_vertex.z > 1.5f) continue;

			int x = (int)(local_vertex.x / local_vertex.z * focal_length) + image_width / 2;
			int y = (int)(local_vertex.y / local_vertex.z * focal_length) + image_height / 2;

			if (x < 0 || x >= image_width
			|| y < 0 || y >= image_height) { continue; }

			int target_index = image_width * y + x;

			// 4. compare between new depth and previous depth
			if (depth_image[target_index] == 0) continue;
			else if (Mathf.Abs(1.0f - (depth_image[target_index] / local_vertex.z)) < diff_threshold)
			{
				correct_coeff += (depth_image[target_index] / local_vertex.z);
				coeff_counter++;
				out_vertices.Add(local_vertex);
				out_colors.Add(in_colors[i]);
			}
		}

		if (coeff_counter > 0) { correct_coeff = correct_coeff / coeff_counter; }
		else
		{
			correct_coeff = 0.0f;
			out_vertices.Clear();
			out_colors.Clear();
		}

		for (int i = 0; i < out_vertices.Count; i++)
		{
			out_vertices[i] = current_mat.MultiplyPoint(out_vertices[i] * correct_coeff);
		}
	}

	public void CopyToTexture()
	{
		for (int i = 0; i < depth_image.Length; i++) { depth_texture_image[i * 3] = (byte)(int)(depth_image[i] / 255.0f); }

		depth_texture.LoadRawTextureData(depth_texture_image);
		depth_texture.Apply();

		GetComponent<Renderer>().material.mainTexture = depth_texture;
//		Debug.Log("Coeff:" + correct_coeff);
//		Debug.Log("Mat:" + debug_mat);
	}
}
