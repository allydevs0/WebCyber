using UnityEngine;

namespace Reptile
{
	public class GraffitiEffect : MonoBehaviour
	{
		private float speed;

		public ParticleSystem splashes;

		public ParticleSystem splat;

		public ParticleSystem strokes;

		public ParticleSystem shockwave;

		public ParticleSystem ball;

		public bool scaleUp;

		public Animation animSlash;

		private void Start()
		{
			if ((bool)shockwave)
			{
				shockwave.Emit(1);
				ParticleSystem.EmissionModule emission = shockwave.emission;
				emission.rateOverTime = 0f;
			}
			if ((bool)ball)
			{
				ball.Emit(1);
				ParticleSystem.EmissionModule emission2 = ball.emission;
				emission2.rateOverTime = 0f;
			}
		}

		public void SetSpeed(float setSpeed)
		{
			speed = setSpeed;
			if ((bool)splashes)
			{
				ParticleSystem.MainModule main = splashes.main;
				main.simulationSpeed = speed;
			}
			if ((bool)splat)
			{
				ParticleSystem.MainModule main = splat.main;
				main.simulationSpeed = speed;
			}
			if ((bool)strokes)
			{
				ParticleSystem.MainModule main = strokes.main;
				main.simulationSpeed = speed;
			}
			if (scaleUp)
			{
				base.gameObject.transform.localScale = new Vector3(base.gameObject.transform.localScale.x + 0.01f, base.gameObject.transform.localScale.y + 0.01f, base.gameObject.transform.localScale.z + 0.01f);
			}
		}

		public float GetSpeed()
		{
			return speed;
		}

		public void SetStun(float stun)
		{
			if ((bool)animSlash)
			{
				animSlash["grafSlashEffect"].speed = stun;
			}
		}
	}
}
