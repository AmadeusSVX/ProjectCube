using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCube : MonoBehaviour {

	public MeshFilter[] src_mesh_filters;
	public MeshFilter[] dst_mesh_filters;
	private Rigidbody rigid_body;

	// Use this for initialization
	void Start () {
		rigid_body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void EnableCopyCube()
	{
		for(int i=0; i< dst_mesh_filters.Length; i++)
		{
			dst_mesh_filters[i].mesh.Clear();
			dst_mesh_filters[i].mesh = Instantiate(src_mesh_filters[i].mesh);	// deep copy
			dst_mesh_filters[i].GetComponent<MeshRenderer>().enabled = true;
		}

		rigid_body.velocity = Vector3.zero;
		rigid_body.angularVelocity = Vector3.zero;
		rigid_body.isKinematic = false;
	}

	public void DisableCopyCube()
	{
		for(int i=0; i<dst_mesh_filters.Length; i++)
		{
			dst_mesh_filters[i].GetComponent<MeshRenderer>().enabled = false;
		}

		rigid_body.velocity = Vector3.zero;
		rigid_body.angularVelocity = Vector3.zero;
		rigid_body.isKinematic = true;
	}
}
