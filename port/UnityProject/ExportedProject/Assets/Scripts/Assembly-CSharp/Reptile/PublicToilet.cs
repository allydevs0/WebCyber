using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class PublicToilet : MonoBehaviour, IInitializableSceneObject
	{
		[SerializeField]
		private PlayableDirector sequence;

		[SerializeField]
		private Transform exit;

		[SerializeField]
		private Animator doorAnimator;

		[SerializeField]
		private GameObject greenLight;

		[SerializeField]
		private GameObject redLight;

		private bool doorOpen = true;

		public bool CanSwap => doorOpen;

		public static event Action<bool> UpdateToilets;

		public void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterSceneObject(this);
		}

		public void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		private void OnDestroy()
		{
			UpdateToilets -= CheckToiletUsed;
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
		}

		public void InitSceneObject()
		{
			OpenDoor(playSound: false);
			UpdateToilets += CheckToiletUsed;
		}

		public void DoSequence()
		{
			SequenceHandler.instance.StartEnteringSequence(sequence, null, setHidePlayer: true, setInterruptPlayer: true, instantly: true, setPausePlayer: true, setAllowPhoneOnAfterSequence: true, null, skippable: false);
			UpdateToilets(obj: true);
			doorAnimator.Play("PublicToiletDoorClose");
			Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.ToiletDoorClose);
			doorOpen = false;
		}

		public void StartMenuVisuals()
		{
			doorAnimator.gameObject.SetActive(value: false);
		}

		public void EndMenuVisuals()
		{
			doorAnimator.gameObject.SetActive(value: true);
			CloseDoor(playSound: false);
			Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.ToiletDoorOpen);
		}

		private void CloseDoor(bool playSound)
		{
			if (playSound)
			{
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.ToiletDoorClose);
			}
			doorAnimator.Play("PublicToiletDoorClose");
			greenLight.SetActive(value: false);
			redLight.SetActive(value: true);
			doorOpen = false;
		}

		private void OpenDoor(bool playSound)
		{
			if (playSound)
			{
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.ToiletDoorOpen);
			}
			greenLight.SetActive(value: true);
			redLight.SetActive(value: false);
			doorAnimator.Play("PublicToiletDoorOpen");
			doorOpen = true;
		}

		public static void CloseAllToilets()
		{
			if (UpdateToilets != null)
			{
				UpdateToilets(obj: false);
			}
		}

		public static void OpenAllToilets()
		{
			if (UpdateToilets != null)
			{
				UpdateToilets(obj: true);
			}
		}

		private void CheckToiletUsed(bool open)
		{
			if (open)
			{
				OpenDoor(playSound: false);
			}
			else
			{
				CloseDoor(playSound: false);
			}
		}
	}
}
