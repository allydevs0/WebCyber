using Reptile.Phone;
using UnityEngine;

namespace Reptile
{
	public class Pickup : MonoBehaviour
	{
		public enum PickUpType
		{
			BOOST_CHARGE = 0,
			BOOST_BIG_CHARGE = 1,
			MUSIC_UNLOCKABLE = 3,
			GRAFFITI_UNLOCKABLE = 4,
			OUTFIT_UNLOCKABLE = 5,
			FORTUNE_BOOST = 6,
			BRIBE = 7,
			DYNAMIC_REP = 8,
			REP = 9,
			MAP = 10,
			MOVESTYLE_SKIN_UNLOCKABLE = 11
		}

		public static float boostChargeValue = 1.5f;

		public static float boostBigChargeValue = 10f;

		public float respawnTime = 60f;

		public PickUpType pickupType;

		public AUnlockable unlock;

		public GameObject pickupObject;

		internal bool pickedUp;

		internal float timer;

		protected SceneObjectsRegister sceneObjectsRegister;

		protected AudioManager audioManager;

		protected WantedManager wantedManager;

		protected SaveManager saveManager;

		protected Player player;

		private Animation anim;

		private AUser user;

		[SerializeField]
		private GameObject pickupEffect;

		public virtual void Awake()
		{
			anim = GetComponentInChildren<Animation>();
			Core.OnCoreUpdatePaused += OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused += OnCoreUpdateUnPaused;
			RegisterToSceneObjectEvent();
		}

		protected void RegisterToSceneObjectEvent()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		private void OnCoreUpdatePaused()
		{
			if ((bool)anim && (bool)anim.clip)
			{
				anim[anim.clip.name].speed = 0f;
			}
		}

		private void OnCoreUpdateUnPaused()
		{
			if ((bool)anim && (bool)anim.clip)
			{
				anim[anim.clip.name].speed = 1f;
			}
		}

		public virtual void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			Core.OnCoreUpdatePaused -= OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused -= OnCoreUpdateUnPaused;
		}

