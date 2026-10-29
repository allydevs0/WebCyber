using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(AudioSource))]
	public class StreetLife : MonoBehaviour
	{
		public enum IdleAnimation
		{
			pedIdleConfidence = 0,
			pedIdleCute = 1,
			pedIdleSideArm = 2,
			pedIdleUpset = 3,
			pedIdleStressed = 4,
			pedIdleSprayed = 5,
			pedIdleKnocking = 6,
			pedIdleSitPhone = 7,
			pedIdleSitLedge = 8,
			pedIdleWaiting = 9,
			pedIdleChill = 10,
			pedIdleListen = 11,
			pedIdlePhone = 12,
			pedIdlePhone2 = 13,
			pedIdleSquat = 14,
			pedIdleCheering = 15,
			pedIdleApplause = 16,
			pedIdleSitRelaxed = 17,
			pedIdleSitRelaxed2 = 18,
			pedIdleSitRelaxed3 = 19,
			pedIdleSitNewspaper = 20,
			pedIdleSitPhone2 = 21,
			pedIdleSitPhone3 = 22,
			pedIdleSitTense = 23,
			pedIdleTalk = 24,
			pedIdleTalk2 = 25,
			pedIdleWaiting2 = 26,
			pedIdlePockets = 27,
			birdIdle = 28,
			birdIdle2 = 29,
			birdIdle3 = 30,
			birdIdleSit = 31,
			birdIdleEat = 32,
			birdIdleGround = 33,
			dogIdle = 34,
			dogIdleSit = 35,
			dogIdleBark = 36,
			dogIdleLie = 37,
			dogIdleSleep = 38,
			pedIdleCheering2 = 39,
			pedIdleLeaning = 40,
			pedIdleLeaning2 = 41,
			pedIdleLeaning3 = 42,
			pedIdleLeaning4 = 43,
			pedIdleDrunkLeaning = 44,
			pedIdleDrunk = 45,
			pedSitDrunk = 46,
			pedSitGround = 47,
			pedSitGround2 = 48,
			pedSitGround3 = 49,
			catIdle = 50,
			catSit = 51,
			catLay = 52,
			catSleep = 53
		}

		public enum MoveAwayAnimation
		{
			pedMoveAway = 0,
			pedMoveAwayFall = 1,
			pedMoveAwayScared = 2,
			birdMoveAway = 3,
			dogMoveAway = 4,
			pedMoveAwaySpin = 5,
			catMoveAway = 6
		}

		public enum WalkAnimation
		{
			pedWalk = 0,
			pedWalkSassy = 1,
			pedWalkPhone = 2,
			pedWalkStrut = 3,
			pedWalkPockets = 4,
			pedWalkPockets2 = 5,
			pedWalkPhoneSassy = 6
		}

		internal StreetLifeCluster cluster;

		internal int indexIntoCluster;

		public IdleAnimation idleAnimation;

		public MoveAwayAnimation moveAwayAnimation;

		public WalkAnimation walkAnimation;

		public Junk attachedJunk;

		internal const SfxCollectionID sfxCollection = SfxCollectionID.StreetlifeSfx;

		public bool reportJostleAsCrime;

		public int streetlifeID = -1;

		internal AudioSource audioSource;

		internal SkinnedMeshRenderer skinnedMeshRenderer;

		internal float lastAudioTime;

		internal float lastJostleTime;

		internal bool isInitialized;

		public StreetLifeHot Init(StreetLifeCluster setCluster, StreetLifeBehaviour behaviour, int index)
		{
			cluster = setCluster;
			indexIntoCluster = index;
			skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
			audioSource = GetComponentInChildren<AudioSource>();
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.minDistance = 3f;
			audioSource.maxDistance = 15f;
			isInitialized = true;
			StreetLifeHot streetLifeHot = new StreetLifeHot
			{
				moveAwayDuration = 0.3f,
				moveAwaySpeed = 4.5f,
				verticalDir = Vector3.down,
				idleAnimation = Animator.StringToHash(idleAnimation.ToString()),
				moveAwayAnimation = Animator.StringToHash(moveAwayAnimation.ToString()),
				walkAnimation = Animator.StringToHash(walkAnimation.ToString()),
				attachedJunk = attachedJunk,
				transform = base.transform,
				startPos = base.transform.position,
				startRot = base.transform.rotation,
				trigger = GetComponent<SphereCollider>(),
				moveAlongPoints = GetComponent<MoveAlongPoints>()
			};
			if (streetLifeHot.moveAlongPoints != null)
			{
				streetLifeHot.activationGameObject = base.transform.parent.gameObject;
			}
			else
			{
				streetLifeHot.activationGameObject = base.gameObject;
			}
			if (streetlifeID == -1)
			{
				streetlifeID = GetIDBasedOnName(base.gameObject.name);
			}
			streetLifeHot.anim = GetComponent<Animator>();
			streetLifeHot = InitCustomValues(streetLifeHot);
			if (streetLifeHot.activationGameObject.activeInHierarchy)
			{
				behaviour.NormalAnim(ref streetLifeHot, randomOffset: true);
			}
			return streetLifeHot;
		}

		public virtual StreetLifeHot InitCustomValues(StreetLifeHot streetLifeHot)
		{
			streetLifeHot.verticalAcc = 60f;
			streetLifeHot.maxVerticalSpeed = 10f;
			return streetLifeHot;
		}

		public void OnTriggerStay(Collider other)
		{
			if (isInitialized)
			{
				if (other.gameObject.layer != 22)
				{
					cluster.StreetLifeMove(indexIntoCluster, this, other.transform);
				}
				if (other.gameObject.CompareTag("MovingObject"))
				{
					other.gameObject.GetComponentInParent<MoveAlongPoints>().TriggerDetectLayer(25);
				}
			}
		}

		public void PlayJostleSound()
		{
			lastAudioTime = Time.time;
			if (streetlifeID >= 0)
			{
				AudioClipID audioClipId = AudioClipID.bird;
				switch (streetlifeID)
				{
				case 0:
					audioClipId = AudioClipID.Crow;
					break;
				case 1:
					audioClipId = AudioClipID.bird;
					break;
				case 2:
					audioClipId = AudioClipID.Seagull;
					break;
				case 3:
				case 4:
				case 5:
				case 6:
					audioClipId = AudioClipID.cat;
					break;
				case 7:
				case 8:
					audioClipId = AudioClipID.dogBark;
					break;
				case 9:
					audioClipId = AudioClipID.femaleGasp;
					break;
				case 10:
					audioClipId = AudioClipID.femaleHey;
					break;
				case 11:
					audioClipId = AudioClipID.maleMoan;
					break;
				case 12:
					audioClipId = AudioClipID.maleBusiness;
					break;
				case 13:
					audioClipId = AudioClipID.maleBro;
					break;
				case 14:
					audioClipId = AudioClipID.maleScream;
					break;
				case 15:
					audioClipId = AudioClipID.maleGasp;
					break;
				case 16:
				case 17:
					audioClipId = AudioClipID.maleScream;
					break;
				case 18:
				case 19:
					audioClipId = AudioClipID.maleChill;
					break;
				case 20:
					audioClipId = AudioClipID.maleGasp;
					break;
				case 21:
					audioClipId = AudioClipID.femaleWatchIt;
					break;
				case 22:
					audioClipId = AudioClipID.femaleRobo;
					break;
				case 23:
				case 24:
					audioClipId = AudioClipID.femaleLookOut;
					break;
				case 25:
				case 26:
					audioClipId = AudioClipID.maleRobo;
					break;
				case 27:
				case 28:
				case 29:
				case 30:
					audioClipId = AudioClipID.maleBusiness;
					break;
				case 31:
				case 32:
				case 33:
					audioClipId = AudioClipID.maleDoeNormaal;
					break;
				case 34:
					audioClipId = AudioClipID.femaleRobo;
					break;
				}
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.StreetlifeSfx, audioClipId, audioSource);
			}
		}

		private int GetIDBasedOnName(string name)
		{
			if (name.Contains("ped"))
			{
				if (name.Contains("Bun"))
				{
					if (!name.Contains("1"))
					{
						return 10;
					}
					return 9;
				}
				if (name.Contains("Business"))
				{
					if (name.Contains("2"))
					{
						return 12;
					}
					return 11;
				}
				if (name.Contains("Cap"))
				{
					if (!name.Contains("1"))
					{
						return 14;
					}
					return 13;
				}
				if (name.Contains("Coat"))
				{
					return 15;
				}
				if (name.Contains("Janitor"))
				{
					if (!name.Contains("1"))
					{
						return 17;
					}
					return 16;
				}
				if (name.Contains("Lank"))
				{
					if (name.Contains("1"))
					{
						return 18;
					}
					if (name.Contains("2"))
					{
						return 19;
					}
					return 20;
				}
				if (name.Contains("Plus"))
				{
					if (!name.Contains("1"))
					{
						return 22;
					}
					return 21;
				}
				if (name.Contains("Racer"))
				{
					if (!name.Contains("1"))
					{
						return 24;
					}
					return 23;
				}
				if (name.Contains("Round"))
				{
					if (!name.Contains("1"))
					{
						return 26;
					}
					return 25;
				}
				if (name.Contains("Scientist"))
				{
					if (name.Contains("1"))
					{
						return 27;
					}
					if (name.Contains("2"))
					{
						return 28;
					}
					if (name.Contains("3"))
					{
						return 29;
					}
					return 30;
				}
				if (name.Contains("Worker"))
				{
					if (name.Contains("1"))
					{
						return 31;
					}
					if (name.Contains("2"))
					{
						return 32;
					}
					if (name.Contains("3"))
					{
						return 33;
					}
					return 34;
				}
			}
			else
			{
				if (name.Contains("bird"))
				{
					if (name.Contains("Crow"))
					{
						return 0;
					}
					if (name.Contains("Pigeon"))
					{
						return 1;
					}
					return 2;
				}
				if (name.Contains("cat"))
				{
					if (name.Contains("1"))
					{
						return 3;
					}
					if (name.Contains("2"))
					{
						return 4;
					}
					if (name.Contains("3"))
					{
						return 5;
					}
					return 6;
				}
				if (name.Contains("dog"))
				{
					if (name.Contains("Cool"))
					{
						return 8;
					}
					return 7;
				}
			}
			return -1;
		}

		private void OnDestroy()
		{
			cluster = null;
			attachedJunk = null;
			skinnedMeshRenderer = null;
		}
	}
}
