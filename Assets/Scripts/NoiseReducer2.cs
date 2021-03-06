﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoiseReducer2 : MonoBehaviour
{
	public float focal_length = 60.0f;
	public int image_width = 44;
	public int image_height = 80;
	public float diff_threshold = 0.02f;
	public int num_threshold = 2;
	public float modify_bias = 1.02f;

	public Texture2D color_texture;
	private float[] max_image;
	private float[] min_image;
	private int[] num_image;
	private float[] ave_image;
	private Color[] color_image;

	private float[] zero_floats;
	private int[] zero_ints;
	private Color[] zero_colors;

	private float[] far_image;

	private List<Vector3> local_vertices;
	private List<int> vertices_indices;

	// Use this for initialization
	void Start()
	{
		color_texture = new Texture2D(image_width, image_height, TextureFormat.ARGB32, false);

		max_image = new float[image_width * image_height];
		min_image = new float[image_width * image_height];
		num_image = new int[image_width * image_height];
		ave_image = new float[image_width * image_height];
		color_image = new Color[image_width * image_height];

		far_image = new float[image_width * image_height];
		zero_floats = new float[image_width * image_height];
		zero_ints = new int[image_width * image_height];
		zero_colors = new Color[image_width * image_height];

		for (int i = 0; i < zero_floats.Length; i++) { zero_floats[i] = 0; }
		for (int i = 0; i < zero_ints.Length; i++) { zero_ints[i] = 0; }
		for (int i = 0; i < far_image.Length; i++) { far_image[i] = 10000.0f; }
		for (int i = 0; i < zero_colors.Length; i++) { zero_colors[i] = new Color(0, 0, 0, 1); }

		zero_floats.CopyTo(max_image, 0);
		far_image.CopyTo(min_image, 0);
		zero_ints.CopyTo(num_image, 0);

		local_vertices = new List<Vector3>();
		vertices_indices = new List<int>();
	}

	public void ReduceNoise(int in_num_vertices, Vector3[] in_vertices, List<Color32> in_colors, Matrix4x4 current_mat, ref List<Vector3> out_vertices, ref List<Color32> out_colors)
	{
		// 1. Zero clear
		zero_floats.CopyTo(max_image, 0);
		far_image.CopyTo(min_image, 0);
		zero_floats.CopyTo(ave_image, 0);
		zero_colors.CopyTo(color_image, 0);

		out_vertices.Clear();
		out_colors.Clear();
		local_vertices.Clear();
		zero_ints.CopyTo(num_image, 0);
		int zero_counter = 0;

		int i = 0;
		// 2. Plot max and min point cloud
		for (i = 0; i < in_num_vertices; i++)
		{
			int random_index = i;// random_value.Next(0, in_vertices.Count - 1);

			// transform world from local
			Vector3 local_vertex = current_mat.inverse.MultiplyPoint(in_vertices[i]);
			local_vertices.Add(local_vertex);

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

			// update images
			ave_image[target_index] += local_vertex.z;
			color_image[target_index] = new Color(color_image[target_index].r + ((int)in_colors[i].r) / 255.0f,
												  color_image[target_index].g + ((int)in_colors[i].g) / 255.0f,
												  color_image[target_index].b + ((int)in_colors[i].b) / 255.0f);
			if (max_image[target_index] < local_vertex.z) { max_image[target_index] = local_vertex.z; }
			if (min_image[target_index] > local_vertex.z) { min_image[target_index] = local_vertex.z; }

			num_image[target_index]++;
		}

		// 3. Evaluete image and generate points
		i = 0;
		for (int y = 0; y < image_height; y++)
		{
			for (int x = 0; x < image_width; x++, i++)
			{
				if (max_image[i] > 0 && max_image[i] - min_image[i] < diff_threshold && num_image[i] > num_threshold)
				{
					float new_z = ave_image[i] / num_image[i] * modify_bias;
					Vector3 new_vertex = current_mat.MultiplyPoint(new Vector3((x - image_width / 2) * new_z / focal_length,
																				(y - image_height / 2) * new_z / focal_length,
																				new_z));
					color_image[i] = color_image[i] / num_image[i];
					Color32 new_color = new Color32((byte)(int)(color_image[i].r * 255.0f),
													(byte)(int)(color_image[i].g * 255.0f),
													(byte)(int)(color_image[i].b * 255.0f),
													255);
					out_vertices.Add(new_vertex);
					out_colors.Add(new_color);
				}
			}
		}

		// 5. output texture for debug
		CopyToTexture();
		//		Debug.Log("Copying:" + out_vertices.Count + " / " + in_num_vertices);
		//		Debug.Log("Zero:" + zero_counter);
	}


	public void CopyToTexture()
	{
		color_texture.SetPixels(color_image);
		color_texture.Apply();

		GetComponent<Renderer>().material.mainTexture = color_texture;
	}
}
