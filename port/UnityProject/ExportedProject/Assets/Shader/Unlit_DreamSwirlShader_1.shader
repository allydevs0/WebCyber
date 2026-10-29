Shader "Unlit/DreamSwirlShader" {
	Properties {
		[Header(Colors)] _MainTex ("Color Ramp", 2D) = "white" {}
		_PaletteIndex ("Palette Offset", Range(0, 1)) = 0
		_ColorOffset ("Color Offset", Float) = 0
		[Header(UV)] [Toggle] _Rings ("Rings", Float) = 0
		[IntRange] _LineMultiplier ("Line multiplier", Range(1, 10)) = 3
		_Twist ("Twist", Float) = 0
		_Scalar5 ("Stretch Along Distortion", Range(-5, 5)) = 2
		_Scalar6 ("Distortion Scale", Range(0, 5)) = 1
		[Header(Animation)] _TimeScalar ("Speed", Range(0, 1)) = 0.5
		_DistortionSpeed ("Distortion Component", Float) = 0.844
		_DistortionFlow ("Distortion Flow", Float) = 1
		_RotationSpeed ("Rotation Component", Float) = 2
		_ColorRotation ("Color Component", Float) = 1
		[Header(Fade)] _BlackGradientApply ("BlackGradientAtBottom", Float) = 0
		_MovementAlpha ("MovementAlpha", Float) = 0
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
}