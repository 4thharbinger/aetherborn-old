void MainLight_half(float3 Normal, float3 WorldPos, out half3 Direction, out half3 Color, out half Attenuation, out float3 AdditionalLighting)
{
	AdditionalLighting = float3(0, 0, 0);
#if SHADERGRAPH_PREVIEW
	Direction = half3(0.5, 0.5, 0);
	Color = 1;
	Attenuation = 1;
#else
#if SHADOWS_SCREEN
	half4 clipPos = TransformWorldToHClip(WorldPos);
	half4 shadowCoord = ComputeScreenPos(clipPos);
#else
	half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPos));
    AmbientOcclusionFactor SSAO = GetScreenSpaceAmbientOcclusion(screenPos.xy / screenPos.w);
	InputData inputData = (InputData)0;
	Light mainLight = GetMainLight(shadowCoord);
	Direction = mainLight.direction;
    Attenuation = (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	Color = mainLight.color;
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPos;
	int lightCount = GetAdditionalLightsCount();
	LIGHT_LOOP_BEGIN(lightCount)
	//for (uint lightIndex = 0; i < lightCount; i++) {
		Light addLight = GetAdditionalLight(lightIndex, WorldPos);
		
		//Attenuation += length(addLight.color) * addLight.distanceAttenuation * addLight.shadowAttenuation;
	
		AdditionalLighting += addLight.color * addLight.distanceAttenuation * AdditionalLightRealtimeShadow(lightIndex, WorldPos, addLight.direction) * clamp(dot(addLight.direction, Normal), 0, 1);
	//}
    LIGHT_LOOP_END
	
	//Attenuation = Attenuation + (SSAO.directAmbientOcclusion * SSAO.indirectAmbientOcclusion);
#endif
}