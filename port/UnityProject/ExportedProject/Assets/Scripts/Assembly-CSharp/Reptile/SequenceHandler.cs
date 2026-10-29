using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	public class SequenceHandler : AMenuController
	{
		internal enum SkipState
		{
			NOT_SKIPPABLE = 0,
			IDLE = 1,
			WAIT_FOR_FADE = 2,
			TEXT_ACTIVE = 3,
			EXITING_SEQUENCE = 4
		}

		public static SequenceHandler instance;

		private GameplayEvent gameplayEvent;

		private ProgressObject progressObject;

		private Battle battle;

		internal bool answeredYes;

		internal bool allowPhoneOnAfterSequence;

		internal bool invokeExitSequenceEvent = true;

		internal bool restartCurrentEncounter;

		internal bool isBusy;

		internal PlayableDirector sequence;

		private bool hidePlayer;

		private bool pausePlayer;

		private bool interruptPlayer;

		private Coroutine exitSequenceRoutine;

		private Coroutine volumeCoroutine;

		internal bool disabledExit;

		private float fadeDuration = 0.4f;

		internal float shutDuration = 0.2f;

		private float skipFadeDuration = 0.3f;

		private float skipStartTimer;

		private float skipTimer;

		private float skipThreshold = 0.4f;

		private bool sequenceOverwritten;

		internal SkipState skipTextActiveState = SkipState.IDLE;

		private float skipTextTimer;

		private bool lowerVolume;

		private Player player;

		private GameInput gameInput;

		private new AudioManager audioManager;

		private BaseModule baseModule;

		public void Init()
		{
			instance = this;
			allowMouseInput = false;
			gameInput = Core.Instance.GameInput;
			uIManager = Core.Instance.UIManager;
			audioManager = Core.Instance.AudioManager;
			baseModule = Core.Instance.BaseModule;
			player = WorldHandler.instance.GetCurrentPlayer();
			Core.OnCoreUpdatePaused += OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused += OnCoreUpdateUnPaused;
		}

		public void UpdateSequenceHandler()
		{
			if (player.sequenceState != SequenceState.IN_SEQUENCE)
			{
				return;
			}
			DialogueUI dialogueUI = uIManager.dialogueUI;
			bool flag = base.IsEnabled && !disabledExit && gameInput.GetButtonNew(2);
			if (flag && dialogueUI.CanBeSkipped && !dialogueUI.isYesNoPromptEnabled)
			{
				if (dialogueUI.ReadyToResume)
				{
					ResumeSequence();
					if (dialogueUI.IsShowingDialogue())
					{
						Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.dialogueconfirm);
					}
					dialogueUI.EndDialogue();
				}
				else
				{
					FastForwardTypewriter();
				}
			}
			if (skipTextActiveState != SkipState.NOT_SKIPPABLE && !dialogueUI.isYesNoPromptEnabled)
			{
				EffectsUI effects = uIManager.effects;
				float dt = Core.dt;
				bool num = base.IsEnabled && gameInput.GetButtonHeld(64);
				if (skipStartTimer >= 0.5f)
				{
					if (skipTextActiveState == SkipState.TEXT_ACTIVE)
					{
						skipTextTimer += dt;
						if (skipTextTimer > 1.5f && skipTimer == 0f)
						{
							Tween tween = effects.FadeSkipOut(skipFadeDuration);
							tween.onComplete = (TweenCallback)Delegate.Combine(tween.onComplete, (TweenCallback)(() =>
							{
								skipTextActiveState = SkipState.IDLE;
							}));
							skipTextActiveState = SkipState.WAIT_FOR_FADE;
							skipTextTimer = 0f;
						}
					}
					else if (skipTextActiveState == SkipState.IDLE && !flag && (gameInput.GetButtonHeld(64) || gameInput.GetButtonHeld(4)))
					{
						Tween tween2 = effects.FadeSkipIn(skipFadeDuration);
						tween2.onComplete = (TweenCallback)Delegate.Combine(tween2.onComplete, (TweenCallback)(() =>
						{
							skipTextActiveState = SkipState.TEXT_ACTIVE;
						}));
						skipTextActiveState = SkipState.WAIT_FOR_FADE;
					}
				}
				skipStartTimer += dt;
				if (num && skipStartTimer >= 0.5f)
				{
					skipTimer += dt;
					skipTextTimer = 0f;
				}
				else
				{
					skipTimer = 0f;
				}
			}
			if (exitSequenceRoutine == null && !disabledExit && (sequence.time >= sequence.duration - (double)fadeDuration || skipTimer >= skipThreshold))
			{
				exitSequenceRoutine = StartCoroutine(ExitSequenceRoutine());
			}
		}

		public void ExitCurrentSequence()
		{
			exitSequenceRoutine = StartCoroutine(ExitSequenceRoutine());
		}

		private void OnCoreUpdatePaused()
		{
			if (sequence != null && !uIManager.dialogueUI.isYesNoPromptEnabled && !disabledExit)
			{
				FixedFramerateSequence component = sequence.GetComponent<FixedFramerateSequence>();
				if (component != null)
				{
					component.Pause();
				}
				else if (sequence.playableGraph.IsValid())
				{
					sequence.playableGraph.GetRootPlayable(0).SetSpeed(0.0);
				}
			}
		}

		private void OnCoreUpdateUnPaused()
		{
			if (!(sequence != null) || uIManager.dialogueUI.isYesNoPromptEnabled || disabledExit)
			{
				return;
			}
			FixedFramerateSequence component = sequence.GetComponent<FixedFramerateSequence>();
			if (component != null)
			{
				component.Resume();
			}
			else
			{
				if (!sequence.playableGraph.IsValid())
				{
					return;
				}
				DialogueUI dialogueUI = uIManager.dialogueUI;
				if (dialogueUI != null)
				{
					bool flag = dialogueUI.IsDialogueUIActive();
					if (!flag || (flag && !dialogueUI.CanBeSkipped))
					{
						sequence.playableGraph.GetRootPlayable(0).SetSpeed(1.0);
					}
				}
			}
		}

		public void SetPreEnteringSequence()
		{
			player.sequenceState = SequenceState.PRE_ENTERING;
		}

		public void StartEnteringSequence(PlayableDirector setSequence, GameplayEvent setGameplayEvent, bool setHidePlayer = true, bool setInterruptPlayer = true, bool instantly = false, bool setPausePlayer = false, bool setAllowPhoneOnAfterSequence = true, ProgressObject setProgressObject = null, bool skippable = true, bool lowerVolumeDuringSequence = false, Battle setBattle = null, bool disabledExitOnInput = false)
		{
			sequenceOverwritten = false;
			if (IsInSequence() && player.sequenceState != SequenceState.PRE_ENTERING)
			{
				FixedFramerateSequence component = sequence.gameObject.GetComponent<FixedFramerateSequence>();
				if (component != null)
				{
					component.Stop();
				}
				else
				{
					sequence.Stop();
				}
				sequenceOverwritten = true;
			}
			if (player.ability == player.characterSelectAbility)
			{
				instantly = false;
			}
			sequence = setSequence;
			skipTimer = 0f;
			skipTextActiveState = (skippable ? SkipState.IDLE : SkipState.NOT_SKIPPABLE);
			skipTextTimer = 0f;
			isBusy = true;
			invokeExitSequenceEvent = true;
			restartCurrentEncounter = false;
			gameplayEvent = setGameplayEvent;
			progressObject = setProgressObject;
			hidePlayer = setHidePlayer;
			interruptPlayer = setInterruptPlayer;
			pausePlayer = setPausePlayer;
			allowPhoneOnAfterSequence = setAllowPhoneOnAfterSequence;
			battle = setBattle;
			lowerVolume = (lowerVolume != lowerVolumeDuringSequence) & lowerVolumeDuringSequence;
			disabledExit = disabledExitOnInput;
			audioManager.PauseAllGameplayLoopingSfx();
			audioManager.ClearAllGameplayLoopingSfx();
			if (uIManager.IsShowingAnyMenu)
			{
				uIManager.PopAllMenusInstant();
			}
			if (instantly)
			{
				SetInSequenceImmediate();
			}
			else
			{
				StartCoroutine(EnterSequenceRoutine());
			}
		}

		private IEnumerator EnterSequenceRoutine()
		{
			player.sequenceState = SequenceState.ENTERING;
			if (player.inGraffitiGame)
			{
				yield return new WaitUntil(() => !player.inGraffitiGame);
			}
			if (player.ability == player.characterSelectAbility)
			{
				yield return new WaitUntil(() => player.ability != player.characterSelectAbility);
			}
			else if (player.ability == player.danceAbility)
			{
				player.StopCurrentAbility();
			}
			player.userInputEnabled = false;
			player.phone.AllowPhone(isAllowed: false);
			SetPartiallyInSequence();
			yield return uIManager.effects.FadeToBlack(fadeDuration).WaitForCompletion();
			SetFullyInSequence();
			yield return new WaitForSeconds(shutDuration);
			uIManager.effects.FadeOpen(fadeDuration);
			uIManager.effects.ShowVerticalBars();
		}

		private void SetPartiallyInSequence()
		{
			player.FlushInput();
			if (interruptPlayer)
			{
				player.CompletelyStop();
			}
			if (pausePlayer)
			{
				player.PauseForSequence(set: true);
			}
			player.StopHoldProps();
			player.DestroyPickupVisuals();
			player.phone.AllowPhone(isAllowed: false);
			if (!sequenceOverwritten)
			{
				player.DisablePlayer();
			}
			if (lowerVolume)
			{
				audioManager.StartTemporarilyLowerVolume(this);
			}
		}

		private void SetFullyInSequence()
		{
			player.sequenceState = SequenceState.IN_SEQUENCE;
			if ((bool)gameplayEvent)
			{
				gameplayEvent.EnterSequence(sequence);
			}
			if ((bool)battle)
			{
				battle.EnterSequence(sequence);
			}
			if ((bool)progressObject)
			{
				progressObject.EnterSequence(sequence);
			}
			if (hidePlayer)
			{
				player.HideForSequence(set: true);
			}
			bool allCamerasForSequenceEnter = false;
			if (progressObject != null)
			{
				allCamerasForSequenceEnter = progressObject.usePhone > 0;
			}
			player.phone.SetAllCamerasForSequenceEnter(allCamerasForSequenceEnter);
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMap(1);
			if (!base.IsEnabled)
			{
				uIManager.PushNewMenuInstant(this);
			}
			player.ui.gameObject.SetActive(value: false);
			sequence.gameObject.SetActive(value: true);
			ReplacePlayerVisualsInCutscene();
			ReplaceMaterialsOnCharactersInCutscene();
			FixedFramerateSequence component = sequence.gameObject.GetComponent<FixedFramerateSequence>();
			if (component != null)
			{
				component.Play();
			}
			else
			{
				sequence.Play();
			}
			skipStartTimer = 0f;
		}

		private void SetInSequenceImmediate()
		{
			EffectsUI effects = uIManager.effects;
			if (!effects.IsFadedToColor(EffectsUI.niceClear))
			{
				effects.FadeOpen(0f);
				effects.ShowVerticalBars();
			}
			SetPartiallyInSequence();
			SetFullyInSequence();
		}

		private void ReplacePlayerVisualsInCutscene()
		{
			Transform transform = sequence.transform.FindRecursive("CutscenePlayerVisuals");
			if (!(transform != null))
			{
				return;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(WorldHandler.instance.GetCurrentPlayer().visualTf.gameObject);
			CharacterVisual component = gameObject.GetComponent<CharacterVisual>();
			component.SetMoveStyleVisualAnim(player, MoveStyle.ON_FOOT);
			component.SetMoveStyleVisualProps(player, MoveStyle.ON_FOOT);
			GameObject characterObject = component.characterObject;
			GameObject gameObject2 = transform.gameObject;
			gameObject.name = gameObject2.name;
			characterObject.name = gameObject2.GetComponent<CharacterVisual>().characterObject.name;
			gameObject.transform.parent = gameObject2.transform.parent;
			gameObject.transform.position = gameObject2.transform.position;
			gameObject.transform.rotation = gameObject2.transform.rotation;
			RuntimeAnimatorController runtimeAnimatorController = gameObject2.GetComponent<CharacterVisual>().characterObject.GetComponent<Animator>().runtimeAnimatorController;
			Animator component2 = characterObject.GetComponent<Animator>();
			component2.runtimeAnimatorController = runtimeAnimatorController;
			component2.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			LookAtIKComponent value = characterObject.AddComponent<LookAtIKComponent>();
			IEnumerator<PlayableBinding> enumerator = sequence.playableAsset.outputs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				UnityEngine.Object sourceObject = enumerator.Current.sourceObject;
				if (!sequence.GetGenericBinding(sourceObject))
				{
					continue;
				}
				string text = sequence.GetGenericBinding(sourceObject).name;
				if (text == "CharacterObject")
				{
					if (sourceObject is AnimationTrack)
					{
						sequence.SetGenericBinding(sourceObject, component2);
					}
					else if (sourceObject is LookAtIKTrack)
					{
						sequence.SetGenericBinding(sourceObject, value);
					}
					else
					{
						sequence.SetGenericBinding(sourceObject, characterObject.gameObject);
					}
				}
				if (text == "CutscenePlayerVisuals")
				{
					if (typeof(Animator).IsAssignableFrom(enumerator.Current.outputTargetType))
					{
						Animator animator = gameObject.AddComponent<Animator>();
						animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
						sequence.SetGenericBinding(sourceObject, animator);
					}
					else
					{
						sequence.SetGenericBinding(sourceObject, gameObject);
					}
				}
			}
			UnityEngine.Object.Destroy(gameObject2);
		}

		private void ReplaceMaterialsOnCharactersInCutscene()
		{
			OutfitSwappableCharacter[] componentsInChildren = sequence.transform.GetComponentsInChildren<OutfitSwappableCharacter>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					int outfit = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(componentsInChildren[i].Character).outfit;
					Material mat = player.CharacterConstructor.CreateCharacterMaterial(componentsInChildren[i].Character, outfit);
					componentsInChildren[i].SetMaterialAndOutfit(mat, outfit);
				}
			}
		}

		private IEnumerator ExitSequenceRoutine()
		{
			player.sequenceState = SequenceState.EXITING;
			Core coreInstance = Core.Instance;
			EffectsUI effectsUI = uIManager.effects;
			skipTextActiveState = SkipState.EXITING_SEQUENCE;
			effectsUI.FadeSkipOut(skipFadeDuration);
			double num = Time.deltaTime;
			float num2 = Mathf.Clamp((float)(sequence.duration - (sequence.time + num)), 0f, fadeDuration);
			float stayBlackSeconds = fadeDuration - num2;
			Tween t = effectsUI.FadeToBlack(num2);
			yield return t.WaitForCompletion();
			yield return new WaitForSeconds(stayBlackSeconds);
			SetExitSequence();
			yield return new WaitForSeconds(shutDuration);
			yield return new WaitWhile(() => coreInstance.IsCorePaused);
			if (!IsInSequence())
			{
				effectsUI.FadeOpen(fadeDuration);
				isBusy = false;
				effectsUI.HideVerticalBars();
			}
			exitSequenceRoutine = null;
		}

		private void SetExitSequence()
		{
			if (hidePlayer)
			{
				player.HideForSequence(set: false);
			}
			if (pausePlayer)
			{
				player.PauseForSequence(set: false);
			}
			if (lowerVolume || (audioManager.IsVolumeLowered && !IsInSequence()))
			{
				lowerVolume = false;
				audioManager.StopTemporarilyLowerVolume(this);
			}
			player.sequenceState = SequenceState.NONE;
			player.RecheckLoopingSounds();
			sequence.time = 0.0;
			FixedFramerateSequence component = sequence.gameObject.GetComponent<FixedFramerateSequence>();
			if (component != null)
			{
				component.Stop();
			}
			else
			{
				sequence.Stop();
			}
			WorldHandler worldHandler = WorldHandler.instance;
			if ((bool)worldHandler)
			{
				Player currentPlayer = worldHandler.GetCurrentPlayer();
				if (currentPlayer != null)
				{
					GameplayCamera cam = currentPlayer.cam;
					if (cam != null && worldHandler.CurrentCamera != cam.cam)
					{
						worldHandler.CurrentCamera.enabled = false;
					}
				}
			}
			sequence.gameObject.SetActive(value: false);
			uIManager.AbsolutePopIfPresent(this);
			if (uIManager.dialogueUI.IsShowingDialogue())
			{
				uIManager.dialogueUI.AbortDialogue();
			}
			if ((bool)gameplayEvent)
			{
				gameplayEvent.ExitSequence(sequence, invokeExitSequenceEvent, restartCurrentEncounter);
			}
			else if ((bool)progressObject)
			{
				progressObject.ExitSequence(sequence, invokeExitSequenceEvent, restartCurrentEncounter);
			}
			else if ((bool)battle)
			{
				battle.ExitSequence(sequence);
			}
			else
			{
				LetPlayerExitSequence();
			}
		}

		public void AnswerYesNoInvokeEvent(bool answer)
		{
			answeredYes = answer;
			invokeExitSequenceEvent = answer;
			ResumeSequence();
		}

		public void AnswerYesNoInvokeOrRestartEncounter(bool answer)
		{
			answeredYes = answer;
			invokeExitSequenceEvent = answer;
			restartCurrentEncounter = !answer;
			ResumeSequence();
		}

		public void AnswerYesNoGeneric(bool answer)
		{
			answeredYes = answer;
			ResumeSequence();
		}

		private void FastForwardTypewriter()
		{
			uIManager.dialogueUI.FastForwardTypewriter();
		}

		public static void PauseGraph(PlayableDirector director)
		{
			FixedFramerateSequence component = director.GetComponent<FixedFramerateSequence>();
			if (component != null)
			{
				component.Pause();
			}
			else
			{
				director.playableGraph.GetRootPlayable(0).SetSpeed(0.0);
			}
		}

		public void ResumeSequence()
		{
			FixedFramerateSequence component = sequence.gameObject.GetComponent<FixedFramerateSequence>();
			if (component != null)
			{
				component.Resume();
			}
			else
			{
				sequence.playableGraph.GetRootPlayable(0).SetSpeed(1.0);
			}
		}

		public void LetPlayerExitSequence()
		{
			player.userInputEnabled = true;
			player.ui.gameObject.SetActive(value: true);
			player.phone.SetAllCamerasForSequenceExit();
			player.phone.AllowPhone(isAllowed: true, completelyTurnOff: false, allowPhoneOnAfterSequence);
			player.EnablePlayer();
			if (!uIManager.IsShowingAnyMenu && baseModule.IsPlayingInStage)
			{
				baseModule.StageManager.RestoreCurrentPlayerInput();
			}
		}

		public bool IsInSequence()
		{
			return player.sequenceState != SequenceState.NONE;
		}

		public bool IsInSequenceTransition()
		{
			if (player.sequenceState != SequenceState.ENTERING)
			{
				return player.sequenceState == SequenceState.EXITING;
			}
			return true;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (Core.Instance != null && uIManager != null)
			{
				uIManager.AbsolutePopIfPresent(this);
			}
			audioManager = null;
			uIManager = null;
			gameInput = null;
			player = null;
			instance = null;
			Core.OnCoreUpdatePaused -= OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused -= OnCoreUpdateUnPaused;
		}

		public override GameObject GetFirstFocusGameObject()
		{
			return null;
		}

		public override void Activate()
		{
		}

		public override void Deactivate()
		{
		}

		public override GameObject GetRoot()
		{
			if (sequence != null)
			{
				return sequence.gameObject;
			}
			return null;
		}
	}
}
