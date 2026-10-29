using UnityEngine;

namespace Reptile
{
	public class Explosive : MonoBehaviour, IPoolable
	{
		[SerializeField]
		private ParticleSystem explosionParticleSystem;

		[SerializeField]
		private float countDownDuration = 2f;

		[SerializeField]
		private float radius = 3f;

		[SerializeField]
		private int playerDamage = 2;

		[SerializeField]
		private int enemyDamage = 2;

		protected const SfxCollectionID collectionID = SfxCollectionID.CombatSfx;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		protected AudioClipID explosionAudioClipID = AudioClipID.NONE;

		private PoolableState state = PoolableState.Pooled;

		private string poolableName;

		private float countDown;

		private Transform tf;

		private AudioSource audioSource;

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
				base.gameObject.SetActive(value: true);
				countDown = countDownDuration;
				if (countDown > 0f)
				{
					GetComponent<Animator>().Play("incomingMissile");
				}
				else
				{
					GetComponent<Animator>().Play("incomingMissileHidden");
				}
				audioSource = GetComponent<AudioSource>();
				state = PoolableState.Active;
			}
		}

		private void UpdateExplosive()
		{
			if (state == PoolableState.Pooled)
			{
				return;
			}
			if (countDown >= 0f)
			{
				countDown -= Core.dt;
				if (countDown <= 0f)
				{
					Explode();
				}
			}
			if (countDown <= 0f && !explosionParticleSystem.IsAlive())
			{
				Pool();
			}
		}

		private void Pool()
		{
			state = PoolableState.Pooled;
			base.gameObject.SetActive(value: false);
		}

		private void Explode()
		{
			if ((WorldHandler.instance.GetCurrentPlayer().tf.position - tf.position).sqrMagnitude < 2500f)
			{
				GameplayCamera.StartScreenShake(ScreenShakeType.HEAVY, 0.3f);
			}
			Vector3 vector = WorldHandler.instance.GetCurrentPlayer().tf.position - tf.position;
			if (vector.sqrMagnitude < radius * radius)
			{
				WorldHandler.instance.GetCurrentPlayer().GetHit(playerDamage, vector.normalized, KnockbackAbility.KnockbackType.BIG);
			}
			for (int i = 0; i < WorldHandler.instance.subscribedEnemies.Count; i++)
			{
				if (!(WorldHandler.instance.subscribedEnemies[i] != null))
				{
					continue;
				}
				EnemyHitResponse component = WorldHandler.instance.subscribedEnemies[i].GetComponent<EnemyHitResponse>();
				if (component != null)
				{
					vector = WorldHandler.instance.subscribedEnemies[i].tf.position - tf.position;
					if (vector.sqrMagnitude < radius * radius)
					{
						component.ManualDamage(EnemyHitResponse.HitType.EXPLOSION, vector.normalized, vector.magnitude, 2f, 3f, enemyDamage);
					}
				}
			}
			explosionParticleSystem.Play();
			Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, explosionAudioClipID, audioSource);
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
