using Rewired;
using UnityEngine;

namespace Reptile
{
	public class DanceAbility : Ability
	{
		public enum State
		{
			SELECT = 0,
			CHOSEN = 1,
			DANCE = 2,
			END = 3,
			CANCEL = 4
		}

		public bool triggerDanceConversationStarter;

		public State state;

		public int danceNumber;

		public float stateTimer;

		private GameInput gameInput;

		private DanceAbilityUI danceUI;

		private UIManager uIManager;

		private AudioManager audioManager;

		private SaveManager saveManager;

		private int idleHash = Animator.StringToHash("idle");

		private int[] danceHashes;

		public DanceAbility(Player player)
			: base(player)
		{
			gameInput = Core.Instance.GameInput;
			uIManager = Core.Instance.UIManager;
			danceUI = uIManager.danceAbilityUI;
			audioManager = Core.Instance.AudioManager;
			saveManager = Core.Instance.SaveManager;
		}

		public override void Init()
		{
			normalMovement = true;
			normalRotation = false;
			targetSpeed = 0f;
			danceHashes = new int[6];
			for (int i = 0; i < 6; i++)
			{
				int[] array = danceHashes;
				int num = i;
				Dances dances = (Dances)i;
				array[num] = Animator.StringToHash(dances.ToString().ToLowerInvariant());
			}
		}

		public override void OnStartAbility()
		{
			p.phone.TurnOff();
			danceNumber = 0;
			p.SwitchToEquippedMovestyle(set: false);
			p.PlayAnim(p.characterVisual.bounceAnimHash);
			SetState(State.SELECT);
			gameInput.EnableControllerMap(1);
			p.ui.TurnOn(set: false);
		}

		public void SetState(State setState)
		{
			state = setState;
			switch (state)
			{
			case State.SELECT:
				triggerDanceConversationStarter = false;
				uIManager.PushNewMenu(danceUI);
				break;
			case State.CHOSEN:
				triggerDanceConversationStarter = false;
				danceNumber = danceUI.GetCurrentSelectedOption();
				uIManager.PopCurrentMenu();
				audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.confirm);
				if (danceNumber >= 0)
				{
					p.PlayAnim(danceHashes[danceNumber]);
					break;
				}
				p.PlayAnim(idleHash);
				SetState(State.END);
				break;
			case State.DANCE:
				triggerDanceConversationStarter = false;
				p.PlayVoice(AudioClipID.VoiceCombo);
				break;
			case State.END:
				triggerDanceConversationStarter = true;
				break;
			}
			stateTimer = 0f;
		}

		public override void UpdateAbility()
		{
			stateTimer += Core.dt;
			switch (state)
			{
			case State.SELECT:
				if (stateTimer > 0.5f)
				{
					if (danceUI.IsOptionSelected())
					{
						SetState(State.CHOSEN);
					}
					else if (danceUI.HasCancelled())
					{
						SetState(State.CANCEL);
					}
				}
				break;
			case State.CHOSEN:
				if (stateTimer > 0.25f)
				{
					SetState(State.DANCE);
				}
				break;
			case State.DANCE:
				if (stateTimer > 1.5f)
				{
					SetState(State.END);
				}
				break;
			case State.END:
				if (!(stateTimer > 0.5f))
				{
					break;
				}
				p.StopCurrentAbility();
				p.phone.AllowPhone(isAllowed: true, completelyTurnOff: false, turnOnWhenAllowed: true);
				if (saveManager.CurrentSaveSlot.characterSelectLocked)
				{
					break;
				}
				if ((bool)p.inCharacterSelectSpot && WorldHandler.instance.currentEncounter is CombatEncounter)
				{
					SequenceHandler.instance.StartEnteringSequence(p.CharacterSelectWanted, null, setHidePlayer: false, setInterruptPlayer: false);
				}
				else if (p.characterSelectAbility.CanStartHere() && p.sequenceState == SequenceState.NONE)
				{
					if (!p.inCharacterSelectSpot.discovered)
					{
						p.inCharacterSelectSpot.Discover();
					}
					if (p.characterSelectAbility.CanStartAtWantedLevel())
					{
						p.ActivateAbility(p.characterSelectAbility);
					}
					else
					{
						SequenceHandler.instance.StartEnteringSequence(p.CharacterSelectWanted, null, setHidePlayer: false, setInterruptPlayer: false);
					}
				}
				break;
			case State.CANCEL:
				if (stateTimer > 0.2f)
				{
					p.phone.AllowPhone(isAllowed: true, completelyTurnOff: false, turnOnWhenAllowed: true);
					p.StopCurrentAbility();
					audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
				}
				break;
			}
		}

		public override void OnStopAbility()
		{
			gameInput.DisableControllerMap(1);
			p.ui.TurnOn(set: true);
			if (uIManager.IsTopMenu(danceUI))
			{
				uIManager.PopCurrentMenu();
			}
			else if (uIManager.IsMenuTransitioning(danceUI))
			{
				uIManager.PopMenu(danceUI);
			}
			triggerDanceConversationStarter = false;
			base.OnStopAbility();
		}

		public override bool CheckActivation()
		{
			bool flag = uIManager.IsMenuTransitioning(danceUI) || danceUI.IsEnabled;
			if (p.danceButtonNew && p.IsGrounded() && !flag && !locked && (!p.phone.IsOn || gameInput.GetCurrentControllerType() != ControllerType.Joystick))
			{
				p.ActivateAbility(this);
				return true;
			}
			return false;
		}
	}
}
