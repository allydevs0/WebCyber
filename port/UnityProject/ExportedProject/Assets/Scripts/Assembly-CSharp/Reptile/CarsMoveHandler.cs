using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class CarsMoveHandler : MoveAlongHandler
	{
		private AudioManager coreAudioManager;

		private List<CarVisualAudio> carsHot;

		private List<CarVisualAudio> carsSparesHot;

		private List<Car> carsCold;

		private List<Car> carsSparesCold;

		public override void Awake()
		{
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			coreAudioManager = null;
			carsHot = null;
			carsCold = null;
			carsSparesHot = null;
			carsSparesCold = null;
			StageManager.OnStageInitialized -= OnStageInitialized;
			Core.OnUpdate -= UpdateCars;
		}

		public override void OnStageInitialized()
		{
			coreAudioManager = Core.Instance.AudioManager;
			Car[] componentsInChildren = GetComponentsInChildren<Car>();
			int num = componentsInChildren.Length;
			carsHot = new List<CarVisualAudio>(10);
			carsCold = new List<Car>(10);
			carsSparesHot = new List<CarVisualAudio>(num);
			carsSparesCold = new List<Car>(num);
			for (int i = 0; i < num; i++)
			{
				Car car = componentsInChildren[i];
				carsSparesHot.Add(car.InitHotData(this, default));
				carsSparesCold.Add(car);
			}
			base.OnStageInitialized();
			Core.OnUpdate += UpdateCars;
		}

		private void UpdateCars()
		{
			int count = carsHot.Count;
			_ = WorldHandler.instance.CurrentCameraPosition;
			for (int i = 0; i < count; i++)
			{
				CarVisualAudio value = carsHot[i];
				MoveAlongPoints_Hot moveAlongPoints_Hot = moversHot[i];
				if (moveAlongPoints_Hot.moverLOD == MoverLOD.DETAILS)
				{
					if (!moveAlongPoints_Hot.activelyMoving)
					{
						value.chassisAnimTimer2 = 1f;
						value.chassisAnimTimer -= Core.dt;
						if (value.chassisAnimTimer > 1f - value.chassisPreviousSpeed / 2f)
						{
							value.chassisAngle = Mathf.Lerp(value.chassisAngle, 2f * value.chassisRoll * (value.chassisPreviousSpeed * value.chassisPreviousSpeed), Core.dt * (8f * value.chassisPreviousSpeed));
						}
						else if (value.chassisAnimTimer > 0.7f - value.chassisPreviousSpeed / 2f)
						{
							value.chassisAngle = Mathf.Lerp(value.chassisAngle, -1f * value.chassisRoll * value.chassisPreviousSpeed, Core.dt * 8f);
						}
						else if (value.chassisAnimTimer < 0.7f - value.chassisPreviousSpeed / 2f)
						{
							value.chassisAngle = Mathf.Lerp(value.chassisAngle, 0f, Core.dt * 5f);
						}
						if (value.chassisAnimTimer < 0f)
						{
							value.chassisAnimTimer = 0f;
						}
						if (moveAlongPoints_Hot.stoppedForPlayer && value.carHornCooldown <= 0f && value.audioSource != null)
						{
							if (!value.stoppedAlready && moveAlongPoints_Hot.timeSpeed > 0.9f)
							{
								coreAudioManager.StopLoopingSfx(value.audioSource);
								value.playingEngineSoundAlready = false;
								value.audioSource.dopplerLevel = 0f;
								coreAudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.CarBrake, value.audioSource);
								value.stoppedAlready = true;
							}
							value.audioSource.dopplerLevel = 0f;
							Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.CarHorn, value.audioSource);
							value.carHornCooldown = value.carHornCooldownDuration;
						}
						if (value.carHornCooldown > 0f)
						{
							value.carHornCooldown -= Core.dt;
						}
					}
					else
					{
						value.carHornCooldown = 0f;
						value.chassisAnimTimer = 1f;
						value.chassisAnimTimer2 -= Core.dt;
						value.chassisPreviousSpeed = moveAlongPoints_Hot.timeSpeed;
						value.stoppedAlready = true;
						if (value.chassisAnimTimer2 > 0.5f)
						{
							value.chassisAngle = Mathf.Lerp(value.chassisAngle, -2f * value.chassisRoll, Core.dt * 5f);
						}
						else
						{
							value.chassisAngle = Mathf.Lerp(value.chassisAngle, 0f, Core.dt * 2f);
						}
						if (value.chassisAnimTimer2 < 0f)
						{
							value.chassisAnimTimer2 = 0f;
						}
						value.stoppedAlready = false;
						if (!value.playingEngineSoundAlready)
						{
							value.audioSource.dopplerLevel = 1f;
							coreAudioManager.PlaySfxGameplayLooping(SfxCollectionID.EnvironmentSfx, AudioClipID.car_engine, value.audioSource);
							value.playingEngineSoundAlready = true;
						}
					}
					value.chassis.localRotation = value.chassisDefaultRotation * Quaternion.Euler(value.chassisAngle, 0f, 0f);
					RotateWheels(value.wheels, moveAlongPoints_Hot);
					if (!carsHot[i].colliders[0].enabled)
					{
						for (int j = 0; j < carsHot[i].colliders.Length; j++)
						{
							carsHot[i].colliders[j].enabled = true;
						}
					}
				}
				else if (carsHot[i].colliders[0].enabled)
				{
					for (int k = 0; k < carsHot[i].colliders.Length; k++)
					{
						carsHot[i].colliders[k].enabled = false;
					}
				}
				if (value.doBounce)
				{
					value.doBounceTimer -= Core.dt;
					if (value.doBounceTimer > 0.85f)
					{
						value.chassisPos = Mathf.Lerp(value.chassisPos, -0.06f, Core.dt * 6f);
					}
					else if (value.doBounceTimer > 0.7f)
					{
						value.chassisPos = Mathf.Lerp(value.chassisPos, 0.035f, Core.dt * 8f);
					}
					else if (value.doBounceTimer < 0.7f)
					{
						value.chassisPos = Mathf.Lerp(value.chassisPos, 0f, Core.dt * 8f);
					}
					value.chassis.localPosition = new Vector3(value.chassisDefaultPosition.x, value.chassisDefaultPosition.y + value.chassisPos, value.chassisDefaultPosition.z);
					if (value.doBounceTimer <= 0f)
					{
						value.doBounce = false;
						value.doBounceTimer = 1f;
						value.chassis.localPosition = value.chassisDefaultPosition;
						value.chassisPos = 0f;
					}
				}
				moversHot[i] = moveAlongPoints_Hot;
				carsHot[i] = value;
			}
		}

		protected override void SetMoverToMove(MoveAlongPoints_Hot moverSpareHot, int indexOfData, int fromPoint)
		{
			base.SetMoverToMove(moverSpareHot, indexOfData, fromPoint);
			CarVisualAudio item = carsSparesHot[indexOfData];
			Car item2 = carsSparesCold[indexOfData];
			carsSparesHot.RemoveAt(indexOfData);
			carsSparesCold.RemoveAt(indexOfData);
			carsHot.Add(item);
			carsCold.Add(item2);
		}

		protected override void SetMoverToSpare(MoveAlongPoints_Hot moverHot, int indexOfData)
		{
			base.SetMoverToSpare(moverHot, indexOfData);
			CarVisualAudio item = carsHot[indexOfData];
			Car item2 = carsCold[indexOfData];
			carsHot.RemoveAt(indexOfData);
			carsCold.RemoveAt(indexOfData);
			carsSparesHot.Add(item);
			carsSparesCold.Add(item2);
		}

		public void PlayerLandsOn(Car car)
		{
			int index = carsCold.IndexOf(car);
			CarVisualAudio value = carsHot[index];
			if ((bool)value.audioSource)
			{
				value.audioSource.dopplerLevel = 0f;
				coreAudioManager.StopLoopingSfx(value.audioSource);
				coreAudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.car_hood_impact, value.audioSource);
			}
			value.doBounce = true;
			carsHot[index] = value;
		}

		private void RotateWheels(Transform[] wheels, MoveAlongPoints_Hot moverHot)
		{
			int num = wheels.Length;
			Quaternion quaternion = Quaternion.Euler(moverHot.timeSpeed * 22f, 0f, 0f);
			for (int i = 0; i < num; i++)
			{
				wheels[i].localRotation = wheels[i].localRotation * quaternion;
			}
		}
	}
}
