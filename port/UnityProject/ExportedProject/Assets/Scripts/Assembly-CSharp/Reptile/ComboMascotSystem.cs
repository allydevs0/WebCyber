using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Reptile
{
	public class ComboMascotSystem : MonoBehaviour, IInitializableSceneObject
	{
		public enum State
		{
			STANDBY = 0,
			CHECKING_COMBO = 1,
			SUCCESS_DECAY = 2,
			DOOR_OPEN = 3
		}

		public delegate void OnStartedComboDelegate(ComboMascotSystem comboMascotSystem);

		private bool isCheckingComboMascots;

		private State state;

		internal Player player;

		internal int mascotsHit;

		private float stateTimer;

		internal ComboMascot[] comboMascots;

		public ProgressObject door1;

		public ProgressObject door2;

		public PlayableDirector openDoorSequence;

		public UnityEvent OnOpenDoor;

		public bool IsCheckingComboMascots => isCheckingComboMascots;

		public static event OnStartedComboDelegate OnStartedCombo;

		public void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		public void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			StageManager.OnStageInitialized -= OnStageInitialized;
			Core.OnUpdate -= ComboMascotDoorUpdate;
		}

		public virtual void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterSceneObject(this);
		}

		public void InitSceneObject()
		{
			StageManager.OnStageInitialized += OnStageInitialized;
			Core.OnUpdate += ComboMascotDoorUpdate;
			comboMascots = GetComponentsInChildren<ComboMascot>();
			for (int i = 0; i < comboMascots.Length; i++)
			{
				comboMascots[i].comboMascotSystem = this;
				comboMascots[i].num = i;
				comboMascots[i].SetState(ComboMascot.State.STANDBY);
			}
		}

		public void OnStageInitialized()
		{
			player = WorldHandler.instance.GetCurrentPlayer();
			if (!door1.isActive)
			{
				TurnOffMascots();
			}
		}

		public void TurnOffMascots()
		{
			for (int i = 0; i < comboMascots.Length; i++)
			{
				comboMascots[i].transform.parent.gameObject.SetActive(value: false);
			}
		}

		public void ComboMascotDoorUpdate()
		{
			stateTimer += Core.dt;
			switch (state)
			{
			case State.STANDBY:
				if (player.IsComboing() || player.ability == player.airTrickAbility)
				{
					SetState(State.CHECKING_COMBO);
				}
				break;
			case State.CHECKING_COMBO:
				if (!player.IsComboing() && player.ability != player.airTrickAbility)
				{
					SetState(State.STANDBY);
				}
				break;
			case State.SUCCESS_DECAY:
				if (stateTimer >= 1.2f)
				{
					SetState(State.DOOR_OPEN);
				}
				break;
			case State.DOOR_OPEN:
				if (!player.IsComboing())
				{
					SetState(State.STANDBY);
				}
				break;
			}
		}

		public void SetState(State setState)
		{
			if (setState == state)
			{
				return;
			}
			state = setState;
			stateTimer = 0f;
			switch (state)
			{
			case State.STANDBY:
			{
				isCheckingComboMascots = false;
				for (int j = 0; j < comboMascots.Length; j++)
				{
					comboMascots[j].SetState(ComboMascot.State.STANDBY);
				}
				break;
			}
			case State.CHECKING_COMBO:
			{
				mascotsHit = 0;
				for (int i = 0; i < comboMascots.Length; i++)
				{
					comboMascots[i].SetState(ComboMascot.State.READY);
				}
				break;
			}
			case State.SUCCESS_DECAY:
				isCheckingComboMascots = true;
				break;
			case State.DOOR_OPEN:
				isCheckingComboMascots = false;
				SequenceHandler.instance.StartEnteringSequence(openDoorSequence, null, setHidePlayer: true, setInterruptPlayer: false, instantly: true, setPausePlayer: true);
				door1.MakeUnactive();
				door2.MakeUnactive();
				TurnOffMascots();
				OnOpenDoor.Invoke();
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.MascotUnlock);
				break;
			}
		}

		public void SignalMascotHit()
		{
			mascotsHit++;
			if (door1.isActive)
			{
				HandleDoorStillActive();
			}
		}

		private void HandleDoorStillActive()
		{
			AttemptStartComboMascotTrack();
			isCheckingComboMascots = true;
			if (mascotsHit >= comboMascots.Length && state == State.CHECKING_COMBO)
			{
				SetState(State.SUCCESS_DECAY);
			}
		}

		private void AttemptStartComboMascotTrack()
		{
			if (!isCheckingComboMascots && OnStartedCombo != null)
			{
				OnStartedCombo(this);
			}
		}
	}
}
