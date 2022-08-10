// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Mesh Animator shader source Copyright (c) 2020 JS Technologies, LLC

// Simplified Bumped Specular shader. Differences from regular Bumped Specular one:
// - no Main Color nor Specular Color
// - specular lighting directions are approximated per vertex
// - writes zero to alpha channel
// - Normalmap uses Tiling/Offset of the Base texture
// - no Deferred Lighting support
// - no Lightmap support
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mesh Animator/Mobile/Bumped Specular" {
Properties {
    _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
    _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
    [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
    [PerRendererData] _AnimTimeInfo ("Animation Time Info", Vector) = (0.0, 0.0, 0.0, 0.0)
    [PerRendererData] _AnimTextures ("Animation Textures", 2DArray) = "" {}
    [PerRendererData] _AnimTextureIndex ("Animation Texture Index", float) = -1.0
	[PerRendererData] _AnimInfo ("Animation Info", Vector) = (0.0, 0.0, 0.0, 0.0)
	[PerRendererData] _AnimScalar ("Animation Scalar", Vector) = (1.0, 1.0, 1.0, 0.0)
    [PerRendererData] _CrossfadeAnimTextureIndex ("Crossfade Texture Index", float) = -1.0
	[PerRendererData] _CrossfadeAnimInfo ("Crossfade Animation Info", Vector) = (0.0, 0.0, 0.0, 0.0)
	[PerRendererData] _CrossfadeAnimScalar ("Crossfade Animation Scalar", Vector) = (1.0, 1.0, 1.0, 0.0)
    [PerRendererData] _CrossfadeStartTime ("Crossfade Start Time", float) = -1.0
    [PerRendererData] _CrossfadeEndTime ("Crossfade End Time", float) = -1.0
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 250

	CGPROGRAM
	#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd halfasview interpolateview addshadow
	#pragma target 3.5

	inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
	{
		fixed diff = max (0, dot (s.Normal, lightDir));
		fixed nh = max (0, dot (s.Normal, halfDir));
		fixed spec = pow (nh, s.Specular*128) * s.Gloss;

		fixed4 c;
		c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
		UNITY_OPAQUE_ALPHA(c.a);
		return c;
	}

	sampler2D _MainTex;
	sampler2D _BumpMap;
	half _Shininess;

	struct Input {
		float2 uv_MainTex;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = tex.rgb;
		o.Gloss = tex.a;
		o.Alpha = tex.a;
		o.Specular = _Shininess;
		o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_MainTex));
	}
	// start MeshAnimator
	#include "MeshAnimator.cginc"
	#pragma vertex vert
	struct appdata_ma {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
		float4 texcoord1 : TEXCOORD1;
		float4 texcoord2 : TEXCOORD2;
		float4 texcoord3 : TEXCOORD3;
		float4 tangent : TANGENT;
		uint vertexId : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	void vert (inout appdata_ma v, out Input o)
	{
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_OUTPUT(Input, o);
		v.vertex = ApplyMeshAnimation(v.vertex, v.vertexId);		
		v.normal = GetAnimatedMeshNormal(v.normal, v.vertexId); 
		UNITY_TRANSFER_DEPTH(v); 
	}
	// end MeshAnimator
	ENDCG
}

}
