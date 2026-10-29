using System;
using System.Collections.Generic;

namespace Reptile
{
	public static class ProgressableDataFactory
	{
		private static Dictionary<ProgressableType, Func<ProgressableData>> progressableTypes = new Dictionary<ProgressableType, Func<ProgressableData>>();

		private static bool isInitialized = false;

		private static void Init()
		{
			progressableTypes.Add(ProgressableType.GRAFFITI_SPOT_PROGRESS, () => new GraffitiSpotProgress());
			progressableTypes.Add(ProgressableType.ENCOUNTER_PROGRESS, () => new EncounterProgress());
			progressableTypes.Add(ProgressableType.NPC_PROGRESS, () => new NPCProgress());
			progressableTypes.Add(ProgressableType.OBJECT_PROGRESS, () => new ObjectProgress());
			progressableTypes.Add(ProgressableType.STAGE_TRANSITION_PROGRESS, () => new StageTransitionProgress());
			progressableTypes.Add(ProgressableType.COLLECTABLE_PROGRESS, () => new CollectableProgress());
			progressableTypes.Add(ProgressableType.PHOTO_PROGRESS, () => new PhotoObjectProgress());
			isInitialized = true;
		}

		public static ProgressableData Create(ProgressableType type)
		{
			if (!isInitialized)
			{
				Init();
			}
			return progressableTypes[type]();
		}
	}
}
