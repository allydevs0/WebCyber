using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class CharacterSelectCharacter : GameplayEvent
	{
		public enum CharSelectCharState
		{
			JOINING = 0,
			BOUNCING = 1,
			GOING_IN = 2,
			DOING_RUN = 3,
			GOING_OUT = 4,
			TALKING = 5
		}

		public CharSelectCharState state;

		public PlayableDirector swapSequence;

		public float startOffset;

		public int startSequence;

		internal Characters character;

		internal CharacterVisual visual;

		private Transform tf;

		private Vector3 startPos;

		private Vector3 centerPos;

		private Quaternion targetRot;

		private Quaternion startRot;

		private float speed = 1f;

		private float runDuration = 22f;

		private float stateTimer;

		private CharacterSelect CharSelect;

		private GameObject colliderObj;

		private GameObject triggerObj;

		private int walkAnimHash;

		private int bounceAnimHash;

		private int dance4AnimHash;

		private int poleFlipAnimHash;

		private int landAnimHash;

		private int runAnimHash;

		private int boostBrakeAnimHash;

		private int joinCypher1Hash;

		private int joinCypher2Hash;

		private int joinCypher3Hash;

		private int joinCypher4Hash;

		private int joinCypher5Hash;

		private int freeStyleHash;

		private bool waited;

		public Vector3 CenterPos
		{
			set
			{
				centerPos = value;
			}
		}

		public Vector3 StartPos => startPos;

		public void Init(CharacterSelect SetCharacterSelect, Characters setCharacter, GameObject collider, GameObject talkTrigger, Vector3 centerPosition, PlayableDirector swapCharacterSequence)
		{
			CharSelect = SetCharacterSelect;
			visual = GetComponent<CharacterVisual>();
			character = setCharacter;
			visual.handIKActiveR = (visual.handIKActiveL = false);
			visual.SetBoostpackEffect(BoostpackEffectMode.OFF);
			visual.SetFrictionEffect(FrictionEffectMode.OFF);
			visual.anim.gameObject.AddComponent<NonPlayerAnimationEventRelay>().Init();
			tf = base.transform;
			startPos = tf.position;
			startRot = (targetRot = tf.rotation);
			colliderObj = Object.Instantiate(collider, base.transform);
			triggerObj = Object.Instantiate(talkTrigger, base.transform);
			centerPos = centerPosition;
			swapSequence = swapCharacterSequence;
			walkAnimHash = Animator.StringToHash("walk");
			dance4AnimHash = Animator.StringToHash("dance4");
			poleFlipAnimHash = Animator.StringToHash("poleFlip");
			landAnimHash = Animator.StringToHash("land");
			runAnimHash = Animator.StringToHash("run");
			boostBrakeAnimHash = Animator.StringToHash("boostBrake");
			joinCypher1Hash = Animator.StringToHash("joinCypher1");
			joinCypher2Hash = Animator.StringToHash("joinCypher2");
			joinCypher3Hash = Animator.StringToHash("joinCypher3");
			joinCypher4Hash = Animator.StringToHash("joinCypher4");
			joinCypher5Hash = Animator.StringToHash("joinCypher5");
			freeStyleHash = CharacterVisual.GetCharacterFreestyleAnim(character);
		}

		public void UpdateCharacter()
		{
			stateTimer += Core.dt;
			_ = speed;
			_ = stateTimer;
			switch (state)
			{
			case CharSelectCharState.JOINING:
				if (stateTimer > startOffset && !waited)
				{
					int stateNameHash = joinCypher1Hash;
					switch (startSequence)
					{
					case 0:
						stateNameHash = joinCypher1Hash;
						break;
					case 1:
						stateNameHash = joinCypher2Hash;
						break;
					case 2:
						stateNameHash = joinCypher3Hash;
						break;
					case 3:
						stateNameHash = joinCypher4Hash;
						break;
					case 4:
						stateNameHash = joinCypher5Hash;
						break;
					}
					visual.anim.Play(stateNameHash);
					waited = true;
				}
				if (stateTimer >= 1.5f + startOffset)
				{
					SetState(CharSelectCharState.BOUNCING);
					speed = 1f;
				}
				break;
			case CharSelectCharState.GOING_IN:
				SetState(CharSelectCharState.DOING_RUN);
				break;
			case CharSelectCharState.DOING_RUN:
			{
				colliderObj.transform.position = FindNearestPointOnLine(startPos, startPos + visual.tf.forward * 2f, visual.root.position);
				AnimatorStateInfo currentAnimatorStateInfo = visual.anim.GetCurrentAnimatorStateInfo(0);
				if (currentAnimatorStateInfo.normalizedTime >= 1f && currentAnimatorStateInfo.shortNameHash == freeStyleHash)
				{
					CharSelect.SetState(CharacterSelect.CharSelectState.CYPHERING);
					SetState(CharSelectCharState.GOING_OUT);
				}
				break;
			}
			case CharSelectCharState.GOING_OUT:
				SetState(CharSelectCharState.BOUNCING);
				break;
			case CharSelectCharState.BOUNCING:
				break;
			}
		}

		private Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 end, Vector3 point)
		{
			Vector3 vector = end - origin;
			float magnitude = vector.magnitude;
			vector.Normalize();
			float value = Vector3.Dot(point - origin, vector);
			value = Mathf.Clamp(value, 0f, magnitude);
			return origin + vector * value;
		}

		public Vector3 GetLookAtPos()
		{
			return visual.head.position + Vector3.up * 0.125f;
		}

		public void StartSwapDialogue(PlayableDirector sequence)
		{
			SequenceHandler.instance.StartEnteringSequence(sequence, this, setHidePlayer: true, setInterruptPlayer: true, instantly: true, setPausePlayer: false, setAllowPhoneOnAfterSequence: true, null, skippable: false);
		}

		public void SetState(CharSelectCharState setState)
		{
			stateTimer = 0f;
			state = setState;
			bool flag = Random.value > 0.5f;
			switch (state)
			{
			case CharSelectCharState.JOINING:
				base.transform.rotation = startRot;
				waited = false;
				break;
			case CharSelectCharState.BOUNCING:
				base.transform.rotation = startRot;
				visual.anim.CrossFadeInFixedTime(visual.bounceAnimHash, 0.5f, -1, Random.Range(-0.03f, 0.03f) + (flag ? 0.5f : 0f));
				colliderObj.transform.position = tf.position;
				colliderObj.transform.localScale = new Vector3(2f, 1f, 1f);
				triggerObj.SetActive(value: true);
				break;
			case CharSelectCharState.GOING_IN:
				base.transform.rotation = startRot;
				triggerObj.SetActive(value: false);
				colliderObj.transform.position = tf.position;
				colliderObj.transform.localScale = new Vector3(1f, 1f, 1f);
				break;
			case CharSelectCharState.DOING_RUN:
				base.transform.rotation = startRot;
				triggerObj.SetActive(value: false);
				colliderObj.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
				visual.anim.CrossFadeInFixedTime(freeStyleHash, 0.5f);
				break;
			case CharSelectCharState.GOING_OUT:
				triggerObj.SetActive(value: true);
				colliderObj.transform.position = tf.position;
				colliderObj.transform.localScale = new Vector3(1f, 1f, 1f);
				break;
			case CharSelectCharState.TALKING:
				triggerObj.SetActive(value: true);
				colliderObj.transform.localScale = new Vector3(1f, 1f, 1f);
				colliderObj.transform.position = tf.position;
				base.transform.rotation = Quaternion.LookRotation(-(centerPos - tf.position).normalized);
				visual.anim.CrossFadeInFixedTime(visual.bounceAnimHash, 0.5f, -1, Random.Range(-0.03f, 0.03f) + (flag ? 0.5f : 0f));
				break;
			}
		}

		public void DoSwapConversation()
		{
			StartSwapDialogue(swapSequence);
			SetState(CharSelectCharState.TALKING);
		}

		public override void ExitSequence(PlayableDirector sequence, bool invokeEvent = true, bool restartCurrentEncounter = false)
		{
			if (invokeEvent)
			{
				CharSelect.SwapWithCharacter(base.gameObject);
			}
			else
			{
				SetState(CharSelectCharState.BOUNCING);
			}
			base.ExitSequence(sequence, invokeEvent);
			if (restartCurrentEncounter && WorldHandler.instance.currentEncounter != null)
			{
				WorldHandler.instance.currentEncounter.ActivateEncounter();
			}
		}
	}
}
