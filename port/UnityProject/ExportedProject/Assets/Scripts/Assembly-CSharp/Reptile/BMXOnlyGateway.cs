using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class BMXOnlyGateway : MonoBehaviour
	{
		private enum State
		{
			STANDBY = 0,
			CHECKING = 1,
			ACCESS_DENIED = 2,
			ACCESS_GRANTED = 3,
			OPENING = 4,
			STAY_OPEN = 5
		}

		private enum LightMode
		{
			NONE = 0,
			RED = 1,
			GREEN = 2,
			WHITE = 3
		}

		private State state;

		private LightMode lightMode;

		private Player p;

		private float stateTimer;

		private float playerOnPlatform;

		public GameObject door;

		public GameObject offLight;

		public GameObject redLight;

		public GameObject greenLight;

		public GameObject whiteLight;

		public AudioSource audioSource;

		private const SfxCollectionID sfxCollectionID = SfxCollectionID.EnvironmentSfx;

		private const AudioClipID audioClipChecking = AudioClipID.BMXGatewayChecking;

		private const AudioClipID audioClipDenied = AudioClipID.BMXGatewayDenied;

		private const AudioClipID audioClipApproved = AudioClipID.BMXGatewayApproved;

		public PlayableDirector openDoorSequence;

		private void OnEnable()
		{
			Core.OnUpdate += GatewayUpdate;
		}

		private void OnDisable()
		{
			Core.OnUpdate -= GatewayUpdate;
		}

		private void OnDestroy()
		{
			Core.OnUpdate -= GatewayUpdate;
		}

		private void GatewayUpdate()
		{
			if (playerOnPlatform > 0f)
			{
				playerOnPlatform -= Core.dt;
			}
			switch (state)
			{
			case State.CHECKING:
				if (stateTimer > 1.35f)
				{
					if (playerOnPlatform <= 0f)
					{
						SetState(State.STANDBY);
					}
					else if (p.moveStyle == MoveStyle.BMX)
					{
						SetState(State.ACCESS_GRANTED);
					}
					else
					{
						SetState(State.ACCESS_DENIED);
					}
				}
				break;
			case State.ACCESS_DENIED:
				if (stateTimer > 2f)
				{
					SetState(State.STANDBY);
				}
				break;
			case State.ACCESS_GRANTED:
				if (stateTimer > 2.35f)
				{
					SetState(State.OPENING);
				}
				break;
			case State.OPENING:
				if (stateTimer > 0.4f)
				{
					SetState(State.STAY_OPEN);
				}
				break;
			}
			stateTimer += Core.dt;
		}

		private void SetState(State setState)
		{
			stateTimer = 0f;
			state = setState;
			switch (state)
			{
			case State.STANDBY:
				SetLightMode(LightMode.NONE);
				break;
			case State.CHECKING:
				SetLightMode(LightMode.WHITE);
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.BMXGatewayChecking, audioSource);
				break;
			case State.ACCESS_DENIED:
				SetLightMode(LightMode.RED);
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.BMXGatewayDenied, audioSource);
				break;
			case State.ACCESS_GRANTED:
				SetLightMode(LightMode.GREEN);
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.BMXGatewayApproved, audioSource);
				break;
			case State.OPENING:
				if (openDoorSequence != null)
				{
					SequenceHandler.instance.StartEnteringSequence(openDoorSequence, null, setHidePlayer: true, setInterruptPlayer: true, instantly: true, setPausePlayer: true);
				}
				break;
			case State.STAY_OPEN:
				Object.Destroy(door);
				SetLightMode(LightMode.NONE);
				break;
			}
		}

		private void SetLightMode(LightMode setLight)
		{
			lightMode = setLight;
			offLight.SetActive(setLight == LightMode.NONE);
			redLight.SetActive(setLight == LightMode.RED);
			greenLight.SetActive(setLight == LightMode.GREEN);
			whiteLight.SetActive(setLight == LightMode.WHITE);
		}

		private void OnTriggerStay(Collider other)
		{
			if (other.gameObject.layer == 9)
			{
				if (state == State.STANDBY)
				{
					p = other.transform.parent.GetComponentInParent<Player>();
					SetState(State.CHECKING);
				}
				playerOnPlatform = 0.5f;
			}
		}
	}
}
