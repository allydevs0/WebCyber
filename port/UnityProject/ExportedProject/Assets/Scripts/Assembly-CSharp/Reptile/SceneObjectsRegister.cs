using System.Collections.Generic;

namespace Reptile
{
	public class SceneObjectsRegister
	{
		public delegate void OnRegisterToWorldHandlerDelegate(SceneObjectsRegister sceneObjectsRegister);

		private List<IInitializableSceneObject> sceneObjects = new List<IInitializableSceneObject>();

		public List<Player> players = new List<Player>();

		public List<GameplayEvent> gameplayEvents = new List<GameplayEvent>();

		public List<NPC> NPCs = new List<NPC>();

		public List<GraffitiSpot> grafSpots = new List<GraffitiSpot>();

		public List<ProgressObject> progressObjects = new List<ProgressObject>();

		public List<Pickup> pickups = new List<Pickup>();

		public List<PlayerSpawner> playerSpawners = new List<PlayerSpawner>();

		public List<StageTransition> stageTransitions = new List<StageTransition>();

		public List<AProgressable> progressables = new List<AProgressable>();

		public List<PlayerAIPath> paths = new List<PlayerAIPath>();

		public List<StreetLifeCluster> streetlifeClusters = new List<StreetLifeCluster>();

		public List<StageChunk> stageChunks = new List<StageChunk>();

		private bool isDisposed;

		public static event OnRegisterToWorldHandlerDelegate OnRegisterToSceneObjectsRegister;

		public void Dispose()
		{
			sceneObjects = null;
			players = null;
			gameplayEvents = null;
			NPCs = null;
			grafSpots = null;
			progressObjects = null;
			pickups = null;
			playerSpawners = null;
			stageTransitions = null;
			progressables = null;
			paths = null;
			streetlifeClusters = null;
			stageChunks = null;
			isDisposed = true;
		}

		public void RegisterAllSceneObjects()
		{
			if (OnRegisterToSceneObjectsRegister != null)
			{
				OnRegisterToSceneObjectsRegister(this);
			}
		}

		public void RegisterStageTransition(StageTransition stageTransition)
		{
			stageTransitions.Add(stageTransition);
			RegisterSceneObject(stageTransition);
		}

		public void RegisterPlayerSpawner(PlayerSpawner playerSpawner)
		{
			playerSpawners.Add(playerSpawner);
		}

		public void RegisterProgressObject(ProgressObject progressObject)
		{
			progressObjects.Add(progressObject);
			RegisterSceneObject(progressObject);
		}

		public void RegisterGraffitiSpot(GraffitiSpot graffitiSpot)
		{
			grafSpots.Add(graffitiSpot);
			RegisterSceneObject(graffitiSpot);
		}

		public void RegisterGameplayEvent(GameplayEvent gameplayEvent)
		{
			gameplayEvents.Add(gameplayEvent);
			RegisterSceneObject(gameplayEvent);
		}

		public void RegisterNPC(NPC npc)
		{
			NPCs.Add(npc);
			RegisterSceneObject(npc);
		}

		public void RegisterPickup(Pickup pickup)
		{
			pickups.Add(pickup);
		}

		public void RegisterProgressableObject(AProgressable progressable)
		{
			progressables.Add(progressable);
		}

		public void RegisterPath(PlayerAIPath path)
		{
			paths.Add(path);
		}

		public void RegisterPlayer(Player player)
		{
			players.Add(player);
		}

		public void RegisterStreetLifeCluster(StreetLifeCluster cluster)
		{
			streetlifeClusters.Add(cluster);
		}

		public void RegisterSceneObject(IInitializableSceneObject sceneObject)
		{
			sceneObjects.Add(sceneObject);
		}

		public void RegisterStageChunk(StageChunk stageChunk)
		{
			stageChunks.Add(stageChunk);
		}

		public void InitAllSceneObjects()
		{
			foreach (IInitializableSceneObject sceneObject in sceneObjects)
			{
				sceneObject.InitSceneObject();
			}
		}

		public void RemoveDynamicPickup(DynamicPickup dynamicPickup)
		{
			if (!isDisposed)
			{
				pickups.Remove(dynamicPickup);
			}
		}

		public DreamEncounter RetrieveDreamEncounter()
		{
			foreach (GameplayEvent gameplayEvent in gameplayEvents)
			{
				if (gameplayEvent is DreamEncounter)
				{
					return gameplayEvent as DreamEncounter;
				}
			}
			return null;
		}

		internal int GetTotalGraffitiREP()
		{
			int num = 0;
			for (int i = 0; i < grafSpots.Count; i++)
			{
				if (!(grafSpots[i] is GraffitiSpotFinisher))
				{
					num += GraffitiSpot.GetREP(grafSpots[i].size);
				}
			}
			return num;
		}
	}
}
