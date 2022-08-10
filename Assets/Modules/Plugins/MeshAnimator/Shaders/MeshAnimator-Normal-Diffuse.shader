// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Mesh Animator shader source Copyright (c) 2020 JS Technologies, LLC

Shader "Mesh Animator/Legacy Shaders/Diffuse" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
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
    LOD 200

CGPROGRAM
#pragma surface surf Lambert addshadow
#pragma target 3.5

sampler2D _MainTex;
fixed4 _Color;

struct Input {
    float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
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
