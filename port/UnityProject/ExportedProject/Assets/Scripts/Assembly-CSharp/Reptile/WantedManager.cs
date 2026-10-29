using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Reptile
{
	public class WantedManager : MonoBehaviour
	{
		public delegate void OnWantedHandler(bool wanted);

		public enum Crime
		{
			VANDALISM_SMALL = 0,
			VANDALISM_MEDIUM = 1,
			VANDALISM_LARGE = 2,
			VANDALISM_XLARGE = 3,
			RESTRICTED_AREA = 4,
			ASSAULT_OFFICER = 5,
			OFFICER_DOWN = 6,
			PEDESTRIAN_JOSTLE = 7,
			DEBUG = 8
		}

		public static WantedManager instance;

		[SerializeField]
		private AnimationCurve maxAttackersAtStars;

		[SerializeField]
		private AnimationCurve maxThreatAtStars;

		[SerializeField]
		private AnimationCurve maxCopAtStars;

		[SerializeField]
		private AnimationCurve maxShieldCopAtStars;

		[SerializeField]
		private AnimationCurve maxTurretAtStars;

		[SerializeField]
		private AnimationCurve maxSniperAtStars;

		[SerializeField]
		private AnimationCurve maxCopterAtStars;

		[SerializeField]
		private AnimationCurve maxTankwalkerAtStars;

		private int[] crimeCountToStars = new int[8] { 10, 11, 35, 60, 126, 198, 250, 300 };

		private float[] crimeMultipliersAtStars = new float[8] { 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f };

		private bool wanted;

		private int prevEnemiesAbleToSeePlayer;

		private int copCount;

		private int turretCount;

		private int shieldCopCount;

		private int sniperCount;

		private int copterCount;

		private int tankwalkerCount;

		private int deadCops;

		private int deadTurrets;

		private int deadShieldCops;

		private int deadSnipers;

		private int deadCopters;

		private int deadTankwalkers;

		private float deathReductionTimer;

		private float deathReductionDuration;

		private PoliceCutscenes cutscenes;

		private Core coreInstance;

		private WorldHandler worldhandlerInstance;

		private Player player;

		private APlatform platform;

		private float starProgress = -1f;

		private int crimeCount;

		private int crimeCountToActivateSurveillance;

		private bool wantedBlocked;

		private float crimeMultiplierLosePolice = 1f;

		private int lostPoliceCount;

		private SaveSlotData saveSlotData;

		private StageProgress stageProgress;

		private BasicEnemySpawner[] spawners;

		private List<BasicEnemySpawner> copSpawners = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> turretSpawners = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> shieldCopSpawners = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> sniperSpawners = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> copterSpawners = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> tankwalkerSpawners = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> copSpawnersAvailable = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> turretSpawnersAvailable = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> shieldCopSpawnersAvailable = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> sniperSpawnersAvailable = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> copterSpawnersAvailable = new List<BasicEnemySpawner>();

		private List<BasicEnemySpawner> tankwalkerSpawnersAvailable = new List<BasicEnemySpawner>();

		private int maxShieldCops;

		private int maxCops;

		private int maxTurrets;

		private int maxSnipers;

		private int maxCopters;

		private int maxTankwalkers;

		private bool copSpawnersAvailabilitySet;

		private bool turretSpawnersAvailabilitySet;

		private bool shieldCopSpawnersAvailabilitySet;

		private bool sniperSpawnersAvailabilitySet;

		private bool copterSpawnersAvailabilitySet;

		private bool tankwalkerSpawnersAvailabilitySet;

		private List<BasicEnemySpawner> spawnerGroup = new List<BasicEnemySpawner>();

		private Crime lastCrime;

		public int WantedStars => Mathf.FloorToInt(starProgress);

		public float WantedStarProgress => starProgress;

		public bool Wanted => wanted;

		public static event OnWantedHandler OnWanted;

		public void Awake()
		{
			instance = this;
		}

		public void Init()
		{
			coreInstance = Core.Instance;
			worldhandlerInstance = WorldHandler.instance;
			player = worldhandlerInstance.GetCurrentPlayer();
			platform = coreInstance.Platform;
			UpdateStageProgress();
			Core.OnUpdate += ManageHeat;
			crimeCountToActivateSurveillance = crimeCountToStars[0] - 1;
			coreInstance.SaveManager.SaveCurrentSaveSlot();
			spawners = Object.FindObjectsOfType<BasicEnemySpawner>();
			cutscenes = Object.FindObjectOfType<PoliceCutscenes>();
			for (int i = 0; i < spawners.Length; i++)
			{
				for (int j = 0; j < spawners[i].SpawnableEnemyTypes.Length; j++)
				{
					switch (spawners[i].SpawnableEnemyTypes[j])
					{
					case EnemyType.BASICCOP:
						copSpawners.Add(spawners[i]);
						break;
					case EnemyType.TURRET:
						turretSpawners.Add(spawners[i]);
						break;
					case EnemyType.SHIELDCOP:
						shieldCopSpawners.Add(spawners[i]);
						break;
					case EnemyType.SNIPERCOP:
						sniperSpawners.Add(spawners[i]);
						break;
					case EnemyType.COPTER:
						copterSpawners.Add(spawners[i]);
						break;
					case EnemyType.TANKWALKER:
						tankwalkerSpawners.Add(spawners[i]);
						break;
					}
				}
			}
		}

		public void OnDestroy()
		{
			instance = null;
			coreInstance = null;
			player = null;
			Core.OnUpdate -= ManageHeat;
		}

		public void ManageHeat()
		{
			maxShieldCops = Mathf.RoundToInt(maxShieldCopAtStars.Evaluate(starProgress));
			maxCops = Mathf.RoundToInt(maxCopAtStars.Evaluate(starProgress));
			maxTurrets = Mathf.RoundToInt(maxTurretAtStars.Evaluate(starProgress));
			maxSnipers = Mathf.RoundToInt(maxSniperAtStars.Evaluate(starProgress));
			maxCopters = Mathf.RoundToInt(maxCopterAtStars.Evaluate(starProgress));
			maxTankwalkers = Mathf.RoundToInt(maxTankwalkerAtStars.Evaluate(starProgress));
			UpdateStageProgress();
			int num = 0;
			copSpawnersAvailable.Clear();
			turretSpawnersAvailable.Clear();
			shieldCopSpawnersAvailable.Clear();
			sniperSpawnersAvailable.Clear();
			copterSpawnersAvailable.Clear();
			tankwalkerSpawnersAvailable.Clear();
			copSpawnersAvailabilitySet = (turretSpawnersAvailabilitySet = (shieldCopSpawnersAvailabilitySet = (sniperSpawnersAvailabilitySet = (copterSpawnersAvailabilitySet = (tankwalkerSpawnersAvailabilitySet = false)))));
			ResetEnemyTypeCounts();
			deathReductionTimer -= Core.dt;
			if (deathReductionTimer <= 0f)
			{
				deathReductionDuration = Mathf.Max(deathReductionDuration - 5f, 5f);
				deathReductionTimer = deathReductionDuration;
				deadCops = ((deadCops > 0) ? Mathf.Max(0, deadCops - 2) : deadCops);
				deadTurrets -= ((deadTurrets > 0) ? 1 : 0);
				deadShieldCops -= ((deadShieldCops > 0) ? 1 : 0);
				deadSnipers -= ((deadSnipers > 0) ? 1 : 0);
				deadCopters -= ((deadCopters > 0) ? 1 : 0);
				deadTankwalkers -= ((deadTankwalkers > 0) ? 1 : 0);
			}
			Vector3 position = player.transform.position;
			for (int i = 0; i < worldhandlerInstance.subscribedEnemies.Count; i++)
			{
				BasicEnemy basicEnemy = worldhandlerInstance.subscribedEnemies[i];
				if (basicEnemy == null)
				{
					continue;
				}
				TrackEnemyTypeCount(basicEnemy);
				if (basicEnemy.ShouldBeReplaced())
				{
					switch (basicEnemy.EnemyType)
					{
					case EnemyType.BASICCOP:
						if (!copSpawnersAvailabilitySet)
						{
							SetSpawnerAvailability(EnemyType.BASICCOP, position);
						}
						if (copSpawnersAvailable.Count > 0 && maxCops - deadCops >= copCount)
						{
							copSpawnersAvailable[0].SpawnEnemy(EnemyType.BASICCOP, position);
							copSpawnersAvailable.RemoveAt(0);
							basicEnemy.StandDown();
						}
						break;
					case EnemyType.TURRET:
						if (!turretSpawnersAvailabilitySet)
						{
							SetSpawnerAvailability(EnemyType.TURRET, position);
						}
						if (turretSpawnersAvailable.Count > 0 && maxTurrets - deadTurrets >= turretCount)
						{
							turretSpawnersAvailable[0].SpawnEnemy(EnemyType.TURRET, position);
							turretSpawnersAvailable.RemoveAt(0);
							basicEnemy.StandDown();
						}
						break;
					case EnemyType.SHIELDCOP:
						if (!shieldCopSpawnersAvailabilitySet)
						{
							SetSpawnerAvailability(EnemyType.SHIELDCOP, position);
						}
						if (shieldCopSpawnersAvailable.Count > 0 && maxShieldCops - deadShieldCops >= shieldCopCount)
						{
							shieldCopSpawnersAvailable[0].SpawnEnemy(EnemyType.SHIELDCOP, position);
							shieldCopSpawnersAvailable.RemoveAt(0);
							basicEnemy.StandDown();
						}
						break;
					case EnemyType.SNIPERCOP:
						if (!sniperSpawnersAvailabilitySet)
						{
							SetSpawnerAvailability(EnemyType.SNIPERCOP, position);
						}
						if (sniperSpawnersAvailable.Count > 0 && maxSnipers - deadSnipers >= sniperCount)
						{
							sniperSpawnersAvailable[0].SpawnEnemy(EnemyType.SNIPERCOP, position);
							sniperSpawnersAvailable.RemoveAt(0);
							basicEnemy.StandDown();
						}
						break;
					case EnemyType.COPTER:
						if (!copterSpawnersAvailabilitySet)
						{
							SetSpawnerAvailability(EnemyType.COPTER, position);
						}
						if (copterSpawnersAvailable.Count > 0 && maxCopters - deadCopters >= copterCount)
						{
							copterSpawnersAvailable[0].SpawnEnemy(EnemyType.COPTER, position);
							copterSpawnersAvailable.RemoveAt(0);
							basicEnemy.StandDown();
						}
						break;
					case EnemyType.TANKWALKER:
						if (!tankwalkerSpawnersAvailabilitySet)
						{
							SetSpawnerAvailability(EnemyType.TANKWALKER, position);
						}
						if (tankwalkerSpawnersAvailable.Count > 0 && maxTankwalkers - deadTankwalkers >= tankwalkerCount)
						{
							tankwalkerSpawnersAvailable[0].SpawnEnemy(EnemyType.TANKWALKER, position);
							tankwalkerSpawnersAvailable.RemoveAt(0);
							basicEnemy.StandDown();
						}
						break;
					}
					num++;
				}
				else
				{
					num++;
				}
			}
			if (maxShieldCops - deadShieldCops > shieldCopCount)
			{
				if (!shieldCopSpawnersAvailabilitySet)
				{
					SetSpawnerAvailability(EnemyType.SHIELDCOP, position);
				}
				for (int j = 0; j < shieldCopSpawnersAvailable.Count; j++)
				{
					if (maxShieldCops - deadShieldCops > shieldCopCount && shieldCopSpawnersAvailable[j].SpawnEnemy(EnemyType.SHIELDCOP, position))
					{
						shieldCopCount++;
						num++;
					}
				}
			}
			if (maxCops - deadCops > copCount)
			{
				if (!copSpawnersAvailabilitySet)
				{
					SetSpawnerAvailability(EnemyType.BASICCOP, position);
				}
				for (int k = 0; k < copSpawnersAvailable.Count; k++)
				{
					if (maxCops - deadCops > copCount && copSpawnersAvailable[k].SpawnEnemy(EnemyType.BASICCOP, position))
					{
						copCount++;
						num++;
					}
				}
			}
			if (maxTurrets - deadTurrets > turretCount)
			{
				if (!turretSpawnersAvailabilitySet)
				{
					SetSpawnerAvailability(EnemyType.TURRET, position);
				}
				for (int l = 0; l < turretSpawnersAvailable.Count; l++)
				{
					if (maxTurrets - deadTurrets > turretCount && turretSpawnersAvailable[l].SpawnEnemy(EnemyType.TURRET, position))
					{
						turretCount++;
						num++;
					}
				}
			}
			if (maxSnipers - deadSnipers > sniperCount)
			{
				if (!sniperSpawnersAvailabilitySet)
				{
					SetSpawnerAvailability(EnemyType.SNIPERCOP, position);
				}
				for (int m = 0; m < sniperSpawnersAvailable.Count; m++)
				{
					if (maxSnipers - deadSnipers > sniperCount && sniperSpawnersAvailable[m].SpawnEnemy(EnemyType.SNIPERCOP, position))
					{
						sniperCount++;
						num++;
					}
				}
			}
			if (maxCopters - deadCopters > copterCount)
			{
				if (!copterSpawnersAvailabilitySet)
				{
					SetSpawnerAvailability(EnemyType.COPTER, position);
				}
				for (int n = 0; n < copterSpawnersAvailable.Count; n++)
				{
					if (maxCopters - deadCopters > copterCount && copterSpawnersAvailable[n].SpawnEnemy(EnemyType.COPTER, position))
					{
						copterCount++;
						num++;
					}
				}
			}
			if (maxTankwalkers - deadTankwalkers > tankwalkerCount)
			{
				if (!tankwalkerSpawnersAvailabilitySet)
				{
					SetSpawnerAvailability(EnemyType.TANKWALKER, position);
				}
				for (int num2 = 0; num2 < tankwalkerSpawnersAvailable.Count; num2++)
				{
					if (maxTankwalkers - deadTankwalkers > tankwalkerCount && tankwalkerSpawnersAvailable[num2].SpawnEnemy(EnemyType.TANKWALKER, position))
					{
						tankwalkerCount++;
						num++;
					}
				}
			}
			prevEnemiesAbleToSeePlayer = num;
			if (worldhandlerInstance.currentEncounter != null && worldhandlerInstance.currentEncounter.stopWantedOnStart)
			{
				StopPlayerWantedStatus(showMessage: false);
			}
		}

		private void SetSpawnerAvailability(EnemyType enemyType, Vector3 playerPosition)
		{
			switch (enemyType)
			{
			case EnemyType.BASICCOP:
				spawnerGroup = copSpawners;
				copSpawnersAvailabilitySet = true;
				break;
			case EnemyType.TURRET:
				spawnerGroup = turretSpawners;
				turretSpawnersAvailabilitySet = true;
				break;
			case EnemyType.SHIELDCOP:
				spawnerGroup = shieldCopSpawners;
				shieldCopSpawnersAvailabilitySet = true;
				break;
			case EnemyType.SNIPERCOP:
				spawnerGroup = sniperSpawners;
				sniperSpawnersAvailabilitySet = true;
				break;
			case EnemyType.COPTER:
				spawnerGroup = copterSpawners;
				copterSpawnersAvailabilitySet = true;
				break;
			case EnemyType.TANKWALKER:
				spawnerGroup = tankwalkerSpawners;
				tankwalkerSpawnersAvailabilitySet = true;
				break;
			}
			for (int i = 0; i < spawnerGroup.Count; i++)
			{
				if (spawnerGroup[i].isActiveAndEnabled && spawnerGroup[i].CanSpawnEnemy(enemyType, playerPosition))
				{
					switch (enemyType)
					{
					case EnemyType.BASICCOP:
						copSpawnersAvailable.Add(spawnerGroup[i]);
						break;
					case EnemyType.TURRET:
						turretSpawnersAvailable.Add(spawnerGroup[i]);
						break;
					case EnemyType.SHIELDCOP:
						shieldCopSpawnersAvailable.Add(spawnerGroup[i]);
						break;
					case EnemyType.SNIPERCOP:
						sniperSpawnersAvailable.Add(spawnerGroup[i]);
						break;
					case EnemyType.COPTER:
						copterSpawnersAvailable.Add(spawnerGroup[i]);
						break;
					case EnemyType.TANKWALKER:
						tankwalkerSpawnersAvailable.Add(spawnerGroup[i]);
						break;
					}
				}
			}
		}

		public bool CanLosePolice()
		{
			int num = 0;
			for (int i = 0; i < worldhandlerInstance.subscribedEnemies.Count; i++)
			{
				BasicEnemy basicEnemy = worldhandlerInstance.subscribedEnemies[i];
				if (!(basicEnemy == null) && (WorldHandler.instance.IsReallyInScreen(basicEnemy.tf.position) || Vector3.Distance(basicEnemy.tf.position, player.tf.position) < 60f || (!WorldHandler.instance.IsReallyInScreen(basicEnemy.tf.position) && Vector3.Distance(basicEnemy.tf.position, player.tf.position) < 20f)))
				{
					num++;
				}
			}
			return num == 0;
		}

		public void StopPlayerWantedStatus(bool showMessage)
		{
			StopPlayerWantedStatus(showMessage, increaseLosePoliceCount: false);
		}

		public void StopPlayerWantedStatus(bool showMessage, bool increaseLosePoliceCount)
		{
			if (wanted)
			{
				wanted = false;
				starProgress = 0f;
				crimeCount = 0;
				SaveProgress();
				if (increaseLosePoliceCount)
				{
					lostPoliceCount++;
					crimeMultiplierLosePolice = Mathf.Lerp(1f, 2f, (float)lostPoliceCount / 2f);
				}
				player.ShowWantedUI(shouldShow: false);
				StandDownEnemies();
				if (OnWanted != null)
				{
					OnWanted(wanted: false);
				}
			}
		}

		public void BlockWantedStatus(bool shouldBlock)
		{
			wantedBlocked = shouldBlock;
			if (wantedBlocked)
			{
				StopPlayerWantedStatus(showMessage: false);
			}
		}

		public void StandDownEnemies()
		{
			for (int num = worldhandlerInstance.subscribedEnemies.Count - 1; num >= 0; num--)
			{
				worldhandlerInstance.subscribedEnemies[num].StandDown();
			}
		}

		public int GetMaxAttacksForWantedLevel()
		{
			return Mathf.RoundToInt(maxAttackersAtStars.Evaluate(starProgress));
		}

		public int GetMaxThreatForWantedLevel()
		{
			return Mathf.RoundToInt(maxThreatAtStars.Evaluate(starProgress));
		}

		public void ManuallySetWanted(int stars)
		{
			crimeCount = crimeCountToStars[stars] - 1;
			starProgress = 0f;
			ProcessCrime(Crime.VANDALISM_SMALL);
			MakePlayerWanted(Crime.VANDALISM_SMALL);
		}

		public void AddDebugeCrime()
		{
			if (!wantedBlocked)
			{
				ProcessCrime(Crime.DEBUG);
			}
		}

		public void SetDebugStarAmount(int starAmount)
		{
			crimeCount = crimeCountToStars[starAmount];
			SaveProgress();
			UpdateStarProgress();
			if (starAmount == 0)
			{
				StopPlayerWantedStatus(showMessage: false);
			}
		}

		private void ResetEnemyTypeCounts()
		{
			copCount = (turretCount = (shieldCopCount = (sniperCount = (copterCount = (tankwalkerCount = 0)))));
		}

		private void TrackEnemyTypeCount(BasicEnemy basicEnemy)
		{
			switch (basicEnemy.EnemyType)
			{
			case EnemyType.BASICCOP:
				copCount++;
				break;
			case EnemyType.TURRET:
				turretCount++;
				break;
			case EnemyType.SHIELDCOP:
				shieldCopCount++;
				break;
			case EnemyType.SNIPERCOP:
				sniperCount++;
				break;
			case EnemyType.COPTER:
				copterCount++;
				break;
			case EnemyType.TANKWALKER:
				tankwalkerCount++;
				break;
			}
		}

		private bool RoomForEnemyType(BasicEnemy basicEnemy, bool replacing, int roomLeft = 1)
		{
			return basicEnemy.EnemyType switch
			{
				EnemyType.BASICCOP => maxCopAtStars.Evaluate(starProgress) >= (float)(copCount - (replacing ? 1 : 0) + roomLeft), 
				EnemyType.TURRET => maxTurretAtStars.Evaluate(starProgress) >= (float)(turretCount - (replacing ? 1 : 0) + roomLeft), 
				EnemyType.SHIELDCOP => maxShieldCopAtStars.Evaluate(starProgress) >= (float)(shieldCopCount - (replacing ? 1 : 0) + roomLeft), 
				EnemyType.SNIPERCOP => maxSniperAtStars.Evaluate(starProgress) >= (float)(sniperCount - (replacing ? 1 : 0) + roomLeft), 
				EnemyType.COPTER => maxCopterAtStars.Evaluate(starProgress) >= (float)(copterCount - (replacing ? 1 : 0) + roomLeft), 
				EnemyType.TANKWALKER => maxTankwalkerAtStars.Evaluate(starProgress) >= (float)(tankwalkerCount - (replacing ? 1 : 0) + roomLeft), 
				_ => false, 
			};
		}

		private void UpdateStageProgress()
		{
			if (saveSlotData == null)
			{
				saveSlotData = coreInstance.SaveManager.CurrentSaveSlot;
			}
			if (saveSlotData != null && stageProgress == null)
			{
				stageProgress = saveSlotData.GetCurrentStageProgress();
			}
			if (saveSlotData != null)
			{
				crimeCount = saveSlotData.crimeCount;
				if (crimeCount > crimeCountToStars[0])
				{
					wanted = true;
					player.ShowWantedUI(shouldShow: true);
					UpdateStarProgress(showMessage: false);
				}
			}
			if (stageProgress != null && !stageProgress.policeAllowed != wantedBlocked)
			{
				BlockWantedStatus(!stageProgress.policeAllowed);
			}
		}

		public void CrimePerpetrated(Crime crime, bool forceResponce = false)
		{
			if (!wantedBlocked)
			{
				ProcessCrime(crime);
				if (!wanted && crimeCount >= crimeCountToStars[1])
				{
					MakePlayerWanted(crime);
				}
			}
		}

		public void CopDied(EnemyType type)
		{
			deathReductionTimer = Mathf.Min(deathReductionTimer + 5f, 20f);
			deathReductionDuration = Mathf.Min(deathReductionDuration + 5f, 20f);
			switch (type)
			{
			case EnemyType.BASICCOP:
				deadCops = Mathf.Min(maxCops, deadCops + 1);
				break;
			case EnemyType.TURRET:
				deadTurrets = Mathf.Min(maxTurrets, deadTurrets + 1);
				break;
			case EnemyType.SHIELDCOP:
				deadShieldCops = Mathf.Min(maxShieldCops, deadShieldCops + 1);
				break;
			case EnemyType.SNIPERCOP:
				deadSnipers = Mathf.Min(maxSnipers, deadSnipers + 1);
				break;
			case EnemyType.COPTER:
				deadCopters = Mathf.Min(maxCopters, deadCopters + 1);
				break;
			case EnemyType.TANKWALKER:
				deadTankwalkers = Mathf.Min(maxTankwalkers, deadTankwalkers + 1);
				break;
			}
		}

		public PoliceTubeSpawner GetNearestPoliceTube(Vector3 pos, int areaMask)
		{
			PoliceTubeSpawner result = null;
			if (copSpawners.Count > 0)
			{
				copSpawners.Sort((BasicEnemySpawner a, BasicEnemySpawner b) => (pos - a.tf.position).sqrMagnitude.CompareTo((pos - b.tf.position).sqrMagnitude));
				for (int num = 0; num < copSpawners.Count; num++)
				{
					if (!(copSpawners[num] as PoliceTubeSpawner).ClaimedForReturn)
					{
						NavMeshPath navMeshPath = new NavMeshPath();
						if (NavMesh.CalculatePath(pos, copSpawners[num].tf.position, areaMask, navMeshPath) && (navMeshPath.corners[navMeshPath.corners.Length - 1] - copSpawners[num].tf.position).sqrMagnitude < 1f)
						{
							return copSpawners[num] as PoliceTubeSpawner;
						}
					}
				}
			}
			return result;
		}

		private void ProcessCrime(Crime crime)
		{
			lastCrime = crime;
			int num = crimeCount;
			int num2 = crimeCount;
			int num3 = Mathf.RoundToInt(coreInstance.SaveManager.CurrentSaveSlot.maxWantedStars);
			switch (crime)
			{
			case Crime.OFFICER_DOWN:
				num2 += Mathf.RoundToInt(6f * crimeMultipliersAtStars[num3] * crimeMultiplierLosePolice);
				break;
			case Crime.RESTRICTED_AREA:
				num2 += Mathf.RoundToInt(6f * crimeMultipliersAtStars[num3] * crimeMultiplierLosePolice);
				break;
			case Crime.VANDALISM_SMALL:
				num2 = Mathf.RoundToInt((float)num2 + 6f * crimeMultipliersAtStars[num3] * crimeMultiplierLosePolice);
				break;
			case Crime.VANDALISM_MEDIUM:
				num2 = Mathf.RoundToInt((float)num2 + 8f * crimeMultipliersAtStars[num3] * crimeMultiplierLosePolice);
				break;
			case Crime.VANDALISM_LARGE:
				num2 = Mathf.RoundToInt((float)num2 + 10f * crimeMultipliersAtStars[num3] * crimeMultiplierLosePolice);
				break;
			case Crime.VANDALISM_XLARGE:
				num2 = Mathf.RoundToInt((float)num2 + 12f * crimeMultipliersAtStars[num3]);
				break;
			case Crime.DEBUG:
				num2 += Mathf.RoundToInt(9f * crimeMultipliersAtStars[num3]);
				break;
			}
			if (num != num2)
			{
				num2 = LimitCrimeCountToNextStarValue(num2);
				crimeCount = num2;
				SaveProgress();
				UpdateStarProgress();
			}
		}

		private void MakePlayerWanted(Crime crime)
		{
			wanted = true;
			player.ShowWantedUI(shouldShow: true);
			deadCops = (deadCopters = (deadShieldCops = (deadSnipers = (deadTankwalkers = (deadTurrets = 0)))));
			if (OnWanted != null)
			{
				OnWanted(wanted: true);
			}
		}

		private void SaveProgress()
		{
			if (saveSlotData == null)
			{
				saveSlotData = coreInstance.SaveManager.CurrentSaveSlot;
			}
			if (saveSlotData != null)
			{
				saveSlotData.crimeCount = crimeCount;
			}
			coreInstance.SaveManager.SaveCurrentSaveSlot();
		}

		private int LimitCrimeCountToNextStarValue(int count)
		{
			int num = -1;
			for (int i = 0; i < crimeCountToStars.Length; i++)
			{
				if (crimeCountToStars[i] <= count)
				{
					num = i;
					if (num > WantedStars)
					{
						return crimeCountToStars[num];
					}
				}
			}
			return count;
		}

		private void UpdateStarProgress(bool showMessage = true)
		{
			float starProgressForCrimeCount = GetStarProgressForCrimeCount();
			int num = Mathf.FloorToInt(starProgressForCrimeCount);
			if (num != WantedStars)
			{
				player.ChangeWantedStars(num);
				if ((starProgressForCrimeCount > starProgress) & showMessage)
				{
					DoNewStarSequence(num);
					if (platform.HasAchievementsSupport && maxTankwalkerAtStars.Evaluate(starProgressForCrimeCount) >= 1f)
					{
						platform.Achievements.UnlockAchievement(AchievementType.FunkStar);
					}
				}
			}
			starProgress = starProgressForCrimeCount;
		}

		private float GetStarProgressForCrimeCount()
		{
			float num = WantedStars;
			for (int i = 0; i < crimeCountToStars.Length; i++)
			{
				if (crimeCountToStars[i] <= crimeCount)
				{
					num = i;
				}
			}
			if (num < (float)(crimeCountToStars.Length - 1) && num > -1f)
			{
				float num2 = Mathf.Lerp(0f, 1f, (float)(crimeCount - crimeCountToStars[Mathf.RoundToInt(num)]) / (float)(crimeCountToStars[Mathf.RoundToInt(num) + 1] - crimeCountToStars[Mathf.RoundToInt(num)]));
				num += num2;
			}
			return Mathf.Clamp(num, -1f, coreInstance.SaveManager.CurrentSaveSlot.maxWantedStars + 1f - 0.1f);
		}

		private void DoNewStarSequence(int star)
		{
			if (cutscenes != null)
			{
				cutscenes.PlaySequenceForStars(star);
			}
		}

		private void DecreaseStars()
		{
			if (WantedStars == 1)
			{
				StopPlayerWantedStatus(showMessage: true);
			}
			else
			{
				crimeCount = crimeCountToStars[Mathf.FloorToInt(starProgress) - 1];
				BasicEnemy basicEnemy = null;
				for (int i = 0; i < worldhandlerInstance.subscribedEnemies.Count; i++)
				{
					basicEnemy = worldhandlerInstance.subscribedEnemies[i];
					if (basicEnemy.EnemyType == EnemyType.BASICCOP && maxCopAtStars.Evaluate(starProgress) < 1f)
					{
						basicEnemy.StandDown();
					}
					if (basicEnemy.EnemyType == EnemyType.TURRET && maxTurretAtStars.Evaluate(starProgress) < 1f)
					{
						basicEnemy.StandDown();
					}
					if (basicEnemy.EnemyType == EnemyType.SHIELDCOP && maxShieldCopAtStars.Evaluate(starProgress) < 1f)
					{
						basicEnemy.StandDown();
					}
					if (basicEnemy.EnemyType == EnemyType.SNIPERCOP && maxSniperAtStars.Evaluate(starProgress) < 1f)
					{
						basicEnemy.StandDown();
					}
					if (basicEnemy.EnemyType == EnemyType.COPTER && maxCopterAtStars.Evaluate(starProgress) < 1f)
					{
						basicEnemy.StandDown();
					}
					if (basicEnemy.EnemyType == EnemyType.TANKWALKER && maxTankwalkerAtStars.Evaluate(starProgress) < 1f)
					{
						basicEnemy.StandDown();
					}
				}
				UpdateStarProgress();
			}
			SaveProgress();
		}
	}
}
