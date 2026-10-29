using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public static class Utility
	{
		public static bool IsActiveSceneMainMenuScene
		{
			get
			{
				Scene activeScene = SceneManager.GetActiveScene();
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Core.Instance.sceneLibrary.mainMenuScenePath);
				if (activeScene.name.Equals(fileNameWithoutExtension))
				{
					return true;
				}
				return false;
			}
		}

		public static bool IsActiveSceneCore
		{
			get
			{
				Scene activeScene = SceneManager.GetActiveScene();
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Core.Instance.sceneLibrary.coreScenePath);
				if (activeScene.name.Equals(fileNameWithoutExtension))
				{
					return true;
				}
				return false;
			}
		}

		public static bool IsActiveSceneCreditsScene
		{
			get
			{
				Scene activeScene = SceneManager.GetActiveScene();
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Core.Instance.sceneLibrary.creditsScenePath);
				if (activeScene.name.Equals(fileNameWithoutExtension))
				{
					return true;
				}
				return false;
			}
		}

		public static bool IsActiveSceneBootstrap
		{
			get
			{
				Scene activeScene = SceneManager.GetActiveScene();
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Core.Instance.sceneLibrary.bootstrapScenePath);
				if (activeScene.name.Equals(fileNameWithoutExtension))
				{
					return true;
				}
				return false;
			}
		}

		public static AchievementType AchievementTypeNameToAchievementType(string achievementTypeName)
		{
			AchievementType result = AchievementType.NONE;
			switch (achievementTypeName)
			{
			case "NULL":
				result = AchievementType.NULL;
				break;
			case "BasketballAchievement":
				result = AchievementType.BasketballAchievement;
				break;
			case "InTheFlow":
				result = AchievementType.InTheFlow;
				break;
			case "IronLegs":
				result = AchievementType.IronLegs;
				break;
			case "Nice":
				result = AchievementType.Nice;
				break;
			case "NowGoOutside":
				result = AchievementType.NowGoOutside;
				break;
			case "FlyingRatsBegone":
				result = AchievementType.FlyingRatsBegone;
				break;
			case "StateOfTheArt":
				result = AchievementType.StateOfTheArt;
				break;
			case "PhotoGeneric":
				result = AchievementType.PhotoGeneric;
				break;
			case "FunkStar":
				result = AchievementType.FunkStar;
				break;
			case "VersumHillTricked":
				result = AchievementType.VersumHillTricked;
				break;
			case "VersumHillBombed":
				result = AchievementType.VersumHillBombed;
				break;
			case "HideoutTricked":
				result = AchievementType.HideoutTricked;
				break;
			case "HideoutBombed":
				result = AchievementType.HideoutBombed;
				break;
			case "MilleniumMallTricked":
				result = AchievementType.MilleniumMallTricked;
				break;
			case "MilleniumMallBombed":
				result = AchievementType.MilleniumMallBombed;
				break;
			case "MataanTricked":
				result = AchievementType.MataanTricked;
				break;
			case "MataanBombed":
				result = AchievementType.MataanBombed;
				break;
			case "PyramidIslandTricked":
				result = AchievementType.PyramidIslandTricked;
				break;
			case "PyramidIslandBombed":
				result = AchievementType.PyramidIslandBombed;
				break;
			case "MilleniumSquareTricked":
				result = AchievementType.MilleniumSquareTricked;
				break;
			case "MilleniumSquareBombed":
				result = AchievementType.MilleniumSquareBombed;
				break;
			case "BrinkTerminalTricked":
				result = AchievementType.BrinkTerminalTricked;
				break;
			case "BrinkTerminalBombed":
				result = AchievementType.BrinkTerminalBombed;
				break;
			}
			return result;
		}

		public static AudioChannelID AudioChannelNameToAudioChannelID(string audioChannelName)
		{
			AudioChannelID result = AudioChannelID.NONE;
			switch (audioChannelName)
			{
			case "Master":
				result = AudioChannelID.Master;
				break;
			case "Sfx":
				result = AudioChannelID.Sfx;
				break;
			case "UI":
				result = AudioChannelID.UI;
				break;
			case "Gameplay":
				result = AudioChannelID.Gameplay;
				break;
			case "Voices":
				result = AudioChannelID.Voices;
				break;
			case "Ambience":
				result = AudioChannelID.Ambience;
				break;
			case "Music":
				result = AudioChannelID.Music;
				break;
			}
			return result;
		}

		public static AudioClipID AudioClipNameToAudioClipID(string audioClipName)
		{
			AudioClipID result = AudioClipID.NONE;
			switch (audioClipName)
			{
			case "Downhilll_Ambience":
				result = AudioClipID.Downhilll_Ambience;
				break;
			case "Dream_Ambience":
				result = AudioClipID.Dream_Ambience;
				break;
			case "Escalator_Ambience":
				result = AudioClipID.Escalator_Ambience;
				break;
			case "Mall_Ambience":
				result = AudioClipID.Mall_Ambience;
				break;
			case "Osaka_Ambience":
				result = AudioClipID.Osaka_Ambience;
				break;
			case "Prelude_Ambience_Cell":
				result = AudioClipID.Prelude_Ambience_Cell;
				break;
			case "Prelude_Ambience_Outside":
				result = AudioClipID.Prelude_Ambience_Outside;
				break;
			case "Prelude_Ambience_StationInterior":
				result = AudioClipID.Prelude_Ambience_StationInterior;
				break;
			case "Pyramid_Ambience":
				result = AudioClipID.Pyramid_Ambience;
				break;
			case "Tower_Port_Ambience":
				result = AudioClipID.Tower_Port_Ambience;
				break;
			case "Waterfall_Ambience":
				result = AudioClipID.Waterfall_Ambience;
				break;
			case "Wind_Ambience":
				result = AudioClipID.Wind_Ambience;
				break;
			case "cop_jetpack_loop":
				result = AudioClipID.cop_jetpack_loop;
				break;
			case "cop_jetpack_start":
				result = AudioClipID.cop_jetpack_start;
				break;
			case "deployCanister":
				result = AudioClipID.deployCanister;
				break;
			case "policeAlarm":
				result = AudioClipID.policeAlarm;
				break;
			case "policeWallDown":
				result = AudioClipID.policeWallDown;
				break;
			case "policeWallUp":
				result = AudioClipID.policeWallUp;
				break;
			case "recordWoosh":
				result = AudioClipID.recordWoosh;
				break;
			case "tankwalkerFootstep01":
				result = AudioClipID.tankwalkerFootstep01;
				break;
			case "BatonAttackHit":
				result = AudioClipID.BatonAttackHit;
				break;
			case "BodyDrop":
				result = AudioClipID.BodyDrop;
				break;
			case "BulletHit":
				result = AudioClipID.BulletHit;
				break;
			case "BulletRicochet":
				result = AudioClipID.BulletRicochet;
				break;
			case "chainBreak":
				result = AudioClipID.chainBreak;
				break;
			case "ClawAttackHit":
				result = AudioClipID.ClawAttackHit;
				break;
			case "CockpitGetHit":
				result = AudioClipID.CockpitGetHit;
				break;
			case "CopterBlades":
				result = AudioClipID.CopterBlades;
				break;
			case "CopterBladesCrash":
				result = AudioClipID.CopterBladesCrash;
				break;
			case "CopterMachinegunShot":
				result = AudioClipID.CopterMachinegunShot;
				break;
			case "cuffClose":
				result = AudioClipID.cuffClose;
				break;
			case "cuffMissile":
				result = AudioClipID.cuffMissile;
				break;
			case "DJJump":
				result = AudioClipID.DJJump;
				break;
			case "DJLand":
				result = AudioClipID.DJLand;
				break;
			case "DJOpen":
				result = AudioClipID.DJOpen;
				break;
			case "DJVinylCounter":
				result = AudioClipID.DJVinylCounter;
				break;
			case "DJVinylThrow":
				result = AudioClipID.DJVinylThrow;
				break;
			case "DJWalking":
				result = AudioClipID.DJWalking;
				break;
			case "EnemyGetHit":
				result = AudioClipID.EnemyGetHit;
				break;
			case "FemaleDeath":
				result = AudioClipID.FemaleDeath;
				break;
			case "FemaleHurt":
				result = AudioClipID.FemaleHurt;
				break;
			case "HatchOpen":
				result = AudioClipID.HatchOpen;
				break;
			case "MachineryExplosion":
				result = AudioClipID.MachineryExplosion;
				break;
			case "Male2Attack":
				result = AudioClipID.Male2Attack;
				break;
			case "Male2Hurt":
				result = AudioClipID.Male2Hurt;
				break;
			case "MaleAttack":
				result = AudioClipID.MaleAttack;
				break;
			case "MaleDeath":
				result = AudioClipID.MaleDeath;
				break;
			case "MaleHurt":
				result = AudioClipID.MaleHurt;
				break;
			case "MortarExplosion":
				result = AudioClipID.MortarExplosion;
				break;
			case "PistolReload":
				result = AudioClipID.PistolReload;
				break;
			case "PistolShot":
				result = AudioClipID.PistolShot;
				break;
			case "PoliceTubeMoveDown":
				result = AudioClipID.PoliceTubeMoveDown;
				break;
			case "PoliceTubeMoveUp":
				result = AudioClipID.PoliceTubeMoveUp;
				break;
			case "ShieldAttackHit":
				result = AudioClipID.ShieldAttackHit;
				break;
			case "ShieldBlock":
				result = AudioClipID.ShieldBlock;
				break;
			case "SniperReload":
				result = AudioClipID.SniperReload;
				break;
			case "SniperShot":
				result = AudioClipID.SniperShot;
				break;
			case "sniper_lockon":
				result = AudioClipID.sniper_lockon;
				break;
			case "TankWalkerGunAway":
				result = AudioClipID.TankWalkerGunAway;
				break;
			case "TankWalkerGunStart":
				result = AudioClipID.TankWalkerGunStart;
				break;
			case "TankwalkerHeadStab":
				result = AudioClipID.TankwalkerHeadStab;
				break;
			case "TankwalkerMachinegunShot":
				result = AudioClipID.TankwalkerMachinegunShot;
				break;
			case "TankwalkerMortarShot":
				result = AudioClipID.TankwalkerMortarShot;
				break;
			case "TankwalkerPound":
				result = AudioClipID.TankwalkerPound;
				break;
			case "TankwalkerStomp":
				result = AudioClipID.TankwalkerStomp;
				break;
			case "tankwalkerVent":
				result = AudioClipID.tankwalkerVent;
				break;
			case "TankWalkerWindUp":
				result = AudioClipID.TankWalkerWindUp;
				break;
			case "TurretDown":
				result = AudioClipID.TurretDown;
				break;
			case "TurretRaise":
				result = AudioClipID.TurretRaise;
				break;
			case "car_engine":
				result = AudioClipID.car_engine;
				break;
			case "car_hood_impact":
				result = AudioClipID.car_hood_impact;
				break;
			case "vendingMachine":
				result = AudioClipID.vendingMachine;
				break;
			case "vendingMachine_drop":
				result = AudioClipID.vendingMachine_drop;
				break;
			case "vendingMachine_short":
				result = AudioClipID.vendingMachine_short;
				break;
			case "Basketball":
				result = AudioClipID.Basketball;
				break;
			case "Bike":
				result = AudioClipID.Bike;
				break;
			case "BMXGatewayApproved":
				result = AudioClipID.BMXGatewayApproved;
				break;
			case "BMXGatewayChecking":
				result = AudioClipID.BMXGatewayChecking;
				break;
			case "BMXGatewayDenied":
				result = AudioClipID.BMXGatewayDenied;
				break;
			case "BoostGlassDoorBounce":
				result = AudioClipID.BoostGlassDoorBounce;
				break;
			case "Bottle":
				result = AudioClipID.Bottle;
				break;
			case "Can":
				result = AudioClipID.Can;
				break;
			case "CarBrake":
				result = AudioClipID.CarBrake;
				break;
			case "Cardboard":
				result = AudioClipID.Cardboard;
				break;
			case "CarHorn":
				result = AudioClipID.CarHorn;
				break;
			case "CashRegister":
				result = AudioClipID.CashRegister;
				break;
			case "ClothingRack":
				result = AudioClipID.ClothingRack;
				break;
			case "Crate":
				result = AudioClipID.Crate;
				break;
			case "DeckProp":
				result = AudioClipID.DeckProp;
				break;
			case "Dog":
				result = AudioClipID.Dog;
				break;
			case "FireExtinguisher":
				result = AudioClipID.FireExtinguisher;
				break;
			case "Glass":
				result = AudioClipID.Glass;
				break;
			case "Guitar":
				result = AudioClipID.Guitar;
				break;
			case "MarketStand":
				result = AudioClipID.MarketStand;
				break;
			case "MascotHit":
				result = AudioClipID.MascotHit;
				break;
			case "MascotSpin":
				result = AudioClipID.MascotSpin;
				break;
			case "MonorailDrive":
				result = AudioClipID.MonorailDrive;
				break;
			case "Paper":
				result = AudioClipID.Paper;
				break;
			case "PlasticSign":
				result = AudioClipID.PlasticSign;
				break;
			case "PortaPottyClose":
				result = AudioClipID.PortaPottyClose;
				break;
			case "PortaPottyOpen":
				result = AudioClipID.PortaPottyOpen;
				break;
			case "ScrewPoleMoving":
				result = AudioClipID.ScrewPoleMoving;
				break;
			case "ShoppingCart":
				result = AudioClipID.ShoppingCart;
				break;
			case "SkatesProp":
				result = AudioClipID.SkatesProp;
				break;
			case "SprayCan":
				result = AudioClipID.SprayCan;
				break;
			case "StoreShelf":
				result = AudioClipID.StoreShelf;
				break;
			case "Table":
				result = AudioClipID.Table;
				break;
			case "ToiletDoorClose":
				result = AudioClipID.ToiletDoorClose;
				break;
			case "ToiletDoorOpen":
				result = AudioClipID.ToiletDoorOpen;
				break;
			case "Trash":
				result = AudioClipID.Trash;
				break;
			case "TrashBag":
				result = AudioClipID.TrashBag;
				break;
			case "TrashBin":
				result = AudioClipID.TrashBin;
				break;
			case "Trolly":
				result = AudioClipID.Trolly;
				break;
			case "TV":
				result = AudioClipID.TV;
				break;
			case "VentEntrance":
				result = AudioClipID.VentEntrance;
				break;
			case "airdash":
				result = AudioClipID.airdash;
				break;
			case "boostStart":
				result = AudioClipID.boostStart;
				break;
			case "boostStop":
				result = AudioClipID.boostStop;
				break;
			case "jump_special":
				result = AudioClipID.jump_special;
				break;
			case "launcher_woosh":
				result = AudioClipID.launcher_woosh;
				break;
			case "singleBoost":
				result = AudioClipID.singleBoost;
				break;
			case "slideSuperLoop":
				result = AudioClipID.slideSuperLoop;
				break;
			case "specialTrick":
				result = AudioClipID.specialTrick;
				break;
			case "ultraBoostLoop":
				result = AudioClipID.ultraBoostLoop;
				break;
			case "slideDropdownBoost":
				result = AudioClipID.slideDropdownBoost;
				break;
			case "wallrunStart":
				result = AudioClipID.wallrunStart;
				break;
			case "CanShakeLoop":
				result = AudioClipID.CanShakeLoop;
				break;
			case "graffitiComplete":
				result = AudioClipID.graffitiComplete;
				break;
			case "graffitiSlash":
				result = AudioClipID.graffitiSlash;
				break;
			case "Spray":
				result = AudioClipID.Spray;
				break;
			case "sprayCanPop":
				result = AudioClipID.sprayCanPop;
				break;
			case "grindLoop":
				result = AudioClipID.grindLoop;
				break;
			case "railStart":
				result = AudioClipID.railStart;
				break;
			case "railStop":
				result = AudioClipID.railStop;
				break;
			case "HollowRail_Grind_Loop_05":
				result = AudioClipID.HollowRail_Grind_Loop_05;
				break;
			case "HollowRail_Grind_Loop_06":
				result = AudioClipID.HollowRail_Grind_Loop_06;
				break;
			case "OldStartStop":
				result = AudioClipID.OldStartStop;
				break;
			case "cancel":
				result = AudioClipID.cancel;
				break;
			case "confirm":
				result = AudioClipID.confirm;
				break;
			case "musicCollectablePickup":
				result = AudioClipID.musicCollectablePickup;
				break;
			case "pickupCharge":
				result = AudioClipID.pickupCharge;
				break;
			case "repPickup":
				result = AudioClipID.repPickup;
				break;
			case "selectionMove":
				result = AudioClipID.selectionMove;
				break;
			case "TestUISound":
				result = AudioClipID.TestUISound;
				break;
			case "ui_phoneBack":
				result = AudioClipID.ui_phoneBack;
				break;
			case "ui_phoneConfirm":
				result = AudioClipID.ui_phoneConfirm;
				break;
			case "ui_phoneCursor":
				result = AudioClipID.ui_phoneCursor;
				break;
			case "ui_phoneFlipClose":
				result = AudioClipID.ui_phoneFlipClose;
				break;
			case "ui_phoneFlipOpen":
				result = AudioClipID.ui_phoneFlipOpen;
				break;
			case "ui_phoneMessage":
				result = AudioClipID.ui_phoneMessage;
				break;
			case "unlockNotification":
				result = AudioClipID.unlockNotification;
				break;
			case "handplant":
				result = AudioClipID.handplant;
				break;
			case "jump":
				result = AudioClipID.jump;
				break;
			case "BackFlip_0":
				result = AudioClipID.BackFlip_0;
				break;
			case "BulletSpin":
				result = AudioClipID.BulletSpin;
				break;
			case "Cheat720":
				result = AudioClipID.Cheat720;
				break;
			case "footstep":
				result = AudioClipID.footstep;
				break;
			case "groundTrick":
				result = AudioClipID.groundTrick;
				break;
			case "HookKick":
				result = AudioClipID.HookKick;
				break;
			case "land":
				result = AudioClipID.land;
				break;
			case "LegSweep":
				result = AudioClipID.LegSweep;
				break;
			case "ScrewPolePlant":
				result = AudioClipID.ScrewPolePlant;
				break;
			case "slideLoop":
				result = AudioClipID.slideLoop;
				break;
			case "AirTime":
				result = AudioClipID.AirTime;
				break;
			case "wallrunLoop":
				result = AudioClipID.wallrunLoop;
				break;
			case "Backflip":
				result = AudioClipID.Backflip;
				break;
			case "GrindTrick_Crank":
				result = AudioClipID.GrindTrick_Crank;
				break;
			case "GrindTrick_ToothPick":
				result = AudioClipID.GrindTrick_ToothPick;
				break;
			case "GrindTrick_Whiplash":
				result = AudioClipID.GrindTrick_Whiplash;
				break;
			case "groundTrick3":
				result = AudioClipID.groundTrick3;
				break;
			case "groundTrick4":
				result = AudioClipID.groundTrick4;
				break;
			case "pedalLeft":
				result = AudioClipID.pedalLeft;
				break;
			case "PedalLeftShort":
				result = AudioClipID.PedalLeftShort;
				break;
			case "pedalRight":
				result = AudioClipID.pedalRight;
				break;
			case "PedalRightShort":
				result = AudioClipID.PedalRightShort;
				break;
			case "SeatGrab":
				result = AudioClipID.SeatGrab;
				break;
			case "TailWhip":
				result = AudioClipID.TailWhip;
				break;
			case "moveLoop":
				result = AudioClipID.moveLoop;
				break;
			case "backside360varial":
				result = AudioClipID.backside360varial;
				break;
			case "groundTrick2":
				result = AudioClipID.groundTrick2;
				break;
			case "SBRailStart":
				result = AudioClipID.SBRailStart;
				break;
			case "Cork720":
				result = AudioClipID.Cork720;
				break;
			case "DuckWalk":
				result = AudioClipID.DuckWalk;
				break;
			case "GrindTrick3":
				result = AudioClipID.GrindTrick3;
				break;
			case "InLineGrindStart":
				result = AudioClipID.InLineGrindStart;
				break;
			case "SavCessSlide":
				result = AudioClipID.SavCessSlide;
				break;
			case "StaleGrab":
				result = AudioClipID.StaleGrab;
				break;
			case "strideLeft":
				result = AudioClipID.strideLeft;
				break;
			case "strideRight":
				result = AudioClipID.strideRight;
				break;
			case "UnitySpinCessSlide":
				result = AudioClipID.UnitySpinCessSlide;
				break;
			case "FlipPhone_Back":
				result = AudioClipID.FlipPhone_Back;
				break;
			case "Flipphone_close":
				result = AudioClipID.Flipphone_close;
				break;
			case "FlipPhone_Confirm":
				result = AudioClipID.FlipPhone_Confirm;
				break;
			case "Flipphone_open":
				result = AudioClipID.Flipphone_open;
				break;
			case "FlipPhone_RingTone":
				result = AudioClipID.FlipPhone_RingTone;
				break;
			case "FlipPhone_Select":
				result = AudioClipID.FlipPhone_Select;
				break;
			case "beheading":
				result = AudioClipID.beheading;
				break;
			case "ch1s10DJSquadDrop":
				result = AudioClipID.ch1s10DJSquadDrop;
				break;
			case "ch1s10VinylDrop":
				result = AudioClipID.ch1s10VinylDrop;
				break;
			case "ch1s10_DJArrival":
				result = AudioClipID.ch1s10_DJArrival;
				break;
			case "ch1s10_DJGetUp":
				result = AudioClipID.ch1s10_DJGetUp;
				break;
			case "ch1s10_DJOpen":
				result = AudioClipID.ch1s10_DJOpen;
				break;
			case "ch1s10_DJVStop":
				result = AudioClipID.ch1s10_DJVStop;
				break;
			case "ch1s10_EnterDream":
				result = AudioClipID.ch1s10_EnterDream;
				break;
			case "ch1s10_RedRun":
				result = AudioClipID.ch1s10_RedRun;
				break;
			case "ch1s10_SpinAndOpen":
				result = AudioClipID.ch1s10_SpinAndOpen;
				break;
			case "ch1s10_SquadWalk":
				result = AudioClipID.ch1s10_SquadWalk;
				break;
			case "ch1s10_VinylPush":
				result = AudioClipID.ch1s10_VinylPush;
				break;
			case "ch1s10_WindUp":
				result = AudioClipID.ch1s10_WindUp;
				break;
			case "Ch1S1a_CellAmb":
				result = AudioClipID.Ch1S1a_CellAmb;
				break;
			case "Ch1S1a_ChiefFS":
				result = AudioClipID.Ch1S1a_ChiefFS;
				break;
			case "Ch1S1a_Destruction":
				result = AudioClipID.Ch1S1a_Destruction;
				break;
			case "Ch1S1a_OutsideStation":
				result = AudioClipID.Ch1S1a_OutsideStation;
				break;
			case "Ch1S1a_PoliceRunning":
				result = AudioClipID.Ch1S1a_PoliceRunning;
				break;
			case "Ch1S1a_ServerRoom":
				result = AudioClipID.Ch1S1a_ServerRoom;
				break;
			case "Ch1S1a_StationAmb":
				result = AudioClipID.Ch1S1a_StationAmb;
				break;
			case "ch1s1b_DoorOpen":
				result = AudioClipID.ch1s1b_DoorOpen;
				break;
			case "ch1s1b_lockpick":
				result = AudioClipID.ch1s1b_lockpick;
				break;
			case "Ch1s5a_BootUp":
				result = AudioClipID.Ch1s5a_BootUp;
				break;
			case "Ch3S4B_GlassBreak":
				result = AudioClipID.Ch3S4B_GlassBreak;
				break;
			case "Ch3S4B_GlassFalling":
				result = AudioClipID.Ch3S4B_GlassFalling;
				break;
			case "Ch3S4B_HeliEngine":
				result = AudioClipID.Ch3S4B_HeliEngine;
				break;
			case "Ch3S4B_HeliPass":
				result = AudioClipID.Ch3S4B_HeliPass;
				break;
			case "Ch3S4B_SkyAmbience":
				result = AudioClipID.Ch3S4B_SkyAmbience;
				break;
			case "Ch3S4B_TankwalkerDrop":
				result = AudioClipID.Ch3S4B_TankwalkerDrop;
				break;
			case "Ch3S4B_TankwalkerTransform":
				result = AudioClipID.Ch3S4B_TankwalkerTransform;
				break;
			case "Ch3S4B_TWDrop":
				result = AudioClipID.Ch3S4B_TWDrop;
				break;
			case "Ch3S4B_TWVent":
				result = AudioClipID.Ch3S4B_TWVent;
				break;
			case "ch4s6b_DreamEntry":
				result = AudioClipID.ch4s6b_DreamEntry;
				break;
			case "CombatEncounterTrap_WallUp":
				result = AudioClipID.CombatEncounterTrap_WallUp;
				break;
			case "IreneTakeOff_PreludeOutro":
				result = AudioClipID.IreneTakeOff_PreludeOutro;
				break;
			case "siren":
				result = AudioClipID.siren;
				break;
			case "WallDownPyramidTrapSequence":
				result = AudioClipID.WallDownPyramidTrapSequence;
				break;
			case "cat":
				result = AudioClipID.cat;
				break;
			case "dogBark":
				result = AudioClipID.dogBark;
				break;
			case "femaleRobo":
				result = AudioClipID.femaleRobo;
				break;
			case "maleDoeNormaal":
				result = AudioClipID.maleDoeNormaal;
				break;
			case "VoiceDie":
				result = AudioClipID.VoiceDie;
				break;
			case "VoiceDieFall":
				result = AudioClipID.VoiceDieFall;
				break;
			case "VoiceTalk":
				result = AudioClipID.VoiceTalk;
				break;
			case "VoiceBoostTrick":
				result = AudioClipID.VoiceBoostTrick;
				break;
			case "VoiceCombo":
				result = AudioClipID.VoiceCombo;
				break;
			case "VoiceGetHit":
				result = AudioClipID.VoiceGetHit;
				break;
			case "VoiceJump":
				result = AudioClipID.VoiceJump;
				break;
			case "Ch4s2_Tinnitus":
				result = AudioClipID.Ch4s2_Tinnitus;
				break;
			case "Ch4s2_FaceZoom":
				result = AudioClipID.Ch4s2_FaceZoom;
				break;
			case "Ch4s2_Snipershot":
				result = AudioClipID.Ch4s2_Snipershot;
				break;
			case "Ch4s2_SniperArrival":
				result = AudioClipID.Ch4s2_SniperArrival;
				break;
			case "Ch4s2_Fall":
				result = AudioClipID.Ch4s2_Fall;
				break;
			case "Ch4s2_Blood_1":
				result = AudioClipID.Ch4s2_Blood_1;
				break;
			case "Ch4s2_Snipershot2":
				result = AudioClipID.Ch4s2_Snipershot2;
				break;
			case "ch3s4a_CameraWhoosh":
				result = AudioClipID.ch3s4a_CameraWhoosh;
				break;
			case "ch3s4a_BrainWash":
				result = AudioClipID.ch3s4a_BrainWash;
				break;
			case "ch3s4a_BatonFall":
				result = AudioClipID.ch3s4a_BatonFall;
				break;
			case "ch3s4a_GunDrawCock":
				result = AudioClipID.ch3s4a_GunDrawCock;
				break;
			case "ch3s4a_RadioStatic":
				result = AudioClipID.ch3s4a_RadioStatic;
				break;
			case "ch3s4a_FSTubes":
				result = AudioClipID.ch3s4a_FSTubes;
				break;
			case "Ch3s5_EnterDream":
				result = AudioClipID.Ch3s5_EnterDream;
				break;
			case "BasicCopArrivalMall":
				result = AudioClipID.BasicCopArrivalMall;
				break;
			case "TankWalkerDropMall":
				result = AudioClipID.TankWalkerDropMall;
				break;
			case "TankWalkerDropStand":
				result = AudioClipID.TankWalkerDropStand;
				break;
			case "CopterArriveMall_6":
				result = AudioClipID.CopterArriveMall_6;
				break;
			case "BasicCopArrivalDry_2":
				result = AudioClipID.BasicCopArrivalDry_2;
				break;
			case "ShieldCopWalkUp_01":
				result = AudioClipID.ShieldCopWalkUp_01;
				break;
			case "ShieldHitArrive_01":
				result = AudioClipID.ShieldHitArrive_01;
				break;
			case "SnipersArrive":
				result = AudioClipID.SnipersArrive;
				break;
			case "SniperCopReady":
				result = AudioClipID.SniperCopReady;
				break;
			case "SniperStand":
				result = AudioClipID.SniperStand;
				break;
			case "TurretHeat":
				result = AudioClipID.TurretHeat;
				break;
			case "SniperWhoosh":
				result = AudioClipID.SniperWhoosh;
				break;
			case "TurretBeep_01":
				result = AudioClipID.TurretBeep_01;
				break;
			case "TurretBeep_02":
				result = AudioClipID.TurretBeep_02;
				break;
			case "TankWalkerVictory_Fall":
				result = AudioClipID.TankWalkerVictory_Fall;
				break;
			case "TankWalkerVictory_HeadSpin":
				result = AudioClipID.TankWalkerVictory_HeadSpin;
				break;
			case "TankWalkerVictory_FS":
				result = AudioClipID.TankWalkerVictory_FS;
				break;
			case "TankWalkerVictory_FS_2":
				result = AudioClipID.TankWalkerVictory_FS_2;
				break;
			case "TankWalkerVictory_FallBarrel":
				result = AudioClipID.TankWalkerVictory_FallBarrel;
				break;
			case "TankWalkerVictory_Fall2":
				result = AudioClipID.TankWalkerVictory_Fall2;
				break;
			case "TankWalkerVictory_FallBarrel_2":
				result = AudioClipID.TankWalkerVictory_FallBarrel_2;
				break;
			case "ch1s4_SlowMo":
				result = AudioClipID.ch1s4_SlowMo;
				break;
			case "ch1s4Ambience":
				result = AudioClipID.ch1s4Ambience;
				break;
			case "ch1s4_DJZoom":
				result = AudioClipID.ch1s4_DJZoom;
				break;
			case "ch1s4_Decapitate2":
				result = AudioClipID.ch1s4_Decapitate2;
				break;
			case "ch1s4_FauxHand":
				result = AudioClipID.ch1s4_FauxHand;
				break;
			case "ch1s4_HeartBeat":
				result = AudioClipID.ch1s4_HeartBeat;
				break;
			case "ch1s4_DiscMovement":
				result = AudioClipID.ch1s4_DiscMovement;
				break;
			case "ch1s4_DoorRoof":
				result = AudioClipID.ch1s4_DoorRoof;
				break;
			case "ch1s4_DJDisc":
				result = AudioClipID.ch1s4_DJDisc;
				break;
			case "ch1s4_BodyDrop":
				result = AudioClipID.ch1s4_BodyDrop;
				break;
			case "ch1s4_Clap":
				result = AudioClipID.ch1s4_Clap;
				break;
			case "ch1s4_DJMovement":
				result = AudioClipID.ch1s4_DJMovement;
				break;
			case "ch1s4_DJDecapitate3":
				result = AudioClipID.ch1s4_DJDecapitate3;
				break;
			case "MascotUnlock":
				result = AudioClipID.MascotUnlock;
				break;
			case "ch1s4_Ambience2":
				result = AudioClipID.ch1s4_Ambience2;
				break;
			case "TaxiLeave":
				result = AudioClipID.TaxiLeave;
				break;
			case "TaxiArrive":
				result = AudioClipID.TaxiArrive;
				break;
			case "TaxiShowUp":
				result = AudioClipID.TaxiShowUp;
				break;
			case "TaxiArrive_02":
				result = AudioClipID.TaxiArrive_02;
				break;
			case "TaxiCancel":
				result = AudioClipID.TaxiCancel;
				break;
			case "TaxiArrive_03":
				result = AudioClipID.TaxiArrive_03;
				break;
			case "ch5s5_TWGunsOutVerb_2":
				result = AudioClipID.ch5s5_TWGunsOutVerb_2;
				break;
			case "ch5s5_TWZoom":
				result = AudioClipID.ch5s5_TWZoom;
				break;
			case "ch5s5_TWOpen":
				result = AudioClipID.ch5s5_TWOpen;
				break;
			case "ch5s5_TWZoomOut":
				result = AudioClipID.ch5s5_TWZoomOut;
				break;
			case "ch5s5_TWArrive":
				result = AudioClipID.ch5s5_TWArrive;
				break;
			case "ch5s5_TWFinalFS":
				result = AudioClipID.ch5s5_TWFinalFS;
				break;
			case "ch5s5_RedGetUp":
				result = AudioClipID.ch5s5_RedGetUp;
				break;
			case "ch5s5_SolaceMove":
				result = AudioClipID.ch5s5_SolaceMove;
				break;
			case "ch5s5_TWGunsOutVerb_3":
				result = AudioClipID.ch5s5_TWGunsOutVerb_3;
				break;
			case "ch5s5_TWGunsAway":
				result = AudioClipID.ch5s5_TWGunsAway;
				break;
			case "ch5s5_TWGunsOutVerb":
				result = AudioClipID.ch5s5_TWGunsOutVerb;
				break;
			case "ch5s5_DTThrow2":
				result = AudioClipID.ch5s5_DTThrow2;
				break;
			case "ch5s5_OSStomp":
				result = AudioClipID.ch5s5_OSStomp;
				break;
			case "Ch3S0_SRFS":
				result = AudioClipID.Ch3S0_SRFS;
				break;
			case "Ch3S0_VaultOpen":
				result = AudioClipID.Ch3S0_VaultOpen;
				break;
			case "Ch3S0_FS1":
				result = AudioClipID.Ch3S0_FS1;
				break;
			case "Ch3S0_Brainwash":
				result = AudioClipID.Ch3S0_Brainwash;
				break;
			case "Ch3S0_VaultAmb":
				result = AudioClipID.Ch3S0_VaultAmb;
				break;
			case "Ch3S0_HallwayAmb":
				result = AudioClipID.Ch3S0_HallwayAmb;
				break;
			case "IreneGunAimLong":
				result = AudioClipID.IreneGunAimLong;
				break;
			case "ch1s8b_HeadDrop":
				result = AudioClipID.ch1s8b_HeadDrop;
				break;
			case "IreneGunAim":
				result = AudioClipID.IreneGunAim;
				break;
			case "IreneGrabGun":
				result = AudioClipID.IreneGrabGun;
				break;
			case "GroundedCopter":
				result = AudioClipID.GroundedCopter;
				break;
			case "ch1s8bWallUp":
				result = AudioClipID.ch1s8bWallUp;
				break;
			case "ch1s12_DJVTurn":
				result = AudioClipID.ch1s12_DJVTurn;
				break;
			case "ch1s12_DJDrivingLoop":
				result = AudioClipID.ch1s12_DJDrivingLoop;
				break;
			case "ch1s12_DJVJumpAway":
				result = AudioClipID.ch1s12_DJVJumpAway;
				break;
			case "ch1s12_DJVLand":
				result = AudioClipID.ch1s12_DJVLand;
				break;
			case "ch1s12_DJVLand2":
				result = AudioClipID.ch1s12_DJVLand2;
				break;
			case "Ch1s7_WallImpact":
				result = AudioClipID.Ch1s7_WallImpact;
				break;
			case "Ch1s7_TurnAround":
				result = AudioClipID.Ch1s7_TurnAround;
				break;
			case "DJVMines_Phase2":
				result = AudioClipID.DJVMines_Phase2;
				break;
			case "DJV_TransistionJump03":
				result = AudioClipID.DJV_TransistionJump03;
				break;
			case "DJV_TransistionFall":
				result = AudioClipID.DJV_TransistionFall;
				break;
			case "RedFall2_Phase2":
				result = AudioClipID.RedFall2_Phase2;
				break;
			case "ch2s5b_Landing":
				result = AudioClipID.ch2s5b_Landing;
				break;
			case "ch2s5b_SniperJump":
				result = AudioClipID.ch2s5b_SniperJump;
				break;
			case "ch2s5b_CW2":
				result = AudioClipID.ch2s5b_CW2;
				break;
			case "ch2s5b_HoodBack":
				result = AudioClipID.ch2s5b_HoodBack;
				break;
			case "ch2s5b_CW1":
				result = AudioClipID.ch2s5b_CW1;
				break;
			case "ch5s6b_CasketSlam2":
				result = AudioClipID.ch5s6b_CasketSlam2;
				break;
			case "ch5s6b_CasketOpen":
				result = AudioClipID.ch5s6b_CasketOpen;
				break;
			case "ch5s6b_SnakeArmUpOpen":
				result = AudioClipID.ch5s6b_SnakeArmUpOpen;
				break;
			case "ch5s6b_SnakeSolaceGrab2":
				result = AudioClipID.ch5s6b_SnakeSolaceGrab2;
				break;
			case "ch5s6b_SnakeHand_Throw":
				result = AudioClipID.ch5s6b_SnakeHand_Throw;
				break;
			case "ch5s6b_TWFS":
				result = AudioClipID.ch5s6b_TWFS;
				break;
			case "ch5s6b_EndingZoom":
				result = AudioClipID.ch5s6b_EndingZoom;
				break;
			case "ch5s6b_BodyFall":
				result = AudioClipID.ch5s6b_BodyFall;
				break;
			case "ch5s4_Fall":
				result = AudioClipID.ch5s4_Fall;
				break;
			case "ch5s6b_HeadSnap3":
				result = AudioClipID.ch5s6b_HeadSnap3;
				break;
			case "ch5s6b_SnakeRumble":
				result = AudioClipID.ch5s6b_SnakeRumble;
				break;
			case "ch5s6b_SnakeLeave":
				result = AudioClipID.ch5s6b_SnakeLeave;
				break;
			case "ch5s6b_SnakeFS1":
				result = AudioClipID.ch5s6b_SnakeFS1;
				break;
			case "ch5s6b_SnakeOpen3":
				result = AudioClipID.ch5s6b_SnakeOpen3;
				break;
			case "ch5s6b_TWHSFall":
				result = AudioClipID.ch5s6b_TWHSFall;
				break;
			case "ch5s6b_SnakeFS2":
				result = AudioClipID.ch5s6b_SnakeFS2;
				break;
			case "ch5s6b_HeadReattach":
				result = AudioClipID.ch5s6b_HeadReattach;
				break;
			case "ch5s6b_SnakeArrivalFINAL":
				result = AudioClipID.ch5s6b_SnakeArrivalFINAL;
				break;
			case "ch5s6b_SnakeGrabRed":
				result = AudioClipID.ch5s6b_SnakeGrabRed;
				break;
			case "ch5s6b_CorpseFall":
				result = AudioClipID.ch5s6b_CorpseFall;
				break;
			case "ch5s6b_IreneLanding":
				result = AudioClipID.ch5s6b_IreneLanding;
				break;
			case "ch5s1_DoorKnock":
				result = AudioClipID.ch5s1_DoorKnock;
				break;
			case "ch5s1_DoorSmash":
				result = AudioClipID.ch5s1_DoorSmash;
				break;
			case "PoliceFSLONG":
				result = AudioClipID.PoliceFSLONG;
				break;
			case "Ch5s1_IreneFS":
				result = AudioClipID.Ch5s1_IreneFS;
				break;
			case "ch5s1_Amb":
				result = AudioClipID.ch5s1_Amb;
				break;
			case "ch5s1_FauxInteractionSFX":
				result = AudioClipID.ch5s1_FauxInteractionSFX;
				break;
			case "ch5s1_FindCorpse":
				result = AudioClipID.ch5s1_FindCorpse;
				break;
			case "ch5s1_EndStinger":
				result = AudioClipID.ch5s1_EndStinger;
				break;
			case "Police_ServerRoom_Ambience":
				result = AudioClipID.Police_ServerRoom_Ambience;
				break;
			case "ch5s4_GraffitiSpray3":
				result = AudioClipID.ch5s4_GraffitiSpray3;
				break;
			case "ch5s4_Alley":
				result = AudioClipID.ch5s4_Alley;
				break;
			case "ch5s4_FelixLand":
				result = AudioClipID.ch5s4_FelixLand;
				break;
			case "ch5s4_GraffitiSpray2":
				result = AudioClipID.ch5s4_GraffitiSpray2;
				break;
			case "PoliceFSLONG_2":
				result = AudioClipID.PoliceFSLONG_2;
				break;
			case "ch5s4_Fan_Loop":
				result = AudioClipID.ch5s4_Fan_Loop;
				break;
			case "ch1s5b_cm3":
				result = AudioClipID.ch1s5b_cm3;
				break;
			case "ch1s5b_cm":
				result = AudioClipID.ch1s5b_cm;
				break;
			case "ch1s5b_cm2":
				result = AudioClipID.ch1s5b_cm2;
				break;
			case "pigeon_temp":
				result = AudioClipID.pigeon_temp;
				break;
			case "ch1s11b_FauxPort":
				result = AudioClipID.ch1s11b_FauxPort;
				break;
			case "ch4s3_vinylHeadTurn":
				result = AudioClipID.ch4s3_vinylHeadTurn;
				break;
			case "ch4s3_RedStand_2":
				result = AudioClipID.ch4s3_RedStand_2;
				break;
			case "ch4s3_camera_2":
				result = AudioClipID.ch4s3_camera_2;
				break;
			case "ch1s11a_DreamDoor":
				result = AudioClipID.ch1s11a_DreamDoor;
				break;
			case "ch4s3_camera_1":
				result = AudioClipID.ch4s3_camera_1;
				break;
			case "ch4s3_RedStand":
				result = AudioClipID.ch4s3_RedStand;
				break;
			case "ch1s8b_flex":
				result = AudioClipID.ch1s8b_flex;
				break;
			case "ch1s11b_DreamEnd":
				result = AudioClipID.ch1s11b_DreamEnd;
				break;
			case "ch4s3_memorywhoosh":
				result = AudioClipID.ch4s3_memorywhoosh;
				break;
			case "ch4s3_RedFix":
				result = AudioClipID.ch4s3_RedFix;
				break;
			case "ch1s11a_GiantDJ":
				result = AudioClipID.ch1s11a_GiantDJ;
				break;
			case "femaleHey":
				result = AudioClipID.femaleHey;
				break;
			case "femaleWatchIt":
				result = AudioClipID.femaleWatchIt;
				break;
			case "femaleLookOut":
				result = AudioClipID.femaleLookOut;
				break;
			case "maleBusiness":
				result = AudioClipID.maleBusiness;
				break;
			case "bird":
				result = AudioClipID.bird;
				break;
			case "maleScream":
				result = AudioClipID.maleScream;
				break;
			case "maleGasp":
				result = AudioClipID.maleGasp;
				break;
			case "maleRobo":
				result = AudioClipID.maleRobo;
				break;
			case "Crow":
				result = AudioClipID.Crow;
				break;
			case "Seagull":
				result = AudioClipID.Seagull;
				break;
			case "ch2s6b_EclipseCrewMove":
				result = AudioClipID.ch2s6b_EclipseCrewMove;
				break;
			case "ch2s6b_EnterDream":
				result = AudioClipID.ch2s6b_EnterDream;
				break;
			case "ch2s6b_EclipseSoloMove":
				result = AudioClipID.ch2s6b_EclipseSoloMove;
				break;
			case "ch2s6b_EclipseFortuneStart":
				result = AudioClipID.ch2s6b_EclipseFortuneStart;
				break;
			case "ch2s6b_EclipseFortuneEnd":
				result = AudioClipID.ch2s6b_EclipseFortuneEnd;
				break;
			case "roboOldhead1":
				result = AudioClipID.roboOldhead1;
				break;
			case "whatsGoodLow":
				result = AudioClipID.whatsGoodLow;
				break;
			case "roboOldhead2":
				result = AudioClipID.roboOldhead2;
				break;
			case "huh3":
				result = AudioClipID.huh3;
				break;
			case "notice":
				result = AudioClipID.notice;
				break;
			case "gasp":
				result = AudioClipID.gasp;
				break;
			case "hu":
				result = AudioClipID.hu;
				break;
			case "nah":
				result = AudioClipID.nah;
				break;
			case "ch2s7b_Static":
				result = AudioClipID.ch2s7b_Static;
				break;
			case "ch2s7b_fall":
				result = AudioClipID.ch2s7b_fall;
				break;
			case "VoiceBlockGuyDisagree":
				result = AudioClipID.VoiceBlockGuyDisagree;
				break;
			case "VoiceOldheadRoboTalk":
				result = AudioClipID.VoiceOldheadRoboTalk;
				break;
			case "VoiceOldheadsDialogue":
				result = AudioClipID.VoiceOldheadsDialogue;
				break;
			case "VoiceBlockGuySurprise":
				result = AudioClipID.VoiceBlockGuySurprise;
				break;
			case "VoiceSpaceGirlSurprise":
				result = AudioClipID.VoiceSpaceGirlSurprise;
				break;
			case "VoiceSpaceGirlNotice":
				result = AudioClipID.VoiceSpaceGirlNotice;
				break;
			case "VoiceOldheadLadyTalk":
				result = AudioClipID.VoiceOldheadLadyTalk;
				break;
			case "MallDreamEnd_Echo1":
				result = AudioClipID.MallDreamEnd_Echo1;
				break;
			case "MallDreamEnd_Verb":
				result = AudioClipID.MallDreamEnd_Verb;
				break;
			case "MallDreamEnd_Echo2":
				result = AudioClipID.MallDreamEnd_Echo2;
				break;
			case "TaxiPeelOutMission":
				result = AudioClipID.TaxiPeelOutMission;
				break;
			case "TaxiPeelOutMission2":
				result = AudioClipID.TaxiPeelOutMission2;
				break;
			case "TaxiStart":
				result = AudioClipID.TaxiStart;
				break;
			case "PoliceSoloFS_02":
				result = AudioClipID.PoliceSoloFS_02;
				break;
			case "PoliceSoloFS_04":
				result = AudioClipID.PoliceSoloFS_04;
				break;
			case "TrapDoorClose":
				result = AudioClipID.TrapDoorClose;
				break;
			case "TrapDoorOpen":
				result = AudioClipID.TrapDoorOpen;
				break;
			case "MasterFoleyClip":
				result = AudioClipID.MasterFoleyClip;
				break;
			case "ch4s5b_DoorCreak_02":
				result = AudioClipID.ch4s5b_DoorCreak_02;
				break;
			case "ch4s5b_DoorCreak_03":
				result = AudioClipID.ch4s5b_DoorCreak_03;
				break;
			case "ch5s6b_BodyFall2":
				result = AudioClipID.ch5s6b_BodyFall2;
				break;
			case "ch4s5b_DoorCreak_01":
				result = AudioClipID.ch4s5b_DoorCreak_01;
				break;
			case "ch4s5b_SolaceImpact":
				result = AudioClipID.ch4s5b_SolaceImpact;
				break;
			case "ch4s5b_SolaceFoley":
				result = AudioClipID.ch4s5b_SolaceFoley;
				break;
			case "LowCameraWhoosh":
				result = AudioClipID.LowCameraWhoosh;
				break;
			case "cutscene_landing_01":
				result = AudioClipID.cutscene_landing_01;
				break;
			case "cutscene_landing_04":
				result = AudioClipID.cutscene_landing_04;
				break;
			case "cutscene_landing_03":
				result = AudioClipID.cutscene_landing_03;
				break;
			case "cutscene_landing_05":
				result = AudioClipID.cutscene_landing_05;
				break;
			case "cutscene_landing_06":
				result = AudioClipID.cutscene_landing_06;
				break;
			case "cutscene_landing_02":
				result = AudioClipID.cutscene_landing_02;
				break;
			case "NoPackJump_02":
				result = AudioClipID.NoPackJump_02;
				break;
			case "NoPackJump_06":
				result = AudioClipID.NoPackJump_06;
				break;
			case "NoPackJump_01":
				result = AudioClipID.NoPackJump_01;
				break;
			case "NoPackJump_04":
				result = AudioClipID.NoPackJump_04;
				break;
			case "NoPackJump_08":
				result = AudioClipID.NoPackJump_08;
				break;
			case "NoPackJump_03":
				result = AudioClipID.NoPackJump_03;
				break;
			case "NoPackJump_05":
				result = AudioClipID.NoPackJump_05;
				break;
			case "NoPackJump_07":
				result = AudioClipID.NoPackJump_07;
				break;
			case "ch4s6a_DJ_Jump":
				result = AudioClipID.ch4s6a_DJ_Jump;
				break;
			case "ch4s6a_JumpInVehicle":
				result = AudioClipID.ch4s6a_JumpInVehicle;
				break;
			case "femaleGasp":
				result = AudioClipID.femaleGasp;
				break;
			case "ch4s6a_DJVLand":
				result = AudioClipID.ch4s6a_DJVLand;
				break;
			case "ch4s6a_hatchclose":
				result = AudioClipID.ch4s6a_hatchclose;
				break;
			case "ch4s7a_DreamEnd":
				result = AudioClipID.ch4s7a_DreamEnd;
				break;
			case "ch4s7a_Pad":
				result = AudioClipID.ch4s7a_Pad;
				break;
			case "ch4s7a_IntroWhoosh":
				result = AudioClipID.ch4s7a_IntroWhoosh;
				break;
			case "ch4s7a_Ambience":
				result = AudioClipID.ch4s7a_Ambience;
				break;
			case "ch4s7b_punch":
				result = AudioClipID.ch4s7b_punch;
				break;
			case "ch4s7b_DJMaskFall_2":
				result = AudioClipID.ch4s7b_DJMaskFall_2;
				break;
			case "ch4s7b_DJMaskFall":
				result = AudioClipID.ch4s7b_DJMaskFall;
				break;
			case "VoiceBlockGuyDialogue":
				result = AudioClipID.VoiceBlockGuyDialogue;
				break;
			case "SnakeArmImpact_05":
				result = AudioClipID.SnakeArmImpact_05;
				break;
			case "SnakeArmImpact_01":
				result = AudioClipID.SnakeArmImpact_01;
				break;
			case "SnakeChase_SnakeMovement":
				result = AudioClipID.SnakeChase_SnakeMovement;
				break;
			case "SnakeArmImpact_02":
				result = AudioClipID.SnakeArmImpact_02;
				break;
			case "SnakeArmImpact_04":
				result = AudioClipID.SnakeArmImpact_04;
				break;
			case "SnakeArmImpact_03":
				result = AudioClipID.SnakeArmImpact_03;
				break;
			case "snakeroar_02":
				result = AudioClipID.snakeroar_02;
				break;
			case "VoiceChiefDialogue":
				result = AudioClipID.VoiceChiefDialogue;
				break;
			case "VoiceSpaceGirlDialogue":
				result = AudioClipID.VoiceSpaceGirlDialogue;
				break;
			case "SnakeBossFinalScene_FelixJump":
				result = AudioClipID.SnakeBossFinalScene_FelixJump;
				break;
			case "SnakeBossFinalScene_FelixLandWhoosh":
				result = AudioClipID.SnakeBossFinalScene_FelixLandWhoosh;
				break;
			case "SnakeBossFinalScene_JetPackImpact_02":
				result = AudioClipID.SnakeBossFinalScene_JetPackImpact_02;
				break;
			case "SnakeBossFinalScene_HeadCrack":
				result = AudioClipID.SnakeBossFinalScene_HeadCrack;
				break;
			case "SnakeBossFinalScene_FelixFlipBoost":
				result = AudioClipID.SnakeBossFinalScene_FelixFlipBoost;
				break;
			case "SnakeBossFinalScene_FelixLandImpact":
				result = AudioClipID.SnakeBossFinalScene_FelixLandImpact;
				break;
			case "SnakeBossFinalScene_HeadImpact":
				result = AudioClipID.SnakeBossFinalScene_HeadImpact;
				break;
			case "SnakeBossFinalScene_JetPackPass_01":
				result = AudioClipID.SnakeBossFinalScene_JetPackPass_01;
				break;
			case "SnakeBossFinalScene_ACLoopEnding":
				result = AudioClipID.SnakeBossFinalScene_ACLoopEnding;
				break;
			case "SnakeBossFinalScene_JetPackImpact_01":
				result = AudioClipID.SnakeBossFinalScene_JetPackImpact_01;
				break;
			case "SnakeBossFinalScene_Close_02":
				result = AudioClipID.SnakeBossFinalScene_Close_02;
				break;
			case "SnakeBossFinalScene_Close_01":
				result = AudioClipID.SnakeBossFinalScene_Close_01;
				break;
			case "SnakeBossFinalScene_HeadShatter":
				result = AudioClipID.SnakeBossFinalScene_HeadShatter;
				break;
			case "SnakeBossFinalScene_JetPackPass_02":
				result = AudioClipID.SnakeBossFinalScene_JetPackPass_02;
				break;
			case "SnakeBossFinalScene_SolaceJP":
				result = AudioClipID.SnakeBossFinalScene_SolaceJP;
				break;
			case "SnakeBossFinalScene_SnakeFall":
				result = AudioClipID.SnakeBossFinalScene_SnakeFall;
				break;
			case "SnakeBossFinalScene_FelixBoostIncoming":
				result = AudioClipID.SnakeBossFinalScene_FelixBoostIncoming;
				break;
			case "SnakeBossFinalScene_Open":
				result = AudioClipID.SnakeBossFinalScene_Open;
				break;
			case "ch1s4_DoorCreak":
				result = AudioClipID.ch1s4_DoorCreak;
				break;
			case "ch1s4_Clap_02":
				result = AudioClipID.ch1s4_Clap_02;
				break;
			case "ch4s2_Blood_2":
				result = AudioClipID.ch4s2_Blood_2;
				break;
			case "ch4s2_EscherFall":
				result = AudioClipID.ch4s2_EscherFall;
				break;
			case "Ch4s2_Snipershot_03":
				result = AudioClipID.Ch4s2_Snipershot_03;
				break;
			case "ch4s2_VinylHit":
				result = AudioClipID.ch4s2_VinylHit;
				break;
			case "ch4s2_EscherMalfuntion":
				result = AudioClipID.ch4s2_EscherMalfuntion;
				break;
			case "ch4s2_Tinnitus":
				result = AudioClipID.ch4s2_Tinnitus;
				break;
			case "Ch4s2_SniperArrival_02":
				result = AudioClipID.Ch4s2_SniperArrival_02;
				break;
			case "ch4s5b_portapotty_rumble_2":
				result = AudioClipID.ch4s5b_portapotty_rumble_2;
				break;
			case "ch4s5b_portapotty_rumble_1":
				result = AudioClipID.ch4s5b_portapotty_rumble_1;
				break;
			case "ch2s5a_Bullet_1":
				result = AudioClipID.ch2s5a_Bullet_1;
				break;
			case "ch2s5a_Bullet_3":
				result = AudioClipID.ch2s5a_Bullet_3;
				break;
			case "ch2s5a_Bullet_4":
				result = AudioClipID.ch2s5a_Bullet_4;
				break;
			case "ch2s5a_Bullet_2":
				result = AudioClipID.ch2s5a_Bullet_2;
				break;
			case "VoiceMedusaDialogue":
				result = AudioClipID.VoiceMedusaDialogue;
				break;
			case "dialogueconfirm":
				result = AudioClipID.dialogueconfirm;
				break;
			case "Solace_KnockDown_FenceFall":
				result = AudioClipID.Solace_KnockDown_FenceFall;
				break;
			case "Solace_KnockDown_ScreenShake":
				result = AudioClipID.Solace_KnockDown_ScreenShake;
				break;
			case "maleChill":
				result = AudioClipID.maleChill;
				break;
			case "maleMoan":
				result = AudioClipID.maleMoan;
				break;
			case "maleBro":
				result = AudioClipID.maleBro;
				break;
			case "VoiceEightBallDialogue":
				result = AudioClipID.VoiceEightBallDialogue;
				break;
			case "ShieldCopDeath":
				result = AudioClipID.ShieldCopDeath;
				break;
			case "ShieldCopEngage":
				result = AudioClipID.ShieldCopEngage;
				break;
			case "ShieldCopBlock":
				result = AudioClipID.ShieldCopBlock;
				break;
			case "ShieldCopAttack":
				result = AudioClipID.ShieldCopAttack;
				break;
			case "ShieldCopHurt":
				result = AudioClipID.ShieldCopHurt;
				break;
			case "ShieldCopSpawn":
				result = AudioClipID.ShieldCopSpawn;
				break;
			case "BasicCopSpawn":
				result = AudioClipID.BasicCopSpawn;
				break;
			case "SniperDeath":
				result = AudioClipID.SniperDeath;
				break;
			case "SniperEngage":
				result = AudioClipID.SniperEngage;
				break;
			case "SniperHurt":
				result = AudioClipID.SniperHurt;
				break;
			case "ch5s4_screenshake":
				result = AudioClipID.ch5s4_screenshake;
				break;
			case "VoiceDJDialogue":
				result = AudioClipID.VoiceDJDialogue;
				break;
			case "VoiceFrankDialogue":
				result = AudioClipID.VoiceFrankDialogue;
				break;
			case "Ch3s5_bodyreveal":
				result = AudioClipID.Ch3s5_bodyreveal;
				break;
			case "ch3s5_slowclap":
				result = AudioClipID.ch3s5_slowclap;
				break;
			case "Ch3s5_Amb":
				result = AudioClipID.Ch3s5_Amb;
				break;
			case "ch3s5_FaceReveal":
				result = AudioClipID.ch3s5_FaceReveal;
				break;
			case "ch3s5_EnterDream":
				result = AudioClipID.ch3s5_EnterDream;
				break;
			case "VoiceMetalhead":
				result = AudioClipID.VoiceMetalhead;
				break;
			case "VoiceLegend":
				result = AudioClipID.VoiceLegend;
				break;
			case "VoiceSniperCaptainDialogue":
				result = AudioClipID.VoiceSniperCaptainDialogue;
				break;
			case "VoiceBlockGuyYellDialogue":
				result = AudioClipID.VoiceBlockGuyYellDialogue;
				break;
			case "VoiceOldheadBoomboxTalk":
				result = AudioClipID.VoiceOldheadBoomboxTalk;
				break;
			case "VentLoop":
				result = AudioClipID.VentLoop;
				break;
			case "VentStart":
				result = AudioClipID.VentStart;
				break;
			case "VentEnd":
				result = AudioClipID.VentEnd;
				break;
			case "VoiceTaxiDialogue":
				result = AudioClipID.VoiceTaxiDialogue;
				break;
			case "MineExplosion":
				result = AudioClipID.MineExplosion;
				break;
			case "MineBeep":
				result = AudioClipID.MineBeep;
				break;
			case "VoiceGirl1Dialogue":
				result = AudioClipID.VoiceGirl1Dialogue;
				break;
			case "FinalBossGunStart":
				result = AudioClipID.FinalBossGunStart;
				break;
			case "VoiceDummyDialogue":
				result = AudioClipID.VoiceDummyDialogue;
				break;
			case "DJVinylLoop":
				result = AudioClipID.DJVinylLoop;
				break;
			case "VoiceHeadmanDialogue":
				result = AudioClipID.VoiceHeadmanDialogue;
				break;
			case "VoicePrinceDialogue":
				result = AudioClipID.VoicePrinceDialogue;
				break;
			case "VoiceJetpackBossDialogue":
				result = AudioClipID.VoiceJetpackBossDialogue;
				break;
			case "MachineGunStartVentingAnimation":
				result = AudioClipID.MachineGunStartVentingAnimation;
				break;
			case "MachineGunStopVentingAnimation":
				result = AudioClipID.MachineGunStopVentingAnimation;
				break;
			case "RailRideStart":
				result = AudioClipID.RailRideStart;
				break;
			case "startMortar":
				result = AudioClipID.startMortar;
				break;
			case "stopMortar":
				result = AudioClipID.stopMortar;
				break;
			case "ch3s5_claps2":
				result = AudioClipID.ch3s5_claps2;
				break;
			case "ch3s5_ScreenShake":
				result = AudioClipID.ch3s5_ScreenShake;
				break;
			case "MallDoorClose":
				result = AudioClipID.MallDoorClose;
				break;
			case "MallDoorOpen":
				result = AudioClipID.MallDoorOpen;
				break;
			case "ShortCircuit":
				result = AudioClipID.ShortCircuit;
				break;
			case "BombEater_Alarm_Final":
				result = AudioClipID.BombEater_Alarm_Final;
				break;
			case "BombEater_GameOver_Final":
				result = AudioClipID.BombEater_GameOver_Final;
				break;
			case "BombEater_Movement_Final":
				result = AudioClipID.BombEater_Movement_Final;
				break;
			case "BombEater_RushModeActivated_Final":
				result = AudioClipID.BombEater_RushModeActivated_Final;
				break;
			case "BombEater_CoinSFX_Final":
				result = AudioClipID.BombEater_CoinSFX_Final;
				break;
			case "MAXID_RESERVED":
				result = AudioClipID.MAXID_RESERVED;
				break;
			}
			return result;
		}

		public static SfxCollectionID CollectionNameToSfxCollectionID(string collectionName)
		{
			SfxCollectionID result = SfxCollectionID.NONE;
			switch (collectionName)
			{
			case "AmbienceSfx":
				result = SfxCollectionID.AmbienceSfx;
				break;
			case "CombatSfx":
				result = SfxCollectionID.CombatSfx;
				break;
			case "EnvironmentSfx":
				result = SfxCollectionID.EnvironmentSfx;
				break;
			case "GenericMovementSfx":
				result = SfxCollectionID.GenericMovementSfx;
				break;
			case "GraffitiSfx":
				result = SfxCollectionID.GraffitiSfx;
				break;
			case "GrindCableSfx":
				result = SfxCollectionID.GrindCableSfx;
				break;
			case "GrindMetalHollow":
				result = SfxCollectionID.GrindMetalHollow;
				break;
			case "GrindMetalSfx":
				result = SfxCollectionID.GrindMetalSfx;
				break;
			case "GrindMetalSolidSfx":
				result = SfxCollectionID.GrindMetalSolidSfx;
				break;
			case "MenuSfx":
				result = SfxCollectionID.MenuSfx;
				break;
			case "MoveStyle_0_Default":
				result = SfxCollectionID.MoveStyle_0_Default;
				break;
			case "MoveStyle_1_BMX":
				result = SfxCollectionID.MoveStyle_1_BMX;
				break;
			case "MoveStyle_2_SkateBoard":
				result = SfxCollectionID.MoveStyle_2_SkateBoard;
				break;
			case "MoveStyle_3_Inline":
				result = SfxCollectionID.MoveStyle_3_Inline;
				break;
			case "MoveStyle_4_SpecialSB":
				result = SfxCollectionID.MoveStyle_4_SpecialSB;
				break;
			case "PhoneSfx":
				result = SfxCollectionID.PhoneSfx;
				break;
			case "StorySfx":
				result = SfxCollectionID.StorySfx;
				break;
			case "StreetlifeSfx":
				result = SfxCollectionID.StreetlifeSfx;
				break;
			case "VoiceAngel":
				result = SfxCollectionID.VoiceAngel;
				break;
			case "VoiceBlockGuy":
				result = SfxCollectionID.VoiceBlockGuy;
				break;
			case "VoiceBoarder":
				result = SfxCollectionID.VoiceBoarder;
				break;
			case "VoiceBunGirl":
				result = SfxCollectionID.VoiceBunGirl;
				break;
			case "VoiceDJ":
				result = SfxCollectionID.VoiceDJ;
				break;
			case "VoiceDummy":
				result = SfxCollectionID.VoiceDummy;
				break;
			case "VoiceEightBall":
				result = SfxCollectionID.VoiceEightBall;
				break;
			case "VoiceEightBallBoss":
				result = SfxCollectionID.VoiceEightBallBoss;
				break;
			case "VoiceFrank":
				result = SfxCollectionID.VoiceFrank;
				break;
			case "VoiceFutureGirl":
				result = SfxCollectionID.VoiceFutureGirl;
				break;
			case "VoiceGirl1":
				result = SfxCollectionID.VoiceGirl1;
				break;
			case "VoiceHeadman":
				result = SfxCollectionID.VoiceHeadman;
				break;
			case "VoiceHeadmanNoJetpack":
				result = SfxCollectionID.VoiceHeadmanNoJetpack;
				break;
			case "VoiceJetpackBoss":
				result = SfxCollectionID.VoiceJetpackBoss;
				break;
			case "VoiceLegendFace":
				result = SfxCollectionID.VoiceLegendFace;
				break;
			case "VoiceMedusa":
				result = SfxCollectionID.VoiceMedusa;
				break;
			case "VoiceMetalHead":
				result = SfxCollectionID.VoiceMetalHead;
				break;
			case "VoicePrince":
				result = SfxCollectionID.VoicePrince;
				break;
			case "VoicePufferGirl":
				result = SfxCollectionID.VoicePufferGirl;
				break;
			case "VoiceRingDude":
				result = SfxCollectionID.VoiceRingDude;
				break;
			case "VoiceSpaceGirl":
				result = SfxCollectionID.VoiceSpaceGirl;
				break;
			case "VoiceWideKid":
				result = SfxCollectionID.VoiceWideKid;
				break;
			case "VoiceChiefTalk":
				result = SfxCollectionID.VoiceChiefTalk;
				break;
			case "VoiceDialogue":
				result = SfxCollectionID.VoiceDialogue;
				break;
			case "VoiceRobot":
				result = SfxCollectionID.VoiceRobot;
				break;
			case "VoiceSkate":
				result = SfxCollectionID.VoiceSkate;
				break;
			case "VoiceOldhead":
				result = SfxCollectionID.VoiceOldhead;
				break;
			case "MAXID_RESERVED":
				result = SfxCollectionID.MAXID_RESERVED;
				break;
			}
			return result;
		}

		public static Stage SceneNameToStage(string sceneName)
		{
			Stage result = Stage.NONE;
			switch (sceneName)
			{
			case "city":
				result = Stage.city;
				break;
			case "city_ArtTest":
				result = Stage.city_ArtTest;
				break;
			case "city_graphics":
				result = Stage.city_graphics;
				break;
			case "DEMO_hideout":
				result = Stage.DEMO_hideout;
				break;
			case "downhill":
				result = Stage.downhill;
				break;
			case "hideout":
				result = Stage.hideout;
				break;
			case "Mall":
				result = Stage.Mall;
				break;
			case "osaka":
				result = Stage.osaka;
				break;
			case "Prelude":
				result = Stage.Prelude;
				break;
			case "pyramid":
				result = Stage.pyramid;
				break;
			case "Sandbox":
				result = Stage.Sandbox;
				break;
			case "square":
				result = Stage.square;
				break;
			case "Swirlbox":
				result = Stage.Swirlbox;
				break;
			case "tower":
				result = Stage.tower;
				break;
			}
			return result;
		}

		public static bool GetIsCurrentSceneStage()
		{
			return GetCurrentStage() != Stage.NONE;
		}

		public static Stage GetCurrentStage()
		{
			string name = SceneManager.GetActiveScene().name;
			if (name.Equals(string.Empty))
			{
				return Stage.NONE;
			}
			return SceneNameToStage(name);
		}

		public static List<Resolution> GetAvailableResolutions()
		{
			List<Resolution> list = FindHighestRefreshRateResolutions();
			if (!list.Contains(Screen.currentResolution))
			{
				list.Add(Screen.currentResolution);
			}
			return list;
		}

		private static List<Resolution> FindHighestRefreshRateResolutions()
		{
			List<Resolution> list = new List<Resolution>();
			int i;
			for (i = 0; i < Screen.resolutions.Length; i++)
			{
				int num = list.FindIndex((Resolution resolution) => CompareResolutionDimension(resolution, Screen.resolutions[i]));
				if (num >= 0)
				{
					ReplaceWithHigherRefreshRateResolution(list, num, Screen.resolutions[i]);
				}
			}
			return list;
		}

		private static bool CompareResolutionDimension(Resolution resolutionInCollection, Resolution resolutionToCompare)
		{
			if (resolutionInCollection.height == resolutionToCompare.height)
			{
				return resolutionInCollection.width == resolutionToCompare.width;
			}
			return false;
		}

		private static void ReplaceWithHigherRefreshRateResolution(List<Resolution> resolutions, int indexToReplace, Resolution resolutionToInsert)
		{
			if (resolutionToInsert.refreshRate > resolutions[indexToReplace].refreshRate)
			{
				resolutions.RemoveAt(indexToReplace);
				resolutions.Insert(indexToReplace, resolutionToInsert);
			}
		}

		public static float Lerp(float startValue, float endValue, float t, float maxDelta)
		{
			return Mathf.Clamp(Mathf.Lerp(startValue, endValue, t), startValue - maxDelta, startValue + maxDelta);
		}

		public static int Mod(int x, int m)
		{
			if (m == 0)
			{
				return 0;
			}
			return (x % m + m) % m;
		}
	}
}
