using System.Collections;

namespace Reptile
{
	public class GraffitiSpotFinisher : GraffitiSpot
	{
		internal EnemyGraffitiSpotDamager enemyDamager;

		internal EnemyExtension enemyExtension;

		internal Encounter encounter;

		public bool raceEncounterGraffitiThatShouldRemainVisibleAfterwards;

		public override void Awake()
		{
			base.Awake();
			if (!raceEncounterGraffitiThatShouldRemainVisibleAfterwards)
			{
				Core.Instance.StartCoroutine(DelayInitialize());
			}
		}

		internal IEnumerator DelayInitialize()
		{
			while (WorldHandler.instance == null)
			{
				yield return null;
			}
			if (isUsingDecal)
			{
				while (topGraffiti.DecalRenderer == null || bottomGraffiti.DecalRenderer == null)
				{
					yield return null;
				}
			}
			InitializeFinisher();
		}

		internal void InitializeFinisher(StageProgress stageProgress = null)
		{
			base.SetDefaultData(stageProgress);
		}

		internal void ToggleRenderers(bool setActive)
		{
			if (isUsingDecal)
			{
				return;
			}
			for (int i = 0; i < decalTopPlaneAlternativeRenderer.Length; i++)
			{
				if (decalTopPlaneAlternativeRenderer[i] != null)
				{
					decalTopPlaneAlternativeRenderer[i].enabled = setActive;
				}
			}
			for (int j = 0; j < decalBottomPlaneAlternativeRenderer.Length; j++)
			{
				if (decalBottomPlaneAlternativeRenderer[j] != null)
				{
					decalBottomPlaneAlternativeRenderer[j].enabled = setActive;
				}
			}
		}

		public override void SetState(GraffitiState setState)
		{
			state = setState;
			switch (state)
			{
			case GraffitiState.NONE:
			case GraffitiState.CANCEL:
				inGraffitiMode = false;
				ToggleMarker(!ClaimedByPlayableCrew());
				break;
			case GraffitiState.PAINTING:
				inGraffitiMode = true;
				ToggleMarker((bool)encounter && playerIsAI);
				break;
			case GraffitiState.PAINTING_COMPLETE:
				if ((bool)encounter && (!raceEncounterGraffitiThatShouldRemainVisibleAfterwards || firstTime))
				{
					encounter.GraffitiDoneSignal(ClaimedByPlayableCrew(onlyPlayer: true));
					ToggleMarker(showMarker: false);
					marker.SetActive(value: false);
				}
				else
				{
					ToggleMarker(!ClaimedByPlayableCrew());
				}
				break;
			case GraffitiState.FINISHED:
				inGraffitiMode = false;
				if (ClaimedByPlayableCrew() && (!raceEncounterGraffitiThatShouldRemainVisibleAfterwards || firstTime))
				{
					OnFinish.Invoke();
				}
				firstTime = false;
				if ((bool)encounter)
				{
					ToggleMarker(showMarker: false);
				}
				else
				{
					ToggleMarker(!ClaimedByPlayableCrew());
				}
				break;
			}
			if ((bool)enemyDamager)
			{
				enemyDamager.SetGraffitiResult(this, state);
			}
			if ((bool)enemyExtension)
			{
				enemyExtension.SetGraffitiResult(this, state);
			}
			if (raceEncounterGraffitiThatShouldRemainVisibleAfterwards)
			{
				WriteToData();
			}
		}

		public override void GiveRep(bool noRepPickup, bool oldBottomWasClaimedByPlayableCrew)
		{
		}

		public override void SetDefaultData(StageProgress stageProgress = null)
		{
			InitializeFinisher(raceEncounterGraffitiThatShouldRemainVisibleAfterwards ? stageProgress : null);
		}

		public override void ReadFromData()
		{
			if (raceEncounterGraffitiThatShouldRemainVisibleAfterwards)
			{
				base.ReadFromData();
			}
		}

		public override void WriteToData()
		{
			if (raceEncounterGraffitiThatShouldRemainVisibleAfterwards)
			{
				base.WriteToData();
			}
		}

		protected override void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			if (raceEncounterGraffitiThatShouldRemainVisibleAfterwards)
			{
				base.OnRegisterToSceneObjectsRegister(sceneObjectsRegister);
			}
		}

		public override bool CanDoGraffiti(Player byPlayer)
		{
			bool result = true;
			if (encounter != null)
			{
				result = encounter.IsWinnable() || (raceEncounterGraffitiThatShouldRemainVisibleAfterwards && encounter.state == Encounter.EncounterState.CLOSED && ClaimedByPlayableCrew());
			}
			else if (inGraffitiMode || byPlayer.crew == topCrew)
			{
				result = false;
			}
			return result;
		}

		public override bool CanBreakAwayFromByEnemies()
		{
			return false;
		}
	}
}
