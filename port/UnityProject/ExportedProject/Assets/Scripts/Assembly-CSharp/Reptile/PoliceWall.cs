using UnityEngine;
using UnityEngine.Events;

namespace Reptile
{
	public class PoliceWall : MonoBehaviour, IInitializableSceneObject
	{
		private enum State
		{
			down = 0,
			movingUp = 1,
			up = 2,
			movingDown = 3
		}

		[SerializeField]
		private Transform moveableWall;

		[SerializeField]
		private Transform edge;

		private const float transitionSpeed = 5f;

		private State state;

		private float wallHeight;

		private float stateTime;

		private Vector3 downPosition;

		private Vector3 upPosition;

		public AudioSource audioSource;

		public UnityEvent OnMovingDown;

		private float timeOutOfSight;

		public void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		public void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			Core.OnUpdate -= UpdateWall;
		}

		public virtual void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterSceneObject(this);
		}

		public void InitSceneObject()
		{
			Core.OnUpdate += UpdateWall;
			downPosition = new Vector3(moveableWall.position.x, edge.position.y, moveableWall.position.z);
			wallHeight = moveableWall.position.y - downPosition.y;
			upPosition = new Vector3(downPosition.x, downPosition.y + wallHeight, downPosition.z);
			StateSwitch(state);
		}

		private void StateSwitch(State newState)
		{
			stateTime = 0f;
			state = newState;
			switch (state)
			{
			case State.down:
				moveableWall.position = downPosition;
				break;
			case State.movingUp:
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.policeWallUp, audioSource);
				break;
			case State.up:
				moveableWall.position = upPosition;
				break;
			case State.movingDown:
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.policeWallDown, audioSource);
				if (WorldHandler.instance.GetCurrentPlayer().GetNormalizedHP() > 0f)
				{
					OnMovingDown.Invoke();
				}
				break;
			}
		}

		private void UpdateWall()
		{
			if (WantedManager.instance.Wanted)
			{
				timeOutOfSight += Core.dt;
			}
			else
			{
				timeOutOfSight = 0f;
			}
			stateTime += Core.dt;
			switch (state)
			{
			case State.down:
				if (WantedManager.instance.Wanted)
				{
					StateSwitch(State.movingUp);
				}
				break;
			case State.movingUp:
				moveableWall.position = Vector3.Lerp(downPosition, upPosition, stateTime / 1.2f);
				if (Mathf.Approximately(moveableWall.position.y, upPosition.y))
				{
					StateSwitch(State.up);
				}
				break;
			case State.up:
				if (!WantedManager.instance.Wanted)
				{
					StateSwitch(State.movingDown);
				}
				break;
			case State.movingDown:
				moveableWall.position = Vector3.Lerp(upPosition, downPosition, stateTime / 1.2f);
				if (Mathf.Approximately(moveableWall.position.y, downPosition.y))
				{
					StateSwitch(State.down);
				}
				break;
			}
		}
	}
}
