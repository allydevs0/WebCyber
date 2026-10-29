using UnityEngine;
using UnityEngine.Events;

namespace Reptile
{
	public class VendingMachine : MonoBehaviour
	{
		private enum State
		{
			NONE = 0,
			SHAKE = 1,
			EMPTY_SHAKE = 2,
			DROP = 3
		}

		public enum Reward
		{
			NOTHING = 0,
			SINGLE_NORMAL_DROP = 1,
			BUNCH_OF_SPECIAL_DROPS = 2,
			PILE_OF_NORMAL_DROPS = 3,
			UNLOCKABLE_DROP = 4
		}

		private Animation anim;

		private State state;

		private bool firstHit = true;

		private AudioSource audioSource;

		private AudioManager audioManager;

		private BoxCollider collider;

		public UnityEvent OnFirstHit;

		public Reward[] rewards;

		public float singleDropForce;

		public float bunchDropForce = 0.5f;

		public float pileDropForce = 3.5f;

		private int dropCount;

		private float dropTimer;

		private float stateTimer;

		private int rewardCount;

		public GameObject normalDrop;

		public GameObject specialDrop;

		public GameObject unlockableDrop;

		public Transform dropPoint;

		public void Start()
		{
			anim = GetComponent<Animation>();
			Core.OnUpdate += MachineUpdate;
			audioSource = GetComponent<AudioSource>();
			audioManager = Core.Instance.AudioManager;
			collider = GetComponent<BoxCollider>();
		}

		private void MachineUpdate()
		{
			if (state == State.SHAKE)
			{
				stateTimer += Core.dt;
				if (stateTimer > 1.7f)
				{
					SetState(State.DROP);
				}
			}
			else if (state == State.DROP)
			{
				stateTimer += Core.dt;
				dropTimer -= Core.dt;
				DropRewards();
			}
			else if (state == State.EMPTY_SHAKE)
			{
				stateTimer += Core.dt;
				if (stateTimer > 1f)
				{
					rewardCount++;
					CheckFirstHit();
					SetState(State.NONE);
				}
			}
		}

		private void SetState(State setState)
		{
			state = setState;
			stateTimer = 0f;
			base.transform.localPosition = Vector3.zero;
			dropCount = 0;
			switch (state)
			{
			case State.NONE:
				audioManager.StopLoopingSfx(audioSource);
				break;
			case State.SHAKE:
				if ((bool)anim)
				{
					anim.Play("shake");
				}
				if ((bool)audioSource)
				{
					audioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.vendingMachine, audioSource);
				}
				break;
			case State.EMPTY_SHAKE:
				if ((bool)anim)
				{
					anim.Play("emptyShake");
				}
				if ((bool)audioSource)
				{
					audioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, AudioClipID.vendingMachine_short, audioSource);
				}
				break;
			case State.DROP:
				if ((bool)anim)
				{
					anim.Play("drop");
				}
				if ((bool)audioSource)
				{
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.EnvironmentSfx, AudioClipID.vendingMachine_drop, audioSource);
				}
				dropTimer = 0f;
				CheckFirstHit();
				if (rewards.Length != 0)
				{
					if (rewards[rewardCount] == Reward.SINGLE_NORMAL_DROP)
					{
						Object.Instantiate(normalDrop, dropPoint.position, Random.rotation).GetComponent<Rigidbody>().AddForce(dropPoint.forward * singleDropForce, ForceMode.Impulse);
					}
					else if (rewards[rewardCount] == Reward.UNLOCKABLE_DROP)
					{
						Object.Instantiate(unlockableDrop, dropPoint.position, base.transform.rotation).GetComponent<Rigidbody>().AddForce(dropPoint.forward * singleDropForce, ForceMode.Impulse);
					}
				}
				break;
			}
		}

		private void CheckFirstHit()
		{
			if (firstHit)
			{
				OnFirstHit.Invoke();
			}
			firstHit = false;
		}

		public void DropRewards()
		{
			if (rewards.Length == 0)
			{
				return;
			}
			switch (rewards[rewardCount])
			{
			case Reward.PILE_OF_NORMAL_DROPS:
				if (dropCount < 36 && dropTimer <= 0f)
				{
					dropCount++;
					Rigidbody component = Object.Instantiate(normalDrop, dropPoint.position + dropPoint.right * Random.value * 0.35f, Random.rotation).GetComponent<Rigidbody>();
					component.AddForce(dropPoint.forward * pileDropForce + Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
					float num = (component.maxAngularVelocity = 20f);
					component.AddTorque(Random.rotation * Vector3.forward * num, ForceMode.Impulse);
					dropTimer = 0.17f;
				}
				if (stateTimer > 6.5f)
				{
					rewardCount++;
					SetState(State.NONE);
					anim.Play("close");
				}
				break;
			case Reward.BUNCH_OF_SPECIAL_DROPS:
				if (dropCount < 3 && dropTimer <= 0f)
				{
					dropCount++;
					Object.Instantiate(specialDrop, dropPoint.position + dropPoint.right * Random.value * 0.35f, Random.rotation).GetComponent<Rigidbody>().AddForce(dropPoint.forward * bunchDropForce, ForceMode.Impulse);
					dropTimer = 0.33f;
				}
				if (stateTimer > 1.4f)
				{
					rewardCount++;
					SetState(State.NONE);
					anim.Play("close");
				}
				break;
			default:
				if (stateTimer > 0.5f)
				{
					rewardCount++;
					SetState(State.NONE);
					anim.Play("close");
				}
				break;
			}
		}

		public bool RewardIsValid(int num)
		{
			bool result = true;
			if (rewards[num] == Reward.UNLOCKABLE_DROP)
			{
				result = !Core.Instance.Platform.User.GetUnlockableSaveDataFor(unlockableDrop.GetComponent<Pickup>().unlock).IsUnlocked;
			}
			return result;
		}

		public void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.layer != 18)
			{
				return;
			}
			Player componentInParent = other.transform.GetComponentInParent<Player>();
			if (componentInParent.ability != componentInParent.hitBounceAbility && state == State.NONE && componentInParent.hitBounceAbility.Trigger(base.transform.position, collider))
			{
				if (rewards.Length > rewardCount && RewardIsValid(rewardCount))
				{
					SetState(State.SHAKE);
				}
				else
				{
					SetState(State.EMPTY_SHAKE);
				}
			}
		}

		public void OnDestroy()
		{
			Core.OnUpdate -= MachineUpdate;
		}
	}
}
