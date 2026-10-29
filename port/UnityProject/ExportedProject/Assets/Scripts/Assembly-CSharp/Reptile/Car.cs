using UnityEngine;

namespace Reptile
{
	public class Car : MonoBehaviour
	{
		public float chassisRoll = 1f;

		public Transform[] wheels;

		public Transform chassis;

		public Collider[] colliders;

		[SerializeField]
		private float carHornCooldownDuration = 3f;

		private CarsMoveHandler handler;

		public CarVisualAudio InitHotData(CarsMoveHandler moveHandler, CarVisualAudio carHot)
		{
			handler = moveHandler;
			carHot.chassisAnimTimer = 1f;
			carHot.chassisAnimTimer2 = 0f;
			carHot.chassisPreviousSpeed = 1f;
			carHot.chassisAngle = 0f;
			carHot.chassisPos = 0f;
			carHot.doBounce = false;
			carHot.doBounceTimer = 1f;
			carHot.carHornCooldown = 0f;
			carHot.stoppedAlready = false;
			carHot.playingEngineSoundAlready = false;
			carHot.chassisDefaultRotation = chassis.localRotation;
			carHot.chassisDefaultPosition = chassis.localPosition;
			carHot.audioSource = base.gameObject.GetComponent<AudioSource>();
			carHot.carTransform = base.transform;
			carHot.chassisRoll = chassisRoll;
			carHot.wheels = wheels;
			carHot.chassis = chassis;
			carHot.colliders = colliders;
			carHot.carHornCooldownDuration = carHornCooldownDuration;
			return carHot;
		}

		public void PlayerLandsOn()
		{
			if (handler != null)
			{
				handler.PlayerLandsOn(this);
			}
		}
	}
}
