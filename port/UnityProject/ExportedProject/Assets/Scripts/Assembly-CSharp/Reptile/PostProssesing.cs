using UnityEngine;

namespace Reptile
{
	[ExecuteInEditMode]
	public class PostProssesing : MonoBehaviour
	{
		public Material PostProsessingMaterial;

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, PostProsessingMaterial);
		}
	}
}
