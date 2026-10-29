using UnityEngine;

namespace Reptile
{
	public class OutfitSwappableCharacter : MonoBehaviour
	{
		[SerializeField]
		private Characters character;

		[SerializeField]
		private SkinnedMeshRenderer mainRenderer;

		private int outfit;

		public Characters Character => character;

		public int Outfit => outfit;

		public void SetMaterialAndOutfit(Material mat, int outfitIndex)
		{
			mainRenderer.sharedMaterial = mat;
			outfit = outfitIndex;
		}

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}
	}
}
