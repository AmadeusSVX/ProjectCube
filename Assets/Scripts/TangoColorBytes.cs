using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Tango;

public class TangoColorBytes : MonoBehaviour, ITangoVideoOverlay
{
	Texture2D byteTexture = null;

	// texture data
	bool isDirty = false;
	byte[] yv12 = null;

	byte[] y8 = null;
	byte[] uv4 = null;

	int width = 0;
	int height = 0;
	int stride = 0;

	void Start()
	{
		TangoApplication tangoApp = FindObjectOfType<TangoApplication>();
		tangoApp.Register(this);
	}

	public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId,
													TangoUnityImageData imageBuffer)
	{
		if (cameraId != Tango.TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR)
			return;

		// allocate for the first time
		width = (int)imageBuffer.width;
		height = (int)imageBuffer.height;
		stride = (int)imageBuffer.stride;

		if (yv12 == null) { yv12 = new byte[width * height * 2]; }
		if (y8 == null) { y8 = new byte[width * height]; }
		if (uv4 == null) { uv4 = new byte[width * height / 2]; }

		// copy data in yv12 format
		imageBuffer.data.CopyTo(yv12, 0);
		isDirty = true;

		System.Array.Copy(yv12, y8, width * height);
		System.Array.Copy(yv12, width * height, uv4, 0, width * height / 2);
	}

	public Color32 PickColor(float px, float py)
	{
		if (px < 0
		|| px >= width
		|| py < 0
		|| py >= height
		|| yv12 == null) { return new Color32(0, 0, 0, 0); }

		int size = (int)(width * height);
		byte y = yv12[(int)py * width + (int)px];
		byte v = yv12[(int)(py / 2) * width + ((int)(px/2)) * 2 + size];
		byte u = yv12[(int)(py / 2) * width + ((int)(px / 2)) * 2 + 1 + size];

//		return new Color32(v, v, v, 255);
		return YUV2Color(y, u, v);
	}

	public static Color32 YUV2Color(byte y, byte u, byte v)
	{
		// http://vision.kuee.kyoto-u.ac.jp/~hiroaki/firewire/yuv.html
		float y_scaled = (float)(int)y;
		float u_scaled = (float)(((int)u)-128);
		float v_scaled = (float)(((int)v)-128);

		float R = 1.000f * y_scaled + 1.402f * v_scaled;
		float G = 1.000f * y_scaled - 0.344f * u_scaled - 0.714f * v_scaled;
		float B = 1.000f * y_scaled + 1.772f * u_scaled;

		if (R > 255.0f) R = 255.0f;
		if (G > 255.0f) G = 255.0f;
		if (B > 255.0f) B = 255.0f;

		return new Color32((byte)(int)R,
						   (byte)(int)G,
						   (byte)(int)B,
						   255);

		/*
				// http://en.wikipedia.org/wiki/YUV
				const float Umax = 0.436f;
				const float Vmax = 0.615f;

				float y_scaled = ((float)(int)y) / 255.0f;
				float u_scaled = 2 * (((float)(((int)u))) / 255.0f - 0.5f) * Umax;
				float v_scaled = 2 * (((float)(((int)v))) / 255.0f - 0.5f) * Vmax;

				return new Color32((byte)(int)((y_scaled + 1.13983f * v_scaled) * 255),
								   (byte)(int)((y_scaled - 0.39465f * u_scaled - 0.58060f * v_scaled) * 255),
								   (byte)(int)((y_scaled + 2.03211f * u_scaled) * 255),
								   255);
		*/
	}

	private void Update()
	{
		if (byteTexture == null && width > 0 && height > 0)
		{
			byteTexture = new Texture2D(width, height/2, TextureFormat.Alpha8, false);
		}

		byteTexture.LoadRawTextureData(uv4);
		byteTexture.Apply();
		GetComponent<Renderer>().material.mainTexture = byteTexture;
	}
}