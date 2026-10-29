using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(menuName = "Reptile/VFX/New MoveStyleSkin")]
	public class MoveStyleSkin : AUnlockable
	{
		public MoveStyle moveStyle;

		public string Title = string.Empty;

		internal int skinIndex;

		public Texture skinTexture;
	}
}
