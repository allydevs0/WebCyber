using System;
using UnityEngine;

namespace Reptile
{
	public class Ability
	{
		public Player p;

		public bool normalMovement = true;

		public bool normalRotation = true;

		public bool normalAnimation;

		public bool allowNormalJump;

		public bool keepJumping;

		public bool canStartGrind = true;

		public bool canStartWallrun = true;

		public float targetSpeed = -1f;

		public float acc = -1f;

		public float decc = -1f;

		public float rotateSpeed = -1f;

		public bool treatPlayerAsSortaGrounded;

		public bool locked;

		public bool autoAirTrickFromLauncher = true;

		public float setSpeedOnHittingBreakable;

		public bool rotateWithPlatform = true;

		public bool canUseSpraycan = true;

		public Vector3 customVelocity;

		public float customGravity = -1f;

		public Vector3 customTargetRot;

		[NonSerialized]
		public CameraMode customCameraMode;

		public Ability(Player player)
		{
			p = player;
			p.abilities.Add(this);
			Init();
		}

		public virtual void Init()
		{
		}

		public virtual void OnStartAbility()
		{
		}

		public virtual void FixedUpdateAbility()
		{
		}

		public virtual void UpdateAbility()
		{
		}

		public virtual void UpdateAbilityJumpStatus()
		{
		}

		public virtual void AfterTransformUpdate()
		{
		}

		public virtual bool CheckActivation()
		{
			return false;
		}

		public virtual void OnStopAbility()
		{
		}

		public virtual void OnJump()
		{
		}

		public virtual void GiveBackVelocity()
		{
		}

		public virtual void RecheckLoopingSoundInAbilityUse()
		{
		}
	}
}
