Shader "Stylized/Planet"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalTex ("Texture", 2D) = "normal" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN _FORWARD_PLUS

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "Assets/Shaders/CelShading.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                half3 normal        : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 position : WORLD_POSITION;
                half3 normal        : TEXCOORD0;
                float4 shadowCoords : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                OUT.position = TransformObjectToWorld(IN.positionOS.xyz);
                
                // Get the VertexPositionInputs for the vertex position  
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);
                // Convert the vertex position to a position on the shadow map
                float4 shadowCoordinates = GetShadowCoord(positions);

                // Pass the shadow coordinates to the fragment shader
                OUT.shadowCoords = shadowCoordinates;

                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
	            float3 additionalLighting = 0;

                #if SHADOWS_SCREEN
	                half4 shadowCoord = ComputeScreenPos(IN.positionHCS);
                #else
	                half4 shadowCoord = TransformWorldToShadowCoord(IN.position);
                #endif
                float4 screenPos = ComputeScreenPos(TransformWorldToHClip(IN.position));
                // AmbientOcclusionFactor SSAO = GetScreenSpaceAmbientOcclusion(screenPos.xy / screenPos.w);

	            Light mainLight = GetMainLight(shadowCoord);
                half Attenuation = MainLightRealtimeShadow(shadowCoord);
	            half3 color = mainLight.color * dot(mainLight.direction, IN.normal) * mainLight.shadowAttenuation;

	            int lightCount = GetAdditionalLightsCount();

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.position;
                LIGHT_LOOP_BEGIN(lightCount)
		            // Light addLight = GetAdditionalLight(lightIndex, IN.position);
		
		            // Attenuation += length(addLight.color) * addLight.distanceAttenuation * addLight.shadowAttenuation;
	
		            // additionalLighting += addLight.color * addLight.distanceAttenuation * AdditionalLightRealtimeShadow(lightIndex, IN.position, addLight.direction) * clamp(dot(addLight.direction, IN.normal), 0, 1);
                LIGHT_LOOP_END

                return Attenuation;
            }
            ENDHLSL
        }
        
        Pass {
	        Name "ShadowCaster"
	        Tags { "LightMode"="ShadowCaster" }

	        ZWrite On
	        ZTest LEqual

	        HLSLPROGRAM
	        #pragma vertex ShadowPassVertex
	        #pragma fragment ShadowPassFragment

	        // Material Keywords
	        #pragma shader_feature _ALPHATEST_ON
	        #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

	        // GPU Instancing
	        #pragma multi_compile_instancing
	        // (Note, this doesn't support instancing for properties though. Same as URP/Lit)
	        // #pragma multi_compile _ DOTS_INSTANCING_ON
	        // (This was handled by LitInput.hlsl. I don't use DOTS so haven't bothered to support it)
             
	        //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
	        ENDHLSL
        }
        Pass {
	        Name "DepthOnly"
	        Tags { "LightMode"="DepthOnly" }

	        ColorMask 0
	        ZWrite On
	        ZTest LEqual

	        HLSLPROGRAM
	        #pragma vertex DepthOnlyVertex
	        #pragma fragment DepthOnlyFragment

	        // Material Keywords
	        #pragma shader_feature _ALPHATEST_ON
	        #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

	        // GPU Instancing
	        #pragma multi_compile_instancing
	        // #pragma multi_compile _ DOTS_INSTANCING_ON

	        //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
	        ENDHLSL
        }
        Pass {
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex DepthNormalsVertex
			#pragma fragment DepthNormalsFragment

			// Material Keywords
			#pragma shader_feature_local _NORMALMAP
			//#pragma shader_feature_local _PARALLAXMAP
			//#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// GPU Instancing
			#pragma multi_compile_instancing
			//#pragma multi_compile _ DOTS_INSTANCING_ON

			//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
	
			ENDHLSL
		}
    }
}
