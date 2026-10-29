using UnityEngine;

namespace Reptile
{
	public class Extrusion
	{
		public enum ExtrudeState
		{
			IN = 0,
			OUT = 1
		}

		private float timer;

		private float speed;

		public float outDuration;

		private Transform tf;

		private GameObject hurtbox;

		private Vector3 reducedPos;

		private Vector3 extendedPos;

		public GraffitiSpotFinisher gSpot;

		public ExtrudeState state = ExtrudeState.OUT;

		public ExtrudeState targetState;

		public Extrusion(Transform setTf, Vector3 setExtendedPos, Vector3 setReducedPos, float setSpeed, GameObject setHurtbox = null, float setOutDuration = -1f)
		{
			tf = setTf;
			reducedPos = setReducedPos;
			extendedPos = setExtendedPos;
			gSpot = tf.GetComponentInChildren<GraffitiSpotFinisher>();
			speed = setSpeed;
			hurtbox = setHurtbox;
			outDuration = setOutDuration;
			SetFullReduced();
		}

		public void Extend()
		{
			timer = 0f;
			targetState = ExtrudeState.OUT;
			if ((bool)hurtbox)
			{
				hurtbox.SetActive(value: false);
			}
		}

		public void Reduce()
		{
			timer = 0f;
			targetState = ExtrudeState.IN;
			if (!gSpot.IsFinished())
			{
				gSpot.gameObject.SetActive(value: false);
			}
		}

		public void SetFullExtended()
		{
			timer = 0f;
			state = (targetState = ExtrudeState.OUT);
			tf.localPosition = extendedPos;
			gSpot.gameObject.SetActive(value: true);
		}

		public void SetFullReduced()
		{
			timer = 0f;
			state = (targetState = ExtrudeState.IN);
			tf.localPosition = reducedPos;
			gSpot.gameObject.SetActive(value: false);
			if ((bool)hurtbox)
			{
				hurtbox.SetActive(value: true);
			}
		}

		public void Update()
		{
			if (targetState == ExtrudeState.OUT && state != ExtrudeState.OUT)
			{
				timer += speed * Core.dt;
				tf.localPosition = Vector3.Lerp(reducedPos, extendedPos, timer);
				if (timer >= 1f)
				{
					SetFullExtended();
				}
			}
			else if (targetState == ExtrudeState.IN && state != ExtrudeState.IN)
			{
				timer += speed * Core.dt;
				tf.localPosition = Vector3.Lerp(extendedPos, reducedPos, timer);
				if (timer >= 1f)
				{
					SetFullReduced();
				}
			}
			if (outDuration > 0f && !gSpot.IsFinished() && state == ExtrudeState.OUT && targetState == ExtrudeState.OUT)
			{
				timer += Core.dt;
				if (timer > outDuration)
				{
					Reduce();
				}
			}
		}
	}
}
