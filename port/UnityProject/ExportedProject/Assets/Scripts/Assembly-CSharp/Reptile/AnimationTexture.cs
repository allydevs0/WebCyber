using UnityEngine;

namespace Reptile
{
	public class AnimationTexture
	{
		public AnimationInstanceType type { get; set; }

		public Texture2D[] boneTexture { get; set; }

		public int blockWidth { get; set; }

		public int blockHeight { get; set; }
	}
}