		protected void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterPickup(this);
			SetReferences();
			if (pickupType == PickUpType.MAP && Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().mapFound)
			{
				SetPickupActive(set: false);
			}
		}

		protected void SetReferences()
		{
			wantedManager = WantedManager.instance;
			WorldHandler instance = WorldHandler.instance;
			sceneObjectsRegister = instance.SceneObjectsRegister;
			audioManager = Core.Instance.AudioManager;
			saveManager = Core.Instance.SaveManager;
			user = Core.Instance.Platform.User;
		}

		public virtual void UpdatePickup()
		{
			if (!pickedUp)
			{
				return;
			}
			if (pickupType == PickUpType.BRIBE)
			{
				if (wantedManager.WantedStars > 0)
				{
					SetPickupActive(set: true);
				}
			}
			else if (pickupType != PickUpType.MAP)
			{
				timer -= Core.dt;
				if (timer < 0f)
				{
					SetPickupActive(set: true);
				}
			}
		}

		public virtual bool CanBePickedUpBy(Player p)
		{
			if (pickupType == PickUpType.BRIBE)
			{
				return wantedManager.WantedStars > 0;
			}
			return true;
		}

		public virtual void PickupPickup(Player p)
		{
			player = p;
			SetPickupActive(set: false);
			PickedUpVisualization(p);
			ApplyPickup(p);
		}

		public virtual void SetPickupActive(bool set)
		{
			pickedUp = !set;
			pickupObject.SetActive(set);
			if (!pickupObject.activeSelf)
			{
				timer = respawnTime;
			}
		}

		public void ApplyPickup(Player forPlayer)
		{
			ApplyPickupType(saveManager, audioManager, user, forPlayer, unlock, pickupType, base.transform.position, wantedManager);
		}

		public static void ApplyPickupType(SaveManager saveManager, AudioManager audioManager, AUser user, Player pickedUpByPlayer, AUnlockable unlockable, PickUpType pickupType, Vector3 position, WantedManager wantedManager = null)
		{
			switch (pickupType)
			{
			case PickUpType.BOOST_CHARGE:
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.pickupCharge);
				pickedUpByPlayer.AddBoostCharge(boostChargeValue);
				break;
			case PickUpType.BOOST_BIG_CHARGE:
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.pickupCharge);
				pickedUpByPlayer.AddBoostCharge(boostBigChargeValue);
				break;
			case PickUpType.MUSIC_UNLOCKABLE:
			{
				UnlockPickupUnlockable(saveManager, user, unlockable);
				MusicTrack musicTrack = unlockable as MusicTrack;
				pickedUpByPlayer.PickupVisualization(position, PickUpType.MUSIC_UNLOCKABLE);
				pickedUpByPlayer.phone.GetAppInstance<AppMusicPlayer>().PushNotification(musicTrack.Title, musicTrack);
				IMusicPlayer gameMusicPlayer = pickedUpByPlayer.phone.GetAppInstance<AppMusicPlayer>().GameMusicPlayer;
				gameMusicPlayer.RefreshMusicQueueForStage(trackToPlay: gameMusicPlayer.GetMusicTrack(gameMusicPlayer.CurrentTrackIndex), chapterMusic: Core.Instance.chapterMusic, stage: Core.Instance.BaseModule.CurrentStage);
				pickedUpByPlayer.phone.GetAppInstance<AppMusicPlayer>().OnAppRefresh();
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.unlockNotification);
				break;
			}
			case PickUpType.GRAFFITI_UNLOCKABLE:
			{
				UnlockPickupUnlockable(saveManager, user, unlockable);
				GraffitiAppEntry graffitiAppEntry = unlockable as GraffitiAppEntry;
				pickedUpByPlayer.PickupVisualization(position, PickUpType.GRAFFITI_UNLOCKABLE);
				pickedUpByPlayer.phone.GetAppInstance<AppGraffiti>().PushNotification(graffitiAppEntry.Title, graffitiAppEntry);
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.unlockNotification);
				break;
			}
			case PickUpType.OUTFIT_UNLOCKABLE:
			{
				UnlockPickupUnlockable(saveManager, user, unlockable);
				OutfitUnlockable outfitUnlockable = unlockable as OutfitUnlockable;
				Characters character = outfitUnlockable.character;
				CharacterProgress characterProgress = saveManager.CurrentSaveSlot.GetCharacterProgress(character);
				UIManager uIManager = Core.Instance.UIManager;
				string text = (characterProgress.unlocked ? Core.Instance.Localizer.GetCharacterName(character) : "???");
				string skinText2 = Core.Instance.Localizer.GetSkinText(outfitUnlockable.outfitName);
				if (Core.Instance.Localizer.Language == SystemLanguage.Japanese)
				{
					uIManager.ShowNotification("OUTFIT_NOTIFICATION", text, skinText2);
				}
				else
				{
					uIManager.ShowNotification("OUTFIT_NOTIFICATION", skinText2, text);
				}
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.unlockNotification);
				break;
			}
			case PickUpType.MOVESTYLE_SKIN_UNLOCKABLE:
			{
				UnlockPickupUnlockable(saveManager, user, unlockable);
				MoveStyleSkin moveStyleSkin = unlockable as MoveStyleSkin;
				string skinText = Core.Instance.Localizer.GetSkinText(moveStyleSkin.Title);
				Core.Instance.UIManager.ShowNotification("MOVESTYLE_NOTIFICATION", skinText, Core.Instance.Localizer.GetStageName(Stage.hideout));
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.unlockNotification);
				break;
			}
			case PickUpType.BRIBE:
				wantedManager.StopPlayerWantedStatus(showMessage: false);
				break;
			case PickUpType.DYNAMIC_REP:
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.repPickup);
				pickedUpByPlayer.ShowAddRep(1);
				break;
			case PickUpType.MAP:
				saveManager.CurrentSaveSlot.GetCurrentStageProgress().mapFound = true;
				pickedUpByPlayer.phone.GetAppInstance<AppHomeScreen>().PushNotification();
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.musicCollectablePickup);
				break;
			case (PickUpType)2:
			case PickUpType.FORTUNE_BOOST:
			case PickUpType.REP:
				break;
			}
		}

		private static void UnlockPickupUnlockable(SaveManager saveManager, AUser user, AUnlockable unlockable)
		{
			user.GetUnlockableSaveDataFor(unlockable).IsUnlocked = true;
			saveManager.SaveCurrentSaveSlot();
		}

		protected void PickedUpVisualization(Player player)
		{
			player.PickupVisualization(base.transform.position, pickupType);
			Object.Instantiate(pickupEffect, base.transform.position, Quaternion.identity);
		}
	}
}
