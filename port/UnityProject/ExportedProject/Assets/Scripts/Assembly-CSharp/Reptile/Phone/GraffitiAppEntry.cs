using UnityEngine;

namespace Reptile.Phone
{
	[CreateAssetMenu(menuName = "Reptile/Phone/Graffiti/New Entry")]
	public class GraffitiAppEntry : AUnlockable
	{
		public string Title;

		public string Artist;

		public GraffitiSize Size;

		public Texture GraffitiTexture;

		public GraffitiArt.Combos Combos;
	}
}
