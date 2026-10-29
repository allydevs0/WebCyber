Shader "Reptile/Skybox/Panoramic" {
	Properties {
		_Tint ("Tint Color", Vector) = (0.5,0.5,0.5,0.5)
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1
		_Rotation ("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _MainTex ("Spherical  (HDR)", 2D) = "grey" {}
		[KeywordEnum(6 Frames Layout, Latitude Longitude Layout)] _Mapping ("Mapping", Float) = 1
		[Enum(360 Degrees, 0, 180 Degrees, 1)] _ImageType ("Image Type", Float) = 0
		[Toggle] _MirrorOnBack ("Mirror on Back", Float) = 0
		[Enum(None, 0, Side by Side, 1, Over Under, 2)] _Layout ("3D Layout", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	Fallback "Reptile/Skybox/XboxSeriesPanoramic"
	//CustomEditor "SkyboxPanoramicShaderGUI"
}