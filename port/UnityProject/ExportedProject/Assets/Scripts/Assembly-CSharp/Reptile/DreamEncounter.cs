using UnityEngine;

namespace Reptile
{
	public class DreamEncounter : Encounter
	{
		public GraffitiSpotFinisher graffitiSpot;

		private float defaultfarClipPlane = 1000f;

		[SerializeField]
		private bool startMusicAfterFirstCheckpoint = true;

		public override void InitSceneObject()
		{
			graffitiSpot.encounter = this;
			graffitiSpot.gameObject.SetActive(value: false);
			outroDelay = 0f;
			base.InitSceneObject();
		}

		public override void StartMainEvent()
		{
			if (currentCheckpoint == -1 && startMusicAfterFirstCheckpoint)
			{
				Core.Instance.AudioManager.StartCustomTrack(Core.Instance.AudioManager.MusicLibraryPlayer.GetMusicTrackByID(MusicTrackID.Silence_Music), ref customMusicTrackInstance);
			}
			else
			{
				Core.Instance.AudioManager.StartCustomTrack(customMusicTrack, ref customMusicTrackInstance);
			}
			base.StartMainEvent();
		}

		public override void UpdateMainEvent()
		{
			base.UpdateMainEvent();
		}

		public override void EnterEncounterState(EncounterState setState)
		{
			if (setState == EncounterState.INTRO)
			{
				graffitiSpot.MakeOpen();
				graffitiSpot.ClearPaint();
				graffitiSpot.SetState(GraffitiState.NONE);
			}
			if (setState == EncounterState.CLOSED)
			{
				WorldHandler.instance.GetCurrentPlayer().cam.cam.clearFlags = CameraClearFlags.Skybox;
				WorldHandler.instance.GetCurrentPlayer().cam.cam.farClipPlane = defaultfarClipPlane;
			}
			base.EnterEncounterState(setState);
		}

		public override void ResetEncounter()
		{
			graffitiSpot.ClearPaint();
			graffitiSpot.SetState(GraffitiState.NONE);
			graffitiSpot.gameObject.SetActive(value: false);
		}

		public override void ActivateEncounter()
		{
			graffitiSpot.gameObject.SetActive(value: true);
			graffitiSpot.ClearPaint();
			WorldHandler.instance.GetCurrentPlayer().cam.cam.clearFlags = CameraClearFlags.Nothing;
			defaultfarClipPlane = WorldHandler.instance.GetCurrentPlayer().cam.cam.farClipPlane;
			WorldHandler.instance.GetCurrentPlayer().cam.cam.farClipPlane = 1500f;
			base.ActivateEncounter();
		}

		public override void TriggerDetectPlayer(Collider trigger, Player player)
		{
			if (state == EncounterState.OPEN)
			{
				ActivateEncounter();
			}
		}

		public override void CheatComplete()
		{
			WorldHandler.instance.PlaceCurrentPlayerAt(graffitiSpot.transform.position + graffitiSpot.transform.forward * 3f, Quaternion.LookRotation(graffitiSpot.transform.forward * -1f));
		}

		public void StartCustomTrack()
		{
			Core.Instance.AudioManager.StartCustomTrack(customMusicTrack, ref customMusicTrackInstance);
		}
	}
}
