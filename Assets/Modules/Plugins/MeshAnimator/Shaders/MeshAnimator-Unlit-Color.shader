// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Mesh Animator shader source Copyright (c) 2020 JS Technologies, LLC

// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Mesh Animator/Unlit/Color" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
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
    LOD 100

    Pass {
        CGPROGRAM
            #pragma fragment frag addshadow
            #pragma target 3.5
            #pragma multi_compile_fog
			#pragma multi_compile_instancing

            #include "UnityCG.cginc"
			
            struct v2f {
                float4 vertex : SV_POSITION;
                UNITY_FOG_COORDS(0)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
			
			// start MeshAnimator
			#include "MeshAnimator.cginc"
			#pragma vertex vert
			struct appdata_ma {
                float4 vertex : POSITION;
				uint vertexId : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert (appdata_ma v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(ApplyMeshAnimation(v.vertex, v.vertexId));
				UNITY_TRANSFER_DEPTH(v); 
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			// end MeshAnimator

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = _Color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
}

}
