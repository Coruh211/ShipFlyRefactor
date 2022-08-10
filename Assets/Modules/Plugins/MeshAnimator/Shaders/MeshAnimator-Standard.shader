// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Mesh Animator/Standard"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

		// start MeshAnimator
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
		// end MeshAnimator
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma fragment fragBase addshadow
            #include "UnityStandardCoreForward.cginc"

			// start meshanimator
            #pragma vertex vertMABase
			#include "MeshAnimator.cginc"
			struct MAVertexInput
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float2 uv0      : TEXCOORD0;
				float2 uv1      : TEXCOORD1;
				fixed4 color    : COLOR;
				#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
					float2 uv2      : TEXCOORD2;
				#endif
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					half4 tangent   : TANGENT;
				#endif
				uint vertexId : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
#if UNITY_STANDARD_SIMPLE
			VertexOutputBaseSimple vertMABase (MAVertexInput v) {
				VertexOutputBaseSimple o;
				UNITY_INITIALIZE_OUTPUT(VertexOutputBaseSimple, o);
#else
			VertexOutputForwardBase vertMABase (MAVertexInput v) {
				VertexOutputForwardBase o;
				UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
#endif
				VertexInput vertexInput;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_TRANSFER_INSTANCE_ID(v, vertexInput);
				vertexInput.uv0 = v.uv0;
				vertexInput.uv1 = v.uv1;
				
				#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
					vertexInput.uv2 = v.uv2;
				#endif

				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					vertexInput.tangent = v.tangent;
				#endif
				v.vertex = ApplyMeshAnimation(v.vertex, v.vertexId);
				v.normal = GetAnimatedMeshNormal(v.normal, v.vertexId); 
				vertexInput.vertex = v.vertex;
				vertexInput.normal = v.normal;
				return vertBase(vertexInput);
			}
			// end meshanimator

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAdd addshadow
            #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE
			
            #include "UnityStandardShadow.cginc"

            #pragma fragment fragShadowCaster addshadow
			
			#include "MeshAnimator.cginc"
            #pragma vertex vertMAShadowCaster
			struct MAVertexInput
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float2 uv0      : TEXCOORD0;
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					half4 tangent   : TANGENT;
				#endif
				uint vertexId : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			void vertMAShadowCaster (MAVertexInput v
				, out float4 opos : SV_POSITION
				#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
				, out VertexOutputShadowCaster o
				#endif
				#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
				, out VertexOutputStereoShadowCaster os
				#endif
			)
			{
				VertexInput vertexInput;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, vertexInput);
				vertexInput.uv0 = v.uv0;
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					vertexInput.tangent = v.tangent;
				#endif
				v.vertex = ApplyMeshAnimation(v.vertex, v.vertexId);
				v.normal = GetAnimatedMeshNormal(v.normal, v.vertexId); 
				vertexInput.vertex = v.vertex;
				vertexInput.normal = v.normal;
				vertShadowCaster(vertexInput, 
					opos
					#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
						, o
					#endif
					#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
						, os
					#endif
					);
			}
            ENDCG
        }
        // ------------------------------------------------------------------
        //  Deferred pass
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferred
            #pragma fragment fragDeferred addshadow

            #include "UnityStandardCore.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta addshadow

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            // SM2.0: NOT SUPPORTED shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma fragment fragBase addshadow
            #include "UnityStandardCoreForward.cginc"

			// start meshanimator
            #pragma vertex vertMABase
			#include "MeshAnimator.cginc"
			struct MAVertexInput
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float2 uv0      : TEXCOORD0;
				float2 uv1      : TEXCOORD1;
				#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
					float2 uv2      : TEXCOORD2;
				#endif
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					half4 tangent   : TANGENT;
				#endif
				uint vertexId : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
#if UNITY_STANDARD_SIMPLE
			VertexOutputBaseSimple vertMABase (MAVertexInput v) {
				VertexOutputBaseSimple o;
				UNITY_INITIALIZE_OUTPUT(VertexOutputBaseSimple, o);
#else
			VertexOutputForwardBase vertMABase (MAVertexInput v) {
				VertexOutputForwardBase o;
				UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
#endif
				VertexInput vertexInput;
				UNITY_SETUP_INSTANCE_ID(vertexInput);
				vertexInput.uv0 = v.uv0;
				vertexInput.uv1 = v.uv1;				
				#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
					vertexInput.uv2 = v.uv2;
				#endif
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					vertexInput.tangent = v.tangent;
				#endif
				v.vertex = ApplyMeshAnimation(v.vertex, v.vertexId);
				v.normal = GetAnimatedMeshNormal(v.normal, v.vertexId); 
				vertexInput.vertex = v.vertex;
				vertexInput.normal = v.normal;
				return vertBase(vertexInput);
			}
			// end meshanimator

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma fragment fragAdd addshadow
            #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster
			
           
            #include "UnityStandardShadow.cginc"

            #pragma fragment fragShadowCaster addshadow
			
			#include "MeshAnimator.cginc"
            #pragma vertex vertMAShadowCaster
			struct MAVertexInput
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float2 uv0      : TEXCOORD0;
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					half4 tangent   : TANGENT;
				#endif
				uint vertexId : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			void vertMAShadowCaster (MAVertexInput v
				, out float4 opos : SV_POSITION
				#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
				, out VertexOutputShadowCaster o
				#endif
				#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
				, out VertexOutputStereoShadowCaster os
				#endif
			)
			{
				VertexInput vertexInput;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, vertexInput);
				vertexInput.uv0 = v.uv0;
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
					vertexInput.tangent = v.tangent;
				#endif
				v.vertex = ApplyMeshAnimation(v.vertex, v.vertexId);
				v.normal = GetAnimatedMeshNormal(v.normal, v.vertexId); 
				vertexInput.vertex = v.vertex;
				vertexInput.normal = v.normal;
				vertShadowCaster(vertexInput, 
					opos
					#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
						, o
					#endif
					#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
						, os
					#endif
					);
			}


            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta addshadow

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }


    FallBack "VertexLit"
    CustomEditor "StandardShaderGUI"
}
