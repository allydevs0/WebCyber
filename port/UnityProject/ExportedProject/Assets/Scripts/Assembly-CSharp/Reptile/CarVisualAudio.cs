using UnityEngine;

namespace Reptile
{
	public struct CarVisualAudio
	{
		public float chassisRoll;

		public Transform[] wheels;

		public Transform chassis;

		public Collider[] colliders;

		internal float carHornCooldownDuration;

		internal Quaternion chassisDefaultRotation;

		internal Vector3 chassisDefaultPosition;

		internal float chassisAnimTimer;

		internal float chassisAnimTimer2;

		internal float chassisPreviousSpeed;

		internal float chassisAngle;

		internal float chassisPos;

		internal AudioSource audioSource;

		internal bool doBounce;

		internal float doBounceTimer;

		internal Transform carTransform;

		internal float carHornCooldown;

		internal bool stoppedAlready;

		internal bool playingEngineSoundAlready;
	}
}
