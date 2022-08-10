#pragma multi_compile_instancing
#pragma require 2darray
#pragma target 3.5

UNITY_DECLARE_TEX2DARRAY(_AnimTextures);
UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(float, _AnimTextureIndex)
	UNITY_DEFINE_INSTANCED_PROP(float4, _AnimTimeInfo)
	UNITY_DEFINE_INSTANCED_PROP(float4, _AnimInfo)
	UNITY_DEFINE_INSTANCED_PROP(float4, _AnimScalar)
	UNITY_DEFINE_INSTANCED_PROP(float, _CrossfadeAnimTextureIndex)
	UNITY_DEFINE_INSTANCED_PROP(float4, _CrossfadeAnimInfo)
	UNITY_DEFINE_INSTANCED_PROP(float4, _CrossfadeAnimScalar)
	UNITY_DEFINE_INSTANCED_PROP(float, _CrossfadeStartTime)
	UNITY_DEFINE_INSTANCED_PROP(float, _CrossfadeEndTime)
UNITY_INSTANCING_BUFFER_END(Props)

inline float GetPixelOffset(inout float textureIndex, float4 animInfo, float4 animTimeInfo)
{
	float normalizedTime = (_Time.y - animTimeInfo.z) / (animTimeInfo.w - animTimeInfo.z);
	normalizedTime = normalizedTime - floor(normalizedTime);
	float currentFrame = min(floor(normalizedTime * animTimeInfo.y), animTimeInfo.y - 1);
	float vertexCount = animInfo.y;
	float textureSizeX = animInfo.z;
	float textureSizeY = animInfo.w;

	float framesPerTexture = floor((textureSizeX * textureSizeY) / (vertexCount * 2));
    float localOffset = floor(currentFrame / framesPerTexture);
    textureIndex = floor(textureIndex + localOffset);
	float frameOffset = floor(currentFrame % framesPerTexture);
    float pixelOffset = vertexCount * 2 * frameOffset;
	return pixelOffset;
}

inline float3 GetUVPos(uint vertexIndex, float textureIndex, float pixelOffset, float textureSizeX, float textureSizeY, uint offset)
{
	uint vertexOffset = pixelOffset + (vertexIndex * 2);
	vertexOffset += offset;
	float offsetX = floor(vertexOffset / textureSizeX);
	float offsetY = vertexOffset - (offsetX * textureSizeY);
	float3 uvPos = float3(offsetX / textureSizeX, offsetY / textureSizeY, textureIndex);
	return uvPos;
}

inline float3 GetAnimationUVPosition(uint vertexIndex, float textureIndex, float4 animInfo, float4 animTimeInfo, uint offset)
{
    float pixelOffset = GetPixelOffset(textureIndex, animInfo, animTimeInfo);
	return GetUVPos(vertexIndex, textureIndex, pixelOffset, animInfo.z, animInfo.w, offset);
}

inline float3 GetCrossfadeUVPosition(uint vertexIndex, float textureIndex, float4 animInfo)
{
	float pixelOffset = animInfo.x;
	return GetUVPos(vertexIndex, textureIndex, pixelOffset, animInfo.z, animInfo.w, 0);
}

inline float4 DecodeNegativeVectors(float4 positionData)
{
	positionData = float4((positionData.x - 0.5) * 2, (positionData.y - 0.5) * 2, (positionData.z - 0.5) * 2, 1);
	return positionData;
}

inline float4 ApplyAnimationScalar(float4 positionData, float4 animScalar)
{
	positionData = DecodeNegativeVectors(positionData);
	positionData.xyz *= animScalar.xyz;
	return positionData;
}

inline float4 ApplyMeshAnimation(float4 position, uint vertexId)
{
	float index = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTextureIndex);
	if (index >= 0)
	{
		float4 animInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo);
		float4 animTimeInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTimeInfo);

		float3 uvPos = GetAnimationUVPosition(vertexId, index, animInfo, animTimeInfo, 0);
		float4 positionData = UNITY_SAMPLE_TEX2DARRAY_LOD(_AnimTextures, uvPos, 0);
		float4 animScalar = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimScalar);
		positionData = ApplyAnimationScalar(positionData, animScalar);

		float crossfadeEndTime = UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeEndTime);
		if (_Time.y < crossfadeEndTime)
		{
			float cfIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimTextureIndex);
			float4 cfAnimInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimInfo);
			
			uvPos = GetCrossfadeUVPosition(vertexId, cfIndex, cfAnimInfo);
			float4 crossfadePositionData = UNITY_SAMPLE_TEX2DARRAY_LOD(_AnimTextures, uvPos, 0);
			animScalar = UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimScalar);
			crossfadePositionData = ApplyAnimationScalar(crossfadePositionData, animScalar);
		 
			float crossfadeStartTime = UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeStartTime);
			float lerpValue = (_Time.y - crossfadeStartTime) / (crossfadeEndTime - crossfadeStartTime);
			positionData = lerp(crossfadePositionData, positionData, lerpValue);
		}
		position = positionData;
	}
	return position;
}

inline float3 GetAnimatedMeshNormal(float3 normal, uint vertexId)
{
	float index = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTextureIndex);
	if (index >= 0)
	{
		float4 animInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo);
		float4 animTimeInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTimeInfo);

		float3 uvPos = GetAnimationUVPosition(vertexId, index, animInfo, animTimeInfo, 1);
		float4 normalData = UNITY_SAMPLE_TEX2DARRAY_LOD(_AnimTextures, uvPos, 0);
		normalData = DecodeNegativeVectors(normalData);
		if (normalData.x != 0 || normalData.y != 0 || normalData.z != 0)
			normal = normalData.xyz;
	}
	return normal;
}