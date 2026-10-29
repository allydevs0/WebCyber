using UnityEngine;

namespace Reptile
{
	public class MovingPlatform : MonoBehaviour
	{
		private Rigidbody rigidBody;

		private Transform tf;

		public Vector3 direction;

		public float speed;

		private void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			tf = base.transform;
		}

		private void FixedUpdate()
		{
			if (tf.position.z > 130f)
			{
				tf.position = new Vector3(tf.position.x, tf.position.y, -130f);
			}
			if (tf.position.z < -130f)
			{
				tf.position = new Vector3(tf.position.x, tf.position.y, 130f);
			}
			rigidBody.MovePosition(tf.position + direction * speed * Time.deltaTime);
		}
	}
}
