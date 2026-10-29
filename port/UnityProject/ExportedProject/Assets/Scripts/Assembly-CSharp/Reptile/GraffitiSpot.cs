using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using ch.sycoforge.Decal;

namespace Reptile
{
	public class GraffitiSpot : AProgressable, IInitializableSceneObject
	{
		public enum AttachType
		{
			DEFAULT = 0,
			COPS = 1,
			DREAMGOAL = 2
		}

		protected GraffitiArtInfo graffitiArtInfo;

		public Story.ObjectiveID beTargetForObjective = Story.ObjectiveID.NONE;

		public bool isOpen = true;

		protected bool firstTime = true;

		[AlignWithRay(8396801)]
		public EasyDecal topGraffiti;

		public EasyDecal bottomGraffiti;

		public Material emptyGraffitiMaterial;

		public Crew topCrew;

		public Crew bottomCrew;

		public AttachType attachedTo;

		public bool reportAsCrime = true;

		public GameObject marker;

		public Transform extraSlotsContainer;

		public PlayerType notAllowedToPaint = PlayerType.NONE;

		public GraffitiSize size;

		public UnityEvent OnFinish;

		public bool isMarkerActive;

		public bool deactivateDuringEncounters;

		public Player lastPlayerToPaint;

		public GameObject dynamicRepPickup;

		public Renderer[] decalTopPlaneAlternativeRenderer;

		public Renderer[] decalBottomPlaneAlternativeRenderer;

		internal GraffitiArt topGraffitiArt;

		internal GraffitiArt bottomGraffitiArt;

		internal bool allowRedo = true;

		internal bool isUsingDecal = true;

		internal bool saveDataInitialized;

		internal Transform myTransform;

		internal Player[] extraSlots;

		internal GraffitiState state = GraffitiState.NONE;

		internal bool inGraffitiMode;

		internal bool playerIsAI;

		internal const int DEFAULT_RENDER_QUEUE = 3000;

		internal int swirlControlHash = Shader.PropertyToID("_SwirlControl");

		private int repToSpawn;

		private SaveManager saveManager;

		private StageManager stageManager;

		internal GraffitiArtInfo GraffitiArtInfo => graffitiArtInfo;

		public override void Awake()
		{
			base.Awake();
			swirlControlHash = Shader.PropertyToID("_SwirlControl");
			isUsingDecal = decalTopPlaneAlternativeRenderer.Length == 0 || decalTopPlaneAlternativeRenderer[0] == null;
			myTransform = base.transform;
		}

		internal void Start()
		{
			saveManager = Core.Instance.SaveManager;
		}

		internal void OnEnable()
		{
			StartCoroutine(ToggleMarkerAfterPotentialMaterialChange());
		}

		protected IEnumerator ToggleMarkerAfterPotentialMaterialChange()
		{
			while (!saveDataInitialized)
			{
				yield return null;
			}
			yield return null;
			ToggleMarker(!ClaimedByPlayableCrew());
		}

		internal static void SetMaterialVariables(Material material, int swirlControlHash, bool markerActive, int renderQueue)
		{
			material.SetFloat(swirlControlHash, Convert.ToInt32(markerActive));
			material.renderQueue = renderQueue;
		}

		protected void ToggleMarker(bool showMarker)
		{
			isMarkerActive = showMarker;
			marker.SetActive(showMarker);
			if (isUsingDecal)
			{
				Material material = topGraffiti.DecalRenderer.material;
				Material material2 = bottomGraffiti.DecalRenderer.material;
				SetMaterialVariables(material, swirlControlHash, isMarkerActive, 3001);
				SetMaterialVariables(material2, swirlControlHash, markerActive: false, 3000);
				return;
			}
			if (decalTopPlaneAlternativeRenderer.Length != 0 && decalTopPlaneAlternativeRenderer[0] != null)
			{
				SetMaterialVariables(decalTopPlaneAlternativeRenderer[0].material, swirlControlHash, isMarkerActive, 3001);
			}
			if (decalBottomPlaneAlternativeRenderer.Length != 0 && decalBottomPlaneAlternativeRenderer[0] != null)
			{
				SetMaterialVariables(decalBottomPlaneAlternativeRenderer[0].material, swirlControlHash, markerActive: false, 3000);
			}
		}

