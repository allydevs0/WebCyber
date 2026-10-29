using System.Reflection;
using UnityEngine;

namespace Reptile
{
	public static class CameraUtility
	{
		public static void RenderCameraWithShaderWithoutUIEvents(Camera camera, Shader shader, FieldInfo canvasWillRenderFieldInfo)
		{
			object value = canvasWillRenderFieldInfo.GetValue(null);
			canvasWillRenderFieldInfo.SetValue(null, null);
			camera.RenderWithShader(shader, "RenderType");
			canvasWillRenderFieldInfo.SetValue(null, value);
		}
	}
}
