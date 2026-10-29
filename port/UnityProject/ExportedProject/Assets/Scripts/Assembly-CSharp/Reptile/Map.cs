using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(menuName = "Reptile/Phone/Minimap/New Map")]
	public class Map : ScriptableObject
	{
		[SerializeField]
		private GameObject m_MapObject;

		[SerializeField]
		private Material mapMaterial;

		[SerializeField]
		private float m_ScaleFactor = 1f;

		[Tooltip("Offset is without scale factored in")]
		[SerializeField]
		private Vector3 m_PositionOffset = Vector3.zero;

		[SerializeField]
		private Vector3 m_EulerOffset = Vector3.zero;

		public GameObject MapObject => m_MapObject;

		public float ScaleFactor => m_ScaleFactor;

		public Vector3 PositionOffset => m_PositionOffset;

		public Vector3 EulerOffset => m_EulerOffset;

		public Material MapMaterial => mapMaterial;
	}
}