		public override void OnDestroy()
		{
			saveDataInitialized = false;
			base.OnDestroy();
		}

		protected override void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			base.OnRegisterToSceneObjectsRegister(sceneObjectsRegister);
			sceneObjectsRegister.RegisterGraffitiSpot(this);
		}

		public void InitSceneObject()
		{
		}

		internal int StartGraffitiMode(Player byPlayer)
		{
			int result = 0;
			if (!byPlayer.IsBusyWithSequence())
			{
				SetState(GraffitiState.PAINTING);
				playerIsAI = byPlayer.isAI;
				if (playerIsAI)
				{
					result = ClaimExtraSlot(byPlayer);
				}
			}
			return result;
		}

		public virtual void SetState(GraffitiState setState)
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
				ToggleMarker(showMarker: false);
				break;
			case GraffitiState.PAINTING_COMPLETE:
				StartCoroutine(ToggleMarkerAfterPotentialMaterialChange());
				break;
			case GraffitiState.FINISHED:
				inGraffitiMode = false;
				if (ClaimedByPlayableCrew())
				{
					if (Core.Instance.Platform.HasAchievementsSupport)
					{
						stageManager.CheckUniqueGraffitiAchievement();
					}
					if (firstTime)
					{
						OnFinish.Invoke();
					}
					firstTime = false;
				}
				WriteToData();
				ToggleMarker(!ClaimedByPlayableCrew());
				break;
			}
		}

		public virtual void GiveRep(bool noRepPickup, bool oldBottomWasClaimedByPlayableCrew)
		{
			repToSpawn = 0;
			if (bottomCrew != topCrew && ClaimedByPlayableCrew() && !oldBottomWasClaimedByPlayableCrew)
			{
				int rEP = GetREP(size);
				saveManager.CurrentSaveSlot.GetCurrentStageProgress().AddRep(rEP);
				repToSpawn = rEP;
				saveManager.PlayerStats.graffitiStats.Notify(GraffitiEvent.GRAFFITISPOT_CLAIMED, this);
				if (noRepPickup)
				{
					WorldHandler.instance.GetCurrentPlayer().ShowAddRep(rEP);
				}
			}
		}

		internal void SpawnRep()
		{
			for (int i = 0; i < repToSpawn; i++)
			{
				UnityEngine.Object.Instantiate(dynamicRepPickup, myTransform.position - myTransform.forward * -1f, Quaternion.LookRotation(myTransform.forward, myTransform.up));
			}
		}

		internal static int GetREP(GraffitiSize size)
		{
			int result = 2;
			if (size == GraffitiSize.M)
			{
				result = 4;
			}
			if (size == GraffitiSize.L)
			{
				result = 6;
			}
			if (size == GraffitiSize.XL)
			{
				result = 8;
			}
			return result;
		}

		internal void Paint(Crew newCrew, GraffitiArt graffitiArt, Player byPlayer = null)
		{
			bottomCrew = topCrew;
			if (!GraffitiEmpty(topCrew))
			{
				bottomGraffitiArt = topGraffitiArt;
			}
			topCrew = newCrew;
			if (graffitiArt == null)
			{
				graffitiArt = graffitiArtInfo.FindCrewArt(newCrew, size);
			}
			topGraffitiArt = graffitiArt;
			Material bottomGraffitiMaterial = null;
			Material graffitiMaterial = graffitiArt.graffitiMaterial;
			if (isUsingDecal)
			{
				bottomGraffitiMaterial = topGraffiti.DecalRenderer.sharedMaterial;
			}
			else if (decalTopPlaneAlternativeRenderer.Length != 0 && decalTopPlaneAlternativeRenderer[0] != null)
			{
				bottomGraffitiMaterial = decalTopPlaneAlternativeRenderer[0].sharedMaterial;
			}
			ReleaseExtraSlotPlayers(byPlayer);
			PaintChanged(graffitiMaterial, bottomGraffitiMaterial);
			SetState(GraffitiState.PAINTING_COMPLETE);
			allowRedo = false;
		}

		internal void ClearPaint()
		{
			bottomCrew = Crew.NONE;
			topCrew = Crew.NONE;
			PaintChanged(null, null);
		}

		protected void SetPaintOnRenderers(Renderer[] topRenderers, Renderer[] bottomRenderers, Material topGraffitiMaterial, Material bottomGraffitiMaterial, Crew topCrew, Crew bottomCrew)
		{
			if (GraffitiEmpty(topCrew))
			{
				for (int i = 0; i < topRenderers.Length; i++)
				{
					if (topRenderers[i] != null)
					{
						topRenderers[i].enabled = false;
					}
				}
			}
			else
			{
				Material sharedMaterial = null;
				if (topRenderers.Length != 0 && topRenderers[0] != null)
				{
					topRenderers[0].material = topGraffitiMaterial;
					SetGraffitiMaterial(topRenderers[0].material, 3001, swirlControlHash, topCrew != Crew.PLAYERS);
					topRenderers[0].enabled = true;
					sharedMaterial = topRenderers[0].material;
				}
				for (int j = 1; j < topRenderers.Length; j++)
				{
					if (topRenderers[j] != null)
					{
						topRenderers[j].sharedMaterial = sharedMaterial;
						topRenderers[j].enabled = true;
					}
				}
			}
			if (GraffitiEmpty(bottomCrew))
			{
				for (int k = 0; k < bottomRenderers.Length; k++)
				{
					if (bottomRenderers[k] != null)
					{
						bottomRenderers[k].enabled = false;
					}
				}
				return;
			}
			Material sharedMaterial2 = null;
			if (bottomRenderers.Length != 0 && bottomRenderers[0] != null)
			{
				UnityEngine.Object.Destroy(bottomRenderers[0].material);
				bottomRenderers[0].material = bottomGraffitiMaterial;
				SetGraffitiMaterial(bottomRenderers[0].material, 3000, swirlControlHash, setMarkerActive: false);
				bottomRenderers[0].enabled = true;
				sharedMaterial2 = bottomRenderers[0].material;
			}
			for (int l = 1; l < bottomRenderers.Length; l++)
			{
				if (bottomRenderers[l] != null)
				{
					bottomRenderers[l].sharedMaterial = sharedMaterial2;
					bottomRenderers[l].enabled = true;
				}
			}
		}

		internal static void SetGraffitiMaterial(Material material, int renderQueue, int swirlControlHash, bool setMarkerActive)
		{
			material.renderQueue = renderQueue;
			material.SetFloat(swirlControlHash, Convert.ToInt32(setMarkerActive));
		}

		internal void SetTopGraffitiDecal(Material newGraffitiMaterial)
		{
			topGraffiti.DecalMaterial = newGraffitiMaterial;
			SetGraffitiMaterial(topGraffiti.DecalRenderer.material, 3001, swirlControlHash, topCrew != Crew.PLAYERS);
		}

		internal void SetBottomGraffitiDecal(Material newGraffitiMaterial)
		{
			UnityEngine.Object.Destroy(bottomGraffiti.DecalRenderer.material);
			bottomGraffiti.DecalMaterial = newGraffitiMaterial;
			SetGraffitiMaterial(bottomGraffiti.DecalRenderer.material, 3000, swirlControlHash, setMarkerActive: false);
		}

		protected void PaintChanged(Material topGraffitiMaterial, Material bottomGraffitiMaterial)
		{
			if (isUsingDecal)
			{
				Material topGraffitiDecal = topGraffitiMaterial;
				if (GraffitiEmpty(topCrew))
				{
					topGraffitiDecal = emptyGraffitiMaterial;
				}
				SetTopGraffitiDecal(topGraffitiDecal);
				if (GraffitiEmpty(bottomCrew))
				{
					bottomGraffiti.DecalRenderer.enabled = false;
					return;
				}
				bottomGraffiti.DecalRenderer.enabled = true;
				SetBottomGraffitiDecal(bottomGraffitiMaterial);
			}
			else
			{
				SetPaintOnRenderers(decalTopPlaneAlternativeRenderer, decalBottomPlaneAlternativeRenderer, topGraffitiMaterial, bottomGraffitiMaterial, topCrew, bottomCrew);
			}
		}

		public void MakeOpen()
		{
			isOpen = true;
			base.gameObject.SetActive(value: true);
			WriteToData();
		}

		public void ResetFirstTime()
		{
			firstTime = true;
			WriteToData();
		}

		public void SetToPlayerGraffiti()
		{
			isOpen = true;
			base.gameObject.SetActive(value: true);
			graffitiArtInfo = WorldHandler.instance.graffitiArtInfo;
			topCrew = Crew.PLAYERS;
			bottomCrew = Crew.NONE;
			firstTime = false;
			topGraffitiArt = graffitiArtInfo.FindCrewArt(Crew.PLAYERS, size);
			bottomGraffitiArt = graffitiArtInfo.FindCrewArt(Crew.NONE, size);
			Material topGraffitiMaterial = null;
			Material bottomGraffitiMaterial = null;
			if (topGraffitiArt != null)
			{
				topGraffitiMaterial = topGraffitiArt.graffitiMaterial;
			}
			if (bottomGraffitiArt != null)
			{
				bottomGraffitiMaterial = bottomGraffitiArt.graffitiMaterial;
			}
			PaintChanged(topGraffitiMaterial, bottomGraffitiMaterial);
			WriteToData();
		}

		internal void InitializeGraffiti(GraffitiArt topGraffitiArt, GraffitiArt bottomGraffitiArt)
		{
			graffitiArtInfo = WorldHandler.instance.graffitiArtInfo;
			stageManager = Core.Instance.BaseModule.StageManager;
			Material topGraffitiMaterial = null;
			Material bottomGraffitiMaterial = null;
			if (topGraffitiArt != null)
			{
				topGraffitiMaterial = topGraffitiArt.graffitiMaterial;
			}
			if (bottomGraffitiArt != null)
			{
				bottomGraffitiMaterial = bottomGraffitiArt.graffitiMaterial;
			}
			PaintChanged(topGraffitiMaterial, bottomGraffitiMaterial);
			base.gameObject.SetActive(isOpen);
			if ((bool)extraSlotsContainer)
			{
				extraSlots = new Player[extraSlotsContainer.childCount];
			}
			saveDataInitialized = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool GraffitiEmpty(Crew crewVariableToCheck)
		{
			return crewVariableToCheck == Crew.NONE;
		}

		public override void SetDefaultData(StageProgress stageProgress = null)
		{
			graffitiArtInfo = WorldHandler.instance.graffitiArtInfo;
			topGraffitiArt = null;
			bottomGraffitiArt = null;
			if (graffitiArtInfo != null)
			{
				if (!GraffitiEmpty(topCrew))
				{
					topGraffitiArt = graffitiArtInfo.FindCrewArt(topCrew, size);
				}
				if (!GraffitiEmpty(bottomCrew))
				{
					bottomGraffitiArt = graffitiArtInfo.FindCrewArt(bottomCrew, size);
				}
			}
			if (stageProgress != null)
			{
				progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.GRAFFITI_SPOT_PROGRESS);
				GraffitiSpotProgress obj = (GraffitiSpotProgress)progressableData;
				obj.isOpen = isOpen;
				obj.topCrew = topCrew;
				obj.bottomCrew = bottomCrew;
				obj.topGraffitiArt = ((topGraffitiArt != null) ? topGraffitiArt.title : string.Empty);
				obj.bottomGraffitiArt = ((bottomGraffitiArt != null) ? bottomGraffitiArt.title : string.Empty);
			}
			InitializeGraffiti(topGraffitiArt, bottomGraffitiArt);
		}

		public override void ReadFromData()
		{
			graffitiArtInfo = WorldHandler.instance.graffitiArtInfo;
			GraffitiSpotProgress graffitiSpotProgress = (GraffitiSpotProgress)progressableData;
			isOpen = graffitiSpotProgress.isOpen;
			topCrew = graffitiSpotProgress.topCrew;
			bottomCrew = graffitiSpotProgress.bottomCrew;
			firstTime = graffitiSpotProgress.firstTime;
			topGraffitiArt = graffitiArtInfo.FindByTitle(graffitiSpotProgress.topGraffitiArt);
			bottomGraffitiArt = graffitiArtInfo.FindByTitle(graffitiSpotProgress.bottomGraffitiArt);
			InitializeGraffiti(topGraffitiArt, bottomGraffitiArt);
		}

		public override void WriteToData()
		{
			GraffitiSpotProgress graffitiSpotProgress = (GraffitiSpotProgress)progressableData;
			graffitiSpotProgress.isOpen = isOpen;
			if (topGraffitiArt == null)
			{
				graffitiSpotProgress.topGraffitiArt = string.Empty;
			}
			else
			{
				graffitiSpotProgress.topGraffitiArt = topGraffitiArt.title;
			}
			if (bottomGraffitiArt == null)
			{
				graffitiSpotProgress.bottomGraffitiArt = string.Empty;
			}
			else
			{
				graffitiSpotProgress.bottomGraffitiArt = bottomGraffitiArt.title;
			}
			graffitiSpotProgress.topCrew = topCrew;
			graffitiSpotProgress.bottomCrew = bottomCrew;
			graffitiSpotProgress.firstTime = firstTime;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public bool IsProgressReady()
		{
			return (GraffitiSpotProgress)progressableData != null;
		}

		internal bool HasExtraSlot()
		{
			bool result = false;
			if (extraSlots != null)
			{
				for (int i = 0; i < extraSlots.Length; i++)
				{
					if (extraSlots[i] == null)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		internal int ClaimExtraSlot(Player player)
		{
			int result = -1;
			if (extraSlots != null)
			{
				for (int i = 0; i < extraSlots.Length; i++)
				{
					if (extraSlots[i] == null)
					{
						extraSlots[i] = player;
						result = i;
						break;
					}
				}
			}
			return result;
		}

		public IEnumerator WaitForGraffitiProgressBeforeResettinAndOpening()
		{
			while (!IsProgressReady())
			{
				yield return null;
			}
			MakeOpen();
			ClearPaint();
			ResetFirstTime();
			SetState(GraffitiState.NONE);
		}

		internal void FreeExtraSlot(int slot)
		{
			if (slot >= 0)
			{
				extraSlots[slot] = null;
			}
		}

		internal void ReleaseExtraSlotPlayers(Player except = null)
		{
			if (extraSlots == null)
			{
				return;
			}
			for (int i = 0; i < extraSlots.Length; i++)
			{
				if (extraSlots[i] != null && extraSlots[i] != except)
				{
					extraSlots[i].StopCurrentAbility();
				}
			}
		}

		internal void ManualAnimateExtraSlotAIPlayers()
		{
			if (extraSlots == null)
			{
				return;
			}
			for (int i = 0; i < extraSlots.Length; i++)
			{
				if (extraSlots[i] != null && extraSlots[i].isAI)
				{
					extraSlots[i].pseudoGraffitiAbility.Animate(justKeepGoing: true);
				}
			}
		}

		internal void ManualLetExtraSlotAIPlayersFinish()
		{
			if (extraSlots == null)
			{
				return;
			}
			for (int i = 0; i < extraSlots.Length; i++)
			{
				if (extraSlots[i] != null && extraSlots[i].isAI)
				{
					extraSlots[i].pseudoGraffitiAbility.CompletePainting();
				}
			}
		}

		internal bool Claimed()
		{
			return GraffitiEmpty(topCrew);
		}

		internal bool ClaimedByPlayableCrew()
		{
			return ClaimedByPlayableCrew(onlyPlayer: false);
		}

		internal bool ClaimedByPlayableCrew(bool onlyPlayer)
		{
			bool flag = false;
			if (!onlyPlayer)
			{
				return topCrew == Crew.PLAYERS || topCrew == Crew.ROGUE;
			}
			return topCrew == Crew.PLAYERS;
		}

		public virtual bool CanDoGraffiti(Player byPlayer)
		{
			if (inGraffitiMode || ((notAllowedToPaint == PlayerType.RACER_AI || notAllowedToPaint == PlayerType.TEMP_RACER_AI) && byPlayer.isAI) || (notAllowedToPaint == PlayerType.HUMAN && !byPlayer.isAI) || byPlayer.spraycanLocked)
			{
				return false;
			}
			if (byPlayer.crew == topCrew && !GraffitiEmpty(topCrew))
			{
				if (!byPlayer.isAI && allowRedo)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		internal bool IsFinished()
		{
			return state == GraffitiState.FINISHED;
		}

		internal Vector3 GetGraffitiSpotPlayerPos()
		{
			return base.transform.position + base.transform.forward + base.transform.up * -1f;
		}

		public virtual bool CanBreakAwayFromByEnemies()
		{
			return true;
		}

		public void SetLastPlayer(Player player)
		{
			lastPlayerToPaint = player;
		}
	}
}
