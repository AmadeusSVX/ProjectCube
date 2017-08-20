using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoiseReducer : MonoBehaviour
{
	public float focal_length = 60.0f;
	public int image_width = 44;
	public int image_height = 80;
	public float diff_threshold = 0.02f;
	public int num_threshold = 2;

	public Texture2D mask_texture;
	private float[] max_image;
	private float[] min_image;
	private int[] num_image;

	private float[] zero_image1;
	private int[] zero_image2;
	private float[] far_image;
	private byte[] mask_image;
	private byte[] mask_texture_image;

	private List<int> vertices_indices;

	// Use this for initialization
	void Start()
	{
		mask_texture = new Texture2D(image_width, image_height, TextureFormat.RGB24, false);

		max_image = new float[image_width * image_height];
		min_image = new float[image_width * image_height];
		num_image = new int[image_width * image_height];

		far_image = new float[image_width * image_height];
		zero_image1 = new float[image_width * image_height];
		zero_image2 = new int[image_width * image_height];

		mask_image = new byte[image_width * image_height];
		mask_texture_image = new byte[image_width * image_height * 3];

		for (int i = 0; i < zero_image1.Length; i++) { zero_image1[i] = 0; }
		for (int i = 0; i < zero_image2.Length; i++) { zero_image2[i] = 0; }
		for (int i = 0; i < far_image.Length; i++) { far_image[i] = 10000.0f; }
		for (int i = 0; i < mask_texture_image.Length; i++) { mask_texture_image[i] = 0; }

		zero_image1.CopyTo(max_image, 0);
		far_image.CopyTo(min_image, 0);
		zero_image2.CopyTo(num_image, 0);

		vertices_indices = new List<int>();
	}

	public void ReduceNoise(int in_num_vertices, Vector3[] in_vertices, List<Color32> in_colors, Matrix4x4 current_mat, ref List<Vector3> out_vertices, ref List<Color32> out_colors)
	{
		// 1. Zero clear
		zero_image1.CopyTo(max_image, 0);
		far_image.CopyTo(min_image, 0);
		out_vertices.Clear();
		out_colors.Clear();
		zero_image2.CopyTo(num_image, 0);
		int zero_counter = 0;

		// 2. Plot max and min point cloud
		for (int i = 0; i < in_num_vertices; i++)
		{
			int random_index = i;// random_value.Next(0, in_vertices.Count - 1);

			// transform world from local
			Vector3 local_vertex = current_mat.inverse.MultiplyPoint(in_vertices[i]);
			//			Vector3 local_vertex = (current_mat.inverse * in_vertices[i]);
			if (local_vertex.z == 0 || local_vertex.z > 10.0f)
			{
				vertices_indices.Add(-1);
				continue;
			}

			int x = (int)(local_vertex.x / local_vertex.z * focal_length) + image_width / 2;
			int y = (int)(local_vertex.y / local_vertex.z * focal_length) + image_height / 2;

			if (x < 0 || x >= image_width
			|| y < 0 || y >= image_height) 
			{
				vertices_indices.Add(-1);
				zero_counter++;
				continue; 
			}

			int target_index = image_width * y + x;
			vertices_indices.Add(target_index);

			if (max_image[target_index] < local_vertex.z) { max_image[target_index] = local_vertex.z; }
			if (min_image[target_index] > local_vertex.z) { min_image[target_index] = local_vertex.z; }

			num_image[target_index]++;
		}

		// 3. Generate mask
		for(int i = 0; i< image_width * image_height; i++)
		{
			if (max_image[i] > 0 && max_image[i] - min_image[i] < diff_threshold && num_image[i] > num_threshold){ mask_image[i] = 255; }
			else { mask_image[i] = 0; }
		}

		// 4. Evaluate each points
		for (int i = 0; i < in_num_vertices; i++)
		{
			if(vertices_indices[i] < 0) { continue; }
			if(mask_image[vertices_indices[i]] > 0) 
			{
				out_vertices.Add(in_vertices[i]);
				out_colors.Add(in_colors[i]);
			}
		}

		// 5. output texture for debug
		CopyToTexture();
//		Debug.Log("Copying:" + out_vertices.Count + " / " + in_num_vertices);
//		Debug.Log("Zero:" + zero_counter);
	}


	public void CopyToTexture()
	{
		for (int i = 0; i < mask_image.Length; i++) { mask_texture_image[i * 3] = mask_image[i]; }

		mask_texture.LoadRawTextureData(mask_texture_image);
		mask_texture.Apply();

		GetComponent<Renderer>().material.mainTexture = mask_texture;
	}
}
