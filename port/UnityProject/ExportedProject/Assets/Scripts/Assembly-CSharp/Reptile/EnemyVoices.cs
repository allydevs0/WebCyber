using UnityEngine;

namespace Reptile
{
	public class EnemyVoices : MonoBehaviour
	{
		protected const SfxCollectionID collectionID = SfxCollectionID.CombatSfx;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID hurtVO;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID deathVO;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID attacKVO;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID blockVO;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID engageVO;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		private AudioClipID spawnVO;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private float chanceToPlayHurtAudio = 0.5f;

		[SerializeField]
		private float chanceToPlayDeathAudio = 1f;

		[SerializeField]
		private float chanceToPlayAttackAudio = 1f;

		[SerializeField]
		private float chanceToPlayBlockAudio = 1f;

		[SerializeField]
		private float chanceToPlayEngageAudio = 1f;

		[SerializeField]
		private float chanceToPlaySpawnAudio = 1f;

		private AudioManager audioManager;

		public void Init()
		{
			audioManager = Core.Instance.AudioManager;
		}

		public void PlayHurt()
		{
			if (Random.value < chanceToPlayHurtAudio)
			{
				audioSource.Stop();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, hurtVO, audioSource);
			}
		}

		public void PlayDeath()
		{
			if (Random.value < chanceToPlayDeathAudio)
			{
				audioSource.Stop();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, deathVO, audioSource);
			}
		}

		public void PlayAttack()
		{
			if (Random.value < chanceToPlayAttackAudio)
			{
				audioSource.Stop();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, attacKVO, audioSource);
			}
		}

		public void PlayBlock()
		{
			if (Random.value < chanceToPlayBlockAudio)
			{
				audioSource.Stop();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, blockVO, audioSource);
			}
		}

		public void PlayEngage()
		{
			if (Random.value < chanceToPlayEngageAudio)
			{
				audioSource.Stop();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, engageVO, audioSource);
			}
		}

		public void PlaySpawn()
		{
			if (Random.value < chanceToPlaySpawnAudio)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, spawnVO, audioSource);
			}
		}
	}
}
