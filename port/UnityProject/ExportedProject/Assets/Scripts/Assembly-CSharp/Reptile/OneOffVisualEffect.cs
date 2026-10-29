using UnityEngine;

namespace Reptile
{
	public class OneOffVisualEffect : MonoBehaviour
	{
		private Animation anim;

		private float timer;

		public float duration = -1f;

		private void Awake()
		{
			anim = GetComponent<Animation>();
			if (anim == null)
			{
				anim = base.gameObject.GetComponentInChildren<Animation>();
			}
		}

		private void Update()
		{
			if (duration > 0f)
			{
				timer += Core.dt;
				if (timer > duration)
				{
					Object.Destroy(base.gameObject);
				}
			}
			else if (!anim.isPlaying)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
