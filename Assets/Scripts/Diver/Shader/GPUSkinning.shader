Shader "Custom/GPUSkinning"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _AnimationTex ("Animation Texture", 2D) = "white" {}
        _BoneCount ("Bone Count", Int) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                uint vertexId : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO 
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;
            
            TEXTURE2D(_AnimationTex);
            SAMPLER(sampler_AnimationTex);
            float4 _AnimationTex_TexelSize;
            uint _BoneCount;
            
            struct BoneWeightData
            {
                int4 boneIndices;
                float4 weights;
            };

            struct ClipInfo
            {
                int startFrame;
                int frameCount;
                float duration;
            };
            
            StructuredBuffer<BoneWeightData> _BoneWeights;
            StructuredBuffer<float2> _AnimationTimes;
            StructuredBuffer<ClipInfo> _ClipInfos;

            float4 SampleAnimTexture(uint x, uint y)
            {
                float2 uv = float2((x + 0.5) * _AnimationTex_TexelSize.x, (y + 0.5) * _AnimationTex_TexelSize.y);
                return SAMPLE_TEXTURE2D_LOD(_AnimationTex, sampler_AnimationTex, uv, 0);
            }

            float4x4 GetBoneMatrix(uint boneIndex, float animTime, int clipIndex)
            {
                ClipInfo clip = _ClipInfos[clipIndex];
                
                float frameRate = 30;
                float frameFloat = animTime * frameRate;
                uint localFrame0 = (uint)frameFloat % (uint)clip.frameCount;
                uint localFrame1 = (localFrame0 + 1) % (uint)clip.frameCount;
                float lerpFactor = frac(frameFloat);
                
                uint frame0 = clip.startFrame + localFrame0;
                uint frame1 = clip.startFrame + localFrame1;
                
                uint xBase = boneIndex * 4;
                
                float4 col0_f0 = SampleAnimTexture(xBase + 0, frame0);
                float4 col1_f0 = SampleAnimTexture(xBase + 1, frame0);
                float4 col2_f0 = SampleAnimTexture(xBase + 2, frame0);
                float4 col3_f0 = SampleAnimTexture(xBase + 3, frame0);
                
                float4 col0_f1 = SampleAnimTexture(xBase + 0, frame1);
                float4 col1_f1 = SampleAnimTexture(xBase + 1, frame1);
                float4 col2_f1 = SampleAnimTexture(xBase + 2, frame1);
                float4 col3_f1 = SampleAnimTexture(xBase + 3, frame1);
                
                float4 col0 = lerp(col0_f0, col0_f1, lerpFactor);
                float4 col1 = lerp(col1_f0, col1_f1, lerpFactor);
                float4 col2 = lerp(col2_f0, col2_f1, lerpFactor);
                float4 col3 = lerp(col3_f0, col3_f1, lerpFactor);
                
                return float4x4(
                    col0.x, col1.x, col2.x, col3.x,
                    col0.y, col1.y, col2.y, col3.y,
                    col0.z, col1.z, col2.z, col3.z,
                    col0.w, col1.w, col2.w, col3.w
                );
            }

            float4 SkinPosition(float4 pos, uint vertexId, float animTime, int clipIndex)
            {
                BoneWeightData bw = _BoneWeights[vertexId];
                
                float4 skinnedPos = float4(0, 0, 0, 0);
                
                if (bw.weights.x > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.x, animTime, clipIndex);
                    skinnedPos += mul(m, pos) * bw.weights.x;
                }
                if (bw.weights.y > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.y, animTime, clipIndex);
                    skinnedPos += mul(m, pos) * bw.weights.y;
                }
                if (bw.weights.z > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.z, animTime, clipIndex);
                    skinnedPos += mul(m, pos) * bw.weights.z;
                }
                if (bw.weights.w > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.w, animTime, clipIndex);
                    skinnedPos += mul(m, pos) * bw.weights.w;
                }
                
                return skinnedPos;
            }

            float3 SkinNormal(float3 normal, uint vertexId, float animTime, int clipIndex)
            {
                BoneWeightData bw = _BoneWeights[vertexId];
                
                float3 skinnedNormal = float3(0, 0, 0);
                
                if (bw.weights.x > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.x, animTime, clipIndex);
                    skinnedNormal += mul((float3x3)m, normal) * bw.weights.x;
                }
                if (bw.weights.y > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.y, animTime, clipIndex);
                    skinnedNormal += mul((float3x3)m, normal) * bw.weights.y;
                }
                if (bw.weights.z > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.z, animTime, clipIndex);
                    skinnedNormal += mul((float3x3)m, normal) * bw.weights.z;
                }
                if (bw.weights.w > 0)
                {
                    float4x4 m = GetBoneMatrix(bw.boneIndices.w, animTime, clipIndex);
                    skinnedNormal += mul((float3x3)m, normal) * bw.weights.w;
                }
                
                return skinnedNormal;
            }

            v2f vert(appdata v, uint instanceId : SV_InstanceID)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float2 animData = _AnimationTimes[instanceId];
                float animTime = animData.x;
                int clipIndex = (int)animData.y;
                
                if (animTime > 0)
                {
                    float4 skinnedPos = SkinPosition(v.vertex, v.vertexId, animTime, clipIndex);
                    float3 skinnedNormal = normalize(SkinNormal(v.normal, v.vertexId, animTime, clipIndex));

                    o.pos = TransformObjectToHClip(skinnedPos.xyz);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.worldNormal = TransformObjectToWorldNormal(skinnedNormal);
                }
                else
                {
                    o.pos = TransformObjectToHClip(v.vertex.xyz);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.worldNormal = TransformObjectToWorldNormal(v.normal);
                }

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                
                Light mainLight = GetMainLight();
                
                float3 normal = normalize(i.worldNormal);
                float ndotl = max(0, dot(normal, mainLight.direction));

                half3 diffuse = ndotl * mainLight.color;

                col.rgb *= diffuse;
                
                return col;
            }
            ENDHLSL
        }
    }
}