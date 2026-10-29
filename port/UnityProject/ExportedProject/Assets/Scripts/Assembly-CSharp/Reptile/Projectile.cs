using UnityEngine;

namespace Reptile
{
	public class Projectile : Bullet, IPoolable
	{
		[SerializeField]
		private float speed = 10f;

		[SerializeField]
		private LayerMask layerMaskRebound;

		[SerializeField]
		private GameObject trail;

		[SerializeField]
		protected GameObject bossImpactVFXPrefab;

		[SerializeField]
		protected GameObject reboundVFXPrefab;

		[SerializeField]
		protected SfxClip reboundSound;

		[SerializeField]
		protected SfxClip flyByLoopSound;

		[SerializeField]
		protected AudioSource oneShotAudioSource;

		[SerializeField]
		protected AudioSource loopingAudioSource;

		private PoolableObjectToName bossImpactEffect;

		private PoolableObjectToName reboundEffect;

		private const float maxFlyDistance = 250000f;

		private TrailRenderer[] trailsRenderers;

		private bool stuck;

		private bool hitPlayer;

		private bool rebound;

		private Player player;

		public new void InitPoolable(Vector3 position, Quaternion rotation)
		{
			if (state != PoolableState.Pooled)
			{
				return;
			}
			base.gameObject.SetActive(value: true);
			tf = base.transform;
			tf.position = position;
			tf.rotation = rotation;
			startPos = (prevPos = tf.position);
			player = WorldHandler.instance.GetCurrentPlayer();
			rebound = false;
			state = PoolableState.Active;
			stuck = false;
			hitPlayer = false;
			trail.SetActive(value: true);
			if (trailsRenderers == null)
			{
				trailsRenderers = GetComponentsInChildren<TrailRenderer>(includeInactive: true);
			}
			if (trailsRenderers != null)
			{
				for (int i = 0; i < trailsRenderers.Length; i++)
				{
					trailsRenderers[i].Clear();
				}
			}
			Core.OnUpdate += UpdateProjectile;
			if (playerImpactEffect == null)
			{
				playerImpactEffect = new PoolableObjectToName(playerImpactVFXPrefab, playerImpactVFXPrefab.name);
			}
			if (environmentImpactEffect == null)
			{
				environmentImpactEffect = new PoolableObjectToName(environmentImpactVFXPrefab, environmentImpactVFXPrefab.name);
			}
			if (bossImpactEffect == null)
			{
				bossImpactEffect = new PoolableObjectToName(bossImpactVFXPrefab, bossImpactVFXPrefab.name);
			}
			if (reboundEffect == null)
			{
				reboundEffect = new PoolableObjectToName(reboundVFXPrefab, reboundVFXPrefab.name);
			}
		}

		internal static void StopLoopingAudioSource(AudioSource audioSource)
		{
			if ((bool)audioSource)
			{
				Core.Instance.AudioManager.StopLoopingSfx(audioSource);
			}
		}

		private void UpdateProjectile()
		{
			if (state == PoolableState.Pooled)
			{
				return;
			}
			prevPos = tf.position;
			if (!stuck)
			{
				if (!player.IsInHitpause())
				{
					if ((bool)loopingAudioSource && !loopingAudioSource.isPlaying)
					{
						Core.Instance.AudioManager.PlaySfxGameplayLooping(flyByLoopSound, loopingAudioSource);
					}
					tf.position += tf.forward * speed * Core.dt;
					HitScan();
				}
				else
				{
					StopLoopingAudioSource(loopingAudioSource);
				}
			}
			else
			{
				StopLoopingAudioSource(loopingAudioSource);
				if (!WorldHandler.instance.IsReallyInScreen(tf.position))
				{
					Pool();
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.layer == 18 && !rebound && !stuck && !hitPlayer && (player.ability == player.groundTrickAbility || player.ability == player.airTrickAbility))
			{
				GetPoolableFromPoolOrInstantiate(reboundEffect).InitPoolable(Vector3.Lerp(base.transform.position, player.tf.position + Vector3.up, 0.5f), Quaternion.LookRotation(tf.forward));
				player.StartHitpause(0.3f);
				tf.forward = -tf.forward;
				rebound = true;
				if (oneShotAudioSource != null)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(reboundSound.collectionId, reboundSound.audioClipId, oneShotAudioSource);
				}
			}
		}

		private void HitScan()
		{
			if (Physics.Raycast(prevPos, tf.forward, out var hitInfo, speed * Core.dt, rebound ? layerMaskRebound : layerMask, QueryTriggerInteraction.Ignore))
			{
				if (hitInfo.collider.gameObject.layer == 17 && rebound)
				{
					DJBoss componentInParent = hitInfo.collider.transform.GetComponentInParent<DJBoss>();
					if (componentInParent != null)
					{
						GetPoolableFromPoolOrInstantiate(bossImpactEffect).InitPoolable(hitInfo.point, Quaternion.LookRotation(tf.forward));
						componentInParent.HitByVinyl();
						Pool();
					}
					DJBossJumper componentInParent2 = hitInfo.collider.transform.GetComponentInParent<DJBossJumper>();
					if (componentInParent2 != null)
					{
						GetPoolableFromPoolOrInstantiate(bossImpactEffect).InitPoolable(hitInfo.point, Quaternion.LookRotation(tf.forward));
						componentInParent2.HitByVinyl();
						Pool();
					}
				}
				if (hitInfo.collider.gameObject.layer == 9 && !hitPlayer)
				{
					Player component = hitInfo.collider.transform.parent.parent.GetComponent<Player>();
					GetPoolableFromPoolOrInstantiate(playerImpactEffect).InitPoolable(hitInfo.point, Quaternion.LookRotation(tf.forward));
					component.GetHit(damage, tf.forward * -1f, KnockbackAbility.KnockbackType.FAR);
					hitPlayer = true;
				}
				if (hitInfo.collider.gameObject.layer == 0 || GroundDetection.LayerIsNonStable(hitInfo.collider.gameObject.layer))
				{
					if (hitInfo.collider.gameObject.GetComponent<BreakableObject>() != null)
					{
						hitInfo.collider.gameObject.GetComponent<BreakableObject>().Break();
						return;
					}
					tf.position = hitInfo.point;
					stuck = true;
					trail.SetActive(value: false);
					GetPoolableFromPoolOrInstantiate(environmentImpactEffect).InitPoolable(hitInfo.point, Quaternion.LookRotation(Vector3.forward, hitInfo.normal));
					StopLoopingAudioSource(loopingAudioSource);
				}
			}
			else if ((prevPos - WorldHandler.instance.CurrentCameraPosition).sqrMagnitude >= 250000f)
			{
				Pool();
			}
		}

		public void OnDisable()
		{
			StopLoopingAudioSource(loopingAudioSource);
		}

		private void Pool()
		{
			StopLoopingAudioSource(loopingAudioSource);
			state = PoolableState.Pooled;
			base.gameObject.SetActive(value: false);
			Core.OnUpdate -= UpdateProjectile;
		}
	}
}
