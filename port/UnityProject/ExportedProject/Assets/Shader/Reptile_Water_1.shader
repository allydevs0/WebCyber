Shader "Reptile/Water" {
	Properties {
		_DepthGradientShallow ("Depth Gradient Shallow", Vector) = (0.325,0.807,0.971,0.725)
		_DepthGradientDeep ("Depth Gradient Deep", Vector) = (0.086,0.407,1,0.749)
		_DepthMaxDistance ("Depth Maximum Distance", Float) = 1
		_FoamColor ("Foam Color", Vector) = (1,1,1,1)
		_SurfaceNoise ("Surface Noise", 2D) = "white" {}
		_SurfaceNoiseScroll ("Surface Noise Scroll Amount", Vector) = (0.03,0.03,0,0)
		_SurfaceNoiseCutoff ("Surface Noise Cutoff", Range(0, 1)) = 0.777
		_SurfaceDistortion ("Surface Distortion", 2D) = "white" {}
		_SurfaceDistortionAmount ("Surface Distortion Amount", Range(0, 1)) = 0.27
		_FoamMaxDistance ("Foam Maximum Distance", Float) = 0.4
		_FoamMinDistance ("Foam Minimum Distance", Float) = 0.04
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
}