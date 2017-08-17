Shader "Custom/VertexColor"
{
	Properties
	{
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float4 v_color : COLOR;
	};

	struct v2f
	{
		float4 v_color : COLOR;
		float4 vertex : SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.v_color = v.v_color;
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = (fixed4)i.v_color;
	return col;
	}
		ENDCG
	}
	}
}
