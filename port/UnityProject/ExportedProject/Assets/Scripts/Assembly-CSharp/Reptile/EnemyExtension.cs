using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class EnemyExtension : MonoBehaviour
	{
		public enum ExtensionState
		{
			CLOSED = 0,
			OPEN = 1
		}

		public OnGSpotStateChanged onGSpotStateChanged;

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private GraffitiSpotFinisher gSpot;

		[SerializeField]
		private bool playSequenceOnOpen;

		[SerializeField]
		private PlayableDirector openSequence;

		private ExtensionState extensionState;

		private bool receivedHit;

		private bool colliderActive = true;

		private float stateTime;

		private Player player;

		public bool ReceivedHit
		{
			get
			{
				return receivedHit;
			}
			set
			{
				receivedHit = value;
			}
		}

		public ExtensionState State => extensionState;

		public GraffitiSpotFinisher GSpot => gSpot;

		public void Init(BasicEnemy enemy, Player player)
		{
			GSpot.enemyExtension = this;
			GSpot.SetDefaultData(null);
			enemy.OnUpdateActive.AddListener(UpdateExtension);
			this.player = player;
		}

		public void OpenExtension()
		{
			SetState(ExtensionState.OPEN);
		}

		public void CloseExtension()
		{
			SetState(ExtensionState.CLOSED);
		}

		private void SetState(ExtensionState target)
		{
			stateTime = 0f;
			extensionState = target;
			switch (extensionState)
			{
			case ExtensionState.CLOSED:
				animator.Play("ExtensionClose");
				colliderActive = true;
				GSpot.gameObject.SetActive(value: false);
				break;
			case ExtensionState.OPEN:
				if (playSequenceOnOpen)
				{
					SequenceHandler.instance.StartEnteringSequence(openSequence, null, setHidePlayer: true, setInterruptPlayer: false, instantly: true, setPausePlayer: true);
				}
				animator.Play("ExtensionOpen");
				colliderActive = false;
				break;
			}
		}

		public virtual void SetGraffitiResult(GraffitiSpotFinisher gSpotFinisher, GraffitiState result)
		{
			if (onGSpotStateChanged != null)
			{
				onGSpotStateChanged.Invoke(gSpotFinisher, result);
			}
		}

		public void OnTriggerStay(Collider other)
		{
			if (colliderActive && other.gameObject.layer == 18 && !(other.gameObject.GetComponentInParent<Player>() != player))
			{
				receivedHit = true;
				player.airTrickAbility.HitEnemy();
			}
		}

		public void UpdateExtension()
		{
			stateTime += Core.dt;
			ExtensionState extensionState = this.extensionState;
			if (extensionState != ExtensionState.CLOSED && extensionState == ExtensionState.OPEN && stateTime > 0.4f && !GSpot.gameObject.activeSelf)
			{
				GSpot.gameObject.SetActive(value: true);
			}
		}

		public void ResetGraffitiSpots()
		{
			GSpot.ClearPaint();
			GSpot.SetState(GraffitiState.CANCEL);
			GSpot.gameObject.SetActive(value: false);
		}
	}
}
