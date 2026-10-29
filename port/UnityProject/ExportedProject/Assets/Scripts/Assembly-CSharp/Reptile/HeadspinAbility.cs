using UnityEngine;

namespace Reptile
{
	public class HeadspinAbility : Ability
	{
		public enum State
		{
			SPIN = 0,
			SPIN_END = 1
		}

		public int rotatecount;

		public float MinRotSpd;

		private Vector2 inputdirect;

		private float holdTimer;

		private bool spinLeft;

		public State state;

		private int headSpinLStopHash = Animator.StringToHash("headSpinLStop");

		private int headSpinRStopHash = Animator.StringToHash("headSpinRStop");

		private int headSpinLStartHash = Animator.StringToHash("headSpinLStart");

		private int headSpinRStartHash = Animator.StringToHash("headSpinRStart");

		public HeadspinAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = true;
			normalRotation = false;
			canStartGrind = false;
			canStartWallrun = false;
			decc = p.stats.groundDecc * 2f;
			targetSpeed = 0f;
		}

		public override void OnStartAbility()
		{
			spinLeft = p.rotLean < 0f;
			SetState(State.SPIN);
		}

		public void SetState(State setState)
		{
			state = setState;
			switch (state)
			{
			case State.SPIN:
				holdTimer = 0f;
				if (spinLeft)
				{
					p.PlayAnim(headSpinLStartHash);
				}
				else
				{
					p.PlayAnim(headSpinRStartHash);
				}
				break;
			case State.SPIN_END:
				if (spinLeft)
				{
					p.PlayAnim(headSpinLStopHash);
				}
				else
				{
					p.PlayAnim(headSpinRStopHash);
				}
				break;
			}
		}

		private void UpdateSpin()
		{
			float f = p.moveInputPlain.x - p.preMoveInputPlain.x;
			float f2 = p.moveInputPlain.y - p.preMoveInputPlain.y;
			if (Mathf.Abs(f) < MinRotSpd && Mathf.Abs(f2) < MinRotSpd)
			{
				rotatecount = 0;
				inputdirect = p.moveInputPlain;
			}
			if (Mathf.Abs(f) >= MinRotSpd || Mathf.Abs(f2) >= MinRotSpd)
			{
				inputdirect = p.moveInputPlain;
				if (p.moveInputPlain.x == inputdirect.x && p.moveInputPlain.y == inputdirect.y)
				{
					rotatecount++;
				}
				_ = rotatecount;
				_ = 2;
			}
		}

		public override void FixedUpdateAbility()
		{
			switch (state)
			{
			case State.SPIN:
				if (p.abilityTimer > 1.5f)
				{
					Vector3 vector = Vector3.Cross(Vector3.up, new Vector3(p.preMoveInputPlain.x, 0f, p.preMoveInputPlain.y).normalized);
					float num = Mathf.Clamp(Vector2.Dot(new Vector3(p.moveInputPlain.x, 0f, p.moveInputPlain.y).normalized, vector) * p.rotLeanFactor, -1f, 1f);
					if ((spinLeft && num > -0.2f) || (!spinLeft && num < 0.2f))
					{
						holdTimer += Core.dt;
					}
					else
					{
						holdTimer = 0f;
					}
				}
				if (holdTimer > 0.35f || p.jumpButtonNew)
				{
					SetState(State.SPIN_END);
				}
				break;
			case State.SPIN_END:
				if (p.curAnim != headSpinLStopHash && p.curAnim != headSpinRStopHash)
				{
					p.StopCurrentAbility();
				}
				break;
			}
		}

		public override void OnStopAbility()
		{
			base.OnStopAbility();
		}

		public override bool CheckActivation()
		{
			return false;
		}
	}
}
