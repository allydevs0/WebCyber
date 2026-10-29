using System;
using UnityEngine;

namespace Reptile
{
	public class ProximityMine : MonoBehaviour, IPoolable
	{
		public enum MineState
		{
			MOVING = 0,
			WAITING = 1,
			DELAYEDTRIGGER = 2,
			EXPLODED = 3
		}

		[SerializeField]
		private float distanceFromPlayer;

		[SerializeField]
		private int playerDamage;

		[SerializeField]
		private float lifeTime = 10f;

		[SerializeField]
		private ParticleSystem explosionParticleSystem;

		[SerializeField]
		private MeshRenderer proximityRenderer;

		[SerializeField]
		private GameObject mainVisual;

		[SerializeField]
		private float speed = 10f;

		[SerializeField]
		private LayerMask layerMask;

		[SerializeField]
		private SfxClip explosionSound;

		[SerializeField]
		private SfxClip proximitySound;

		private PoolableState state = PoolableState.Pooled;

		private string poolableName;

		private float countDown;

		private bool exploded;

		private Transform tf;

		private AudioSource audioSource;

		private Player player;

		private float remainingLife;

		private bool delayedTrigger;

		private MineState mineState;

		private AudioManager audioManager;

		private Vector3 targetPos;

		private Vector3 targetUp;

		private Vector3 startPos;

		private float dist;

		private Vector3 prevPos = Vector3.zero;

		private float stateTime;

		public PoolableState State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
			}
		}

		public string PoolableName
		{
			get
			{
				return poolableName;
			}
			set
			{
				poolableName = value;
			}
		}

		public void InitPoolable(Vector3 position, Quaternion rotation)
		{
			if (state == PoolableState.Pooled)
			{
				tf = base.transform;
				tf.position = position;
				tf.rotation = rotation;
				startPos = tf.position;
				base.gameObject.SetActive(value: true);
				remainingLife = lifeTime;
				delayedTrigger = false;
				exploded = false;
				player = WorldHandler.instance.GetCurrentPlayer();
				audioSource = GetComponent<AudioSource>();
				state = PoolableState.Active;
				proximityRenderer.enabled = false;
				audioManager = Core.Instance.AudioManager;
				if (Physics.Raycast(tf.position, tf.forward, out var hitInfo, 100f, layerMask, QueryTriggerInteraction.Ignore))
				{
					targetPos = hitInfo.point;
					targetUp = hitInfo.normal;
					dist = Vector3.Distance(targetPos, startPos);
				}
				else
				{
					Pool();
				}
				SetState(MineState.MOVING);
			}
		}

		public void SetTargetPos(Vector3 targetPosition, Vector3 targetNormal)
		{
			targetPos = targetPos;
			targetUp = targetNormal;
			dist = Vector3.Distance(targetPos, startPos);
		}

		private void SetState(MineState targetState)
		{
			stateTime = 0f;
			mineState = targetState;
			switch (mineState)
			{
			case MineState.MOVING:
				proximityRenderer.enabled = false;
				mainVisual.SetActive(value: true);
				break;
			case MineState.WAITING:
				proximityRenderer.enabled = true;
				mainVisual.SetActive(value: true);
				tf.position = targetPos;
				tf.up = targetUp;
				break;
			case MineState.DELAYEDTRIGGER:
				proximityRenderer.enabled = true;
				mainVisual.SetActive(value: true);
				break;
			case MineState.EXPLODED:
				Explode();
				proximityRenderer.enabled = false;
				mainVisual.SetActive(value: false);
				break;
			}
		}

		public static Vector3 EaseInSine(Vector3 start, Vector3 end, float value)
		{
			end -= start;
			return -end * Mathf.Cos(value * (MathF.PI / 2f)) + end + start;
		}

		private void UpdateExplosive()
		{
			if (state == PoolableState.Pooled)
			{
				return;
			}
			stateTime += Core.dt;
			switch (mineState)
			{
			case MineState.MOVING:
			{
				prevPos = tf.position;
				Vector3 vector = Vector3.Lerp(startPos, targetPos, stateTime / (dist / speed));
				Vector3 vector2 = EaseInSine(startPos, targetPos, stateTime / (dist / speed));
				tf.position = new Vector3(vector.x, vector2.y, vector.z);
				tf.up = (prevPos - tf.position).normalized;
				if (stateTime > dist / speed)
				{
					SetState(MineState.WAITING);
				}
				break;
			}
			case MineState.WAITING:
			{
				proximityRenderer.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 4f, stateTime * 2f);
				bool flag = proximityRenderer.enabled;
				bool flag2 = stateTime % 0.5f < 0.35f;
				proximityRenderer.enabled = flag2;
				if (((bool)audioSource & flag2) && flag != flag2)
				{
					audioManager.PlaySfxGameplay(proximitySound, audioSource);
				}
				if (Vector3.Distance(tf.position, player.tf.position) < distanceFromPlayer * 3f)
				{
					SetState(MineState.DELAYEDTRIGGER);
				}
				if (Vector3.Distance(tf.position, player.tf.position) < distanceFromPlayer)
				{
					SetState(MineState.EXPLODED);
				}
				if (stateTime > 10f && !WorldHandler.instance.IsReallyInScreen(tf.position))
				{
					Pool();
				}
				break;
			}
			case MineState.DELAYEDTRIGGER:
			{
				bool flag3 = proximityRenderer.enabled;
				bool flag4 = stateTime % 0.15f < 0.075f;
				proximityRenderer.enabled = flag4;
				if (((bool)audioSource & flag4) && flag3 != flag4)
				{
					audioManager.PlaySfxGameplay(proximitySound, audioSource);
				}
				if (stateTime > lifeTime)
				{
					SetState(MineState.EXPLODED);
				}
				break;
			}
			case MineState.EXPLODED:
				if (!explosionParticleSystem.IsAlive())
				{
					Pool();
				}
				break;
			}
		}

		private void Pool()
		{
			state = PoolableState.Pooled;
			base.gameObject.SetActive(value: false);
		}

		private void Explode()
		{
			explosionParticleSystem.Play();
			exploded = true;
			if (audioSource != null)
			{
				audioManager.PlaySfxGameplay(explosionSound.collectionId, explosionSound.audioClipId, audioSource);
			}
			float num = Vector3.Distance(tf.position, player.tf.position);
			if (num < distanceFromPlayer * 2f)
			{
				GameplayCamera.StartScreenShake(ScreenShakeType.HEAVY, 0.3f);
			}
			if (num < distanceFromPlayer)
			{
				player.GetHit(playerDamage, (player.tf.position - tf.position).normalized, KnockbackAbility.KnockbackType.BIG);
			}
		}

		private void OnEnable()
		{
			Core.OnUpdate += UpdateExplosive;
		}

		private void OnDisable()
		{
			Core.OnUpdate -= UpdateExplosive;
		}
	}
}
