using UnityEngine;

namespace Reptile
{
	public class BannerSpin : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody rigidBody;

		private void Start()
		{
			rigidBody.centerOfMass = Vector3.zero;
			rigidBody.inertiaTensorRotation = new Quaternion(1f, 0f, 1f, 0f);
			rigidBody.constraints = RigidbodyConstraints.FreezeAll;
			rigidBody.constraints &= (RigidbodyConstraints)(-33);
			rigidBody.useGravity = false;
			rigidBody.isKinematic = false;
			rigidBody.angularDrag = 0.7f;
		}
	}
}
