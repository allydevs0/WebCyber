using UnityEngine;

namespace Reptile
{
	public class PseudoGraffitiAbility : Ability
	{
		private bool wasUsingEquippedMoveStyle;

		private Side lastSlashSide = Side.NONE;

		private int slashesNeeded;

		private float slashDuration = 0.4f;

		private int slotInGraffitiSpot;

		public GraffitiSpot graffitiSpot;

		private Transform gTf;

		private float timer;

		private int slashCount;

		internal float cooldown;

		private int[] grafSlashHashes;

		private int grafSlashFinisherHash = Animator.StringToHash("grafSlashFinisher");

		private int grafSlideRHash = Animator.StringToHash("grafSlideR");

		private int grafSlideLHash = Animator.StringToHash("grafSlideL");

		private int grafSlideFHash = Animator.StringToHash("grafSlideF");

		public PseudoGraffitiAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = false;
			normalRotation = false;
			canStartGrind = false;
			canStartWallrun = false;
			grafSlashHashes = new int[8];
			for (int i = 0; i < 8; i++)
			{
				int[] array = grafSlashHashes;
				int num = i;
				Side side = (Side)i;
				array[num] = Animator.StringToHash("grafSlash" + side);
			}
		}

		public override void OnStartAbility()
		{
			timer = 0f;
			p.motor.HaveCollision(have: false);
			p.SetBoostpackAndFrictionEffects(BoostpackEffectMode.GRAFFITI);
			wasUsingEquippedMoveStyle = p.usingEquippedMovestyle;
			p.SwitchToEquippedMovestyle(set: false);
			p.inGraffitiGame = true;
			gTf = graffitiSpot.transform;
			slotInGraffitiSpot = graffitiSpot.StartGraffitiMode(p);
			slashCount = 0;
			slashesNeeded = 1;
			if (graffitiSpot.size == GraffitiSize.M)
			{
				slashesNeeded = 4;
			}
			else if (graffitiSpot.size == GraffitiSize.L)
			{
				slashesNeeded = 5;
			}
			else if (graffitiSpot.size == GraffitiSize.XL)
			{
				slashesNeeded = 6;
			}
			float magnitude = p.GetPracticalVelocity().magnitude;
			if (graffitiSpot.size == GraffitiSize.S)
			{
				p.PlayAnim(grafSlashFinisherHash);
			}
			else if (magnitude <= p.minMoveSpeed)
			{
				p.PlayAnim(grafSlideFHash);
			}
			else
			{
				float num = Vector3.Dot(p.dir, graffitiSpot.transform.right);
				if (num > 0.5f)
				{
					p.PlayAnim(grafSlideRHash);
				}
				else if (num < -0.5f)
				{
					p.PlayAnim(grafSlideLHash);
				}
				else
				{
					p.PlayAnim(grafSlideFHash);
				}
			}
			p.SetVelocity(Vector3.zero);
			SetGraffitiPos();
			p.SetVisualRot(Quaternion.LookRotation(-graffitiSpot.transform.forward));
		}

		public void PassiveUpdate()
		{
			if (cooldown > 0f)
			{
				cooldown -= Core.dt;
			}
		}

		public override void UpdateAbility()
		{
			if (slashCount >= slashesNeeded)
			{
				timer += Core.dt;
				if (timer > 1f)
				{
					Finish();
				}
			}
			else
			{
				Animate();
			}
			SetGraffitiPos();
		}

		public void CompletePainting()
		{
			p.PlayAnim(grafSlashFinisherHash);
			p.PlayVoice(AudioClipID.VoiceBoostTrick, VoicePriority.BOOST_TRICK);
			p.AudioManager.PlaySfxGameplay(SfxCollectionID.GraffitiSfx, AudioClipID.graffitiComplete, p.playerOneShotAudioSource);
			p.RemoveGraffitiSlash();
			p.CreateGraffitiFinishEffect(gTf, graffitiSpot.size);
			_ = p.crew;
			GraffitiArt graffitiArt = graffitiSpot.GraffitiArtInfo.FindCrewArt(p.crew, graffitiSpot.size);
			if ((p.crew == Crew.PLAYERS || p.crew == Crew.ROGUE) && graffitiSpot.size == GraffitiSize.S)
			{
				graffitiArt = graffitiSpot.GraffitiArtInfo.FindByCharacter(p.character);
			}
			bool oldBottomWasClaimedByPlayableCrew = graffitiSpot.bottomCrew == Crew.PLAYERS || graffitiSpot.bottomCrew == Crew.ROGUE;
			if (graffitiArt != null)
			{
				graffitiSpot.Paint(p.crew, graffitiArt, p);
			}
			slashCount = slashesNeeded;
			timer = 0f;
			if (p.crew == Crew.ROGUE || p.crew == Crew.PLAYERS)
			{
				graffitiSpot.GiveRep(noRepPickup: true, oldBottomWasClaimedByPlayableCrew);
			}
		}

		public void Finish()
		{
			graffitiSpot.SetLastPlayer(p);
			graffitiSpot.SetState(GraffitiState.FINISHED);
			p.StopCurrentAbility();
		}

		public void Animate(bool justKeepGoing = false)
		{
			timer += Core.dt;
			if (!(timer > slashDuration))
			{
				return;
			}
			p.anim.speed = 1f;
			timer = 0f;
			slashCount++;
			Random.Range(1, 4);
			if (slashCount == slashesNeeded && !justKeepGoing)
			{
				CompletePainting();
				return;
			}
			p.RemoveGraffitiSlash();
			Vector3 sCardinal = Vector3.zero;
			Side side = GraffitiGame.GetClosestCardinalDirection(Random.rotation * Vector3.forward, graffitiSpot, ref sCardinal);
			if (side == lastSlashSide)
			{
				side++;
				if (side == Side.MAX)
				{
					side = Side.UP;
				}
			}
			p.PlayAnim(grafSlashHashes[(int)side], forceOverwrite: true);
			p.CreateGraffitiSlashEffect(p.visualTf, sCardinal);
			p.AudioManager.PlaySfxGameplay(SfxCollectionID.GraffitiSfx, AudioClipID.graffitiSlash, p.playerOneShotAudioSource);
			lastSlashSide = side;
		}

		private void SetGraffitiPos()
		{
			Vector3 b = gTf.position + gTf.forward + gTf.up * -1f;
			if (slotInGraffitiSpot != -1)
			{
				b = graffitiSpot.extraSlotsContainer.GetChild(slotInGraffitiSpot).position;
			}
			Vector3 positionTeleport = Vector3.Lerp(p.tf.position, b, 0.1f);
			p.motor.SetPositionTeleport(positionTeleport);
		}

		public override void OnStopAbility()
		{
			cooldown = 1f;
			p.motor.HaveCollision(have: true);
			p.ResetVisualRot();
			p.inGraffitiGame = false;
			graffitiSpot.FreeExtraSlot(slotInGraffitiSpot);
			p.SwitchToEquippedMovestyle(wasUsingEquippedMoveStyle);
			p.RemoveGraffitiSlash();
			p.characterVisual.SetSpraycan(set: false, p.character);
		}
	}
}
