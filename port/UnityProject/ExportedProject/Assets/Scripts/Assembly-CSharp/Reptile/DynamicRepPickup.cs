using UnityEngine;

namespace Reptile
{
	public class DynamicRepPickup : DynamicPickup
	{
		private bool off;

		private Vector3 dir;

		private float speed;

		private float targetSpeed;

		private float acc = 5f;

		private Vector3 velocity;

		private Transform tf;

		public new void Awake()
		{
			player = WorldHandler.instance.GetCurrentPlayer();
			tf = base.transform;
			dir = tf.forward * 2f + tf.right * Random.Range(-1f, 1f) + tf.up * Random.Range(-1f, 1f);
			dir.Normalize();
			speed = 6f;
			velocity = dir * speed;
			if (WorldHandler.instance != null && WorldHandler.instance.SceneObjectsRegister != null)
			{
				AddPickupToSceneObjects();
			}
		}

		public override void UpdatePickup()
		{
			if (off)
			{
				if (!player.IsBusyWithSequence())
				{
					pickupObject.SetActive(value: true);
					off = false;
				}
			}
			else if (player.IsBusyWithSequence())
			{
				pickupObject.SetActive(value: false);
				off = true;
				return;
			}
			if (speed < targetSpeed)
			{
				speed = Mathf.Min(speed + acc * Core.dt, targetSpeed);
			}
			else if (speed > targetSpeed)
			{
				speed = Mathf.Max(speed - acc * Core.dt, targetSpeed);
			}
			velocity = dir * speed;
			base.transform.position += velocity * Core.dt;
			base.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
			timer += Core.dt;
			if (timer > 0.6f)
			{
				targetSpeed = Mathf.Max(7f, player.GetForwardSpeed() + 5f);
				Vector3 normalized = (player.capsuleCenter - tf.position).normalized;
				dir = Vector3.Lerp(dir, normalized, 0.1f);
			}
			else if (timer > 30f)
			{
				PickupPickup(player);
			}
		}
	}
}
