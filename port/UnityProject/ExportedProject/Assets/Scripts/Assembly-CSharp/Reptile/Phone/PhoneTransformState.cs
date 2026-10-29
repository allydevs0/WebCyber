using UnityEngine;

namespace Reptile.Phone
{
	[CreateAssetMenu(menuName = "Reptile/Phone/New Phone transform state")]
	public class PhoneTransformState : ScriptableObject
	{
		[SerializeField]
		private float m_HingeAngle;

		[SerializeField]
		private Vector3 m_PhoneAngle = Vector3.zero;

		[SerializeField]
		private Vector2 m_Position = Vector2.zero;

		[SerializeField]
		private float m_XOvershoot;

		public float HingeAngle => m_HingeAngle;

		public Vector3 PhoneAngle => m_PhoneAngle;

		public Vector3 Position => m_Position;

		public float XOvershoot => m_XOvershoot;
	}
}
