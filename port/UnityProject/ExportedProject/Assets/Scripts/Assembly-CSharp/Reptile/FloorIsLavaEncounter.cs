using UnityEngine;

namespace Reptile
{
	public class FloorIsLavaEncounter : Encounter
	{
		public GameObject[] trackSetups;

		private GraffitiSpot[] graffitiSpots;

		private Transform[] spawners;

		private int atTrack;

		public override void InitSceneObject()
		{
			graffitiSpots = new GraffitiSpot[trackSetups.Length];
			spawners = new Transform[trackSetups.Length];
			for (int i = 0; i < trackSetups.Length; i++)
			{
				graffitiSpots[i] = trackSetups[i].GetComponentInChildren<GraffitiSpot>();
				spawners[i] = trackSetups[i].transform.FindRecursive("spawner");
				spawners[i].gameObject.SetActive(value: false);
				trackSetups[i].SetActive(value: false);
			}
			base.InitSceneObject();
		}

		public override void StartMainEvent()
		{
			StartTrack(0);
			base.StartMainEvent();
		}

		public override void UpdateMainEvent()
		{
			if (player.IsGrounded() && player.tf.position.y <= 0.25f)
			{
				ReadyPlayer();
			}
			if (graffitiSpots[atTrack].ClaimedByPlayableCrew())
			{
				if (atTrack >= trackSetups.Length - 1)
				{
					SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
				}
				else
				{
					StartTrack(atTrack + 1);
				}
			}
			base.UpdateMainEvent();
		}

		public void StartTrack(int num)
		{
			SetTrackSetup(num);
			ReadyPlayer();
		}

		public void SetTrackSetup(int num)
		{
			for (int i = 0; i < trackSetups.Length; i++)
			{
				trackSetups[i].SetActive(i == num);
			}
			atTrack = num;
		}

		public void ReadyPlayer()
		{
			WorldHandler.instance.PlaceCurrentPlayerAt(spawners[atTrack]);
		}
	}
}
