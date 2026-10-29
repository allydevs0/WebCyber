using UnityEngine;

namespace Reptile
{
	public class MoveObject : MonoBehaviour
	{
		public float maxUpdateDistanceSqrd = 2500f;

		public Vector3 moveVelocity;

		public Vector3 rotateVelocity;

		public Transform lookAt;

		public bool LocalRotation;

		private Rigidbody body;

		internal Transform tf;

		public void Init()
		{
			tf = base.transform;
			body = GetComponent<Rigidbody>();
			if (body == null)
			{
				body = GetComponentInChildren<Rigidbody>();
			}
		}

		public void UpdateMoveObject(Vector3 currentCameraPosition)
		{
			if (base.gameObject.activeSelf && (currentCameraPosition - tf.position).sqrMagnitude < maxUpdateDistanceSqrd)
			{
				if (body == null)
				{
					UpdateMoveTransform();
				}
				else
				{
					UpdateMoveBody();
				}
			}
		}

		private void UpdateMoveTransform()
		{
			float dt = Core.dt;
			if (lookAt != null)
			{
				tf.LookAt(lookAt);
			}
			else if (LocalRotation)
			{
				if (rotateVelocity.sqrMagnitude > 0f)
				{
					tf.localEulerAngles += rotateVelocity * dt;
				}
			}
			else if (rotateVelocity.sqrMagnitude > 0f)
			{
				tf.eulerAngles += rotateVelocity * dt;
			}
			if (moveVelocity.sqrMagnitude > 0f)
			{
				tf.position += moveVelocity * dt;
			}
		}

		private void UpdateMoveBody()
		{
			float dt = Core.dt;
			if (moveVelocity.sqrMagnitude > 0f)
			{
				body.MovePosition(tf.position + moveVelocity * dt);
			}
			if (rotateVelocity.sqrMagnitude > 0f)
			{
				body.MoveRotation(Quaternion.Euler(tf.eulerAngles + rotateVelocity * dt));
			}
		}
	}
}
