using UnityEngine;

public class InfoOutput : MonoBehaviour
{
	public string info_text;
	TextMesh text_mesh;

	// Use this for initialization
	void Start()
	{
		text_mesh = gameObject.GetComponent<TextMesh>();
	}

	void Update()
	{
		text_mesh.text = info_text;
	}
}