using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class StoryManager
	{
		public const int MaxObjects = 20;

		private int currentAmountOfObjects;

		private GameObject[] currentObjectivesObjects = new GameObject[20];

		private List<GameplayEvent> gameplayEvents;

		private List<NPC> npcs;

		private List<ProgressObject> progressObjects;

		private List<StageTransition> stageTransitions;

		private List<GraffitiSpot> graffitiSpots;

		public void Init(Stage newStage)
		{
			SceneObjectsRegister sceneObjectsRegister = WorldHandler.instance.SceneObjectsRegister;
			gameplayEvents = sceneObjectsRegister.gameplayEvents;
			npcs = sceneObjectsRegister.NPCs;
			progressObjects = sceneObjectsRegister.progressObjects;
			stageTransitions = sceneObjectsRegister.stageTransitions;
			graffitiSpots = sceneObjectsRegister.grafSpots;
			UpdateObjectives();
		}

		public GameObject[] GetCurrentObjectivesObjects(out int currentAmountOfObjects)
		{
			currentAmountOfObjects = this.currentAmountOfObjects;
			return currentObjectivesObjects;
		}

		public void SetStoryObjective(Story.ObjectiveID newObjective)
		{
			if (newObjective != Story.ObjectiveID.NONE)
			{
				Core.Instance.SaveManager.CurrentSaveSlot.CurrentStoryObjective = newObjective;
				Core.Instance.SaveManager.SaveCurrentSaveSlot();
				UpdateObjectives();
				if (!(Mapcontroller.Instance == null))
				{
					Mapcontroller.Instance.UpdateObjectivePins();
				}
			}
		}

		public void UpdateObjectives()
		{
			if (gameplayEvents == null && npcs == null)
			{
				return;
			}
			Story.ObjectiveInfo currentObjectiveInfo = Story.GetCurrentObjectiveInfo();
			currentAmountOfObjects = 0;
			int count = gameplayEvents.Count;
			for (int i = 0; i < count; i++)
			{
				if (currentAmountOfObjects >= 20)
				{
					break;
				}
				GameplayEvent gameplayEvent = gameplayEvents[i];
				if (gameplayEvent.available && gameplayEvent.beTargetForObjective == currentObjectiveInfo.id && gameplayEvent.canOpen)
				{
					currentObjectivesObjects[currentAmountOfObjects] = gameplayEvent.gameObject;
					currentAmountOfObjects++;
				}
			}
			int count2 = npcs.Count;
			for (int j = 0; j < count2; j++)
			{
				if (currentAmountOfObjects >= 20)
				{
					break;
				}
				NPC nPC = npcs[j];
				if (nPC.isActive && nPC.beTargetForObjective == currentObjectiveInfo.id && nPC.canOpen && (nPC.objectiveTargetOnlyFromDialogueLevel <= 0 || nPC.dialogueLevel >= nPC.objectiveTargetOnlyFromDialogueLevel))
				{
					currentObjectivesObjects[currentAmountOfObjects] = nPC.gameObject;
					currentAmountOfObjects++;
				}
			}
			int count3 = progressObjects.Count;
			for (int k = 0; k < count3; k++)
			{
				if (currentAmountOfObjects >= 20)
				{
					break;
				}
				ProgressObject progressObject = progressObjects[k];
				if (progressObject.isActive && progressObject.showOnMapWhenObjectiveSet && progressObject.beTargetForObjective == currentObjectiveInfo.id)
				{
					currentObjectivesObjects[currentAmountOfObjects] = progressObject.gameObject;
					currentAmountOfObjects++;
				}
			}
			int count4 = graffitiSpots.Count;
			for (int l = 0; l < count4; l++)
			{
				if (currentAmountOfObjects >= 20)
				{
					break;
				}
				GraffitiSpot graffitiSpot = graffitiSpots[l];
				if (graffitiSpot.isOpen && !graffitiSpot.ClaimedByPlayableCrew() && graffitiSpot.beTargetForObjective == currentObjectiveInfo.id)
				{
					currentObjectivesObjects[currentAmountOfObjects] = graffitiSpot.gameObject;
					currentAmountOfObjects++;
				}
			}
			_ = currentAmountOfObjects;
			_ = 20;
		}
	}
}
