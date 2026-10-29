using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class EnemyFirearm : MonoBehaviour
	{
		[SerializeField]
		private string weaponName;

		[SerializeField]
		protected float fireInterval;

		[SerializeField]
		protected float reloadDuration;

		[SerializeField]
		protected int clipSize;

		[SerializeField]
		protected float minSpread;

		[SerializeField]
		protected float maxSpread;

		[SerializeField]
		protected float minAttackRange;

		[SerializeField]
		protected float desiredAttackRange;

		[SerializeField]
		protected float maxAttackRange;

		[SerializeField]
		protected GameObject bulletPrefab;

		[SerializeField]
		protected GameObject muzzleFlashPrefab;

		[SerializeField]
		protected GameObject visualRoot;

		[SerializeField]
		protected Transform muzzle;

		[SerializeField]
		protected AudioSource audioSourceGunFire;

		[SerializeField]
		protected AudioSource audioSourceReload;

		protected const SfxCollectionID collectionID = SfxCollectionID.CombatSfx;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		protected AudioClipID gunfireAudioClipID = AudioClipID.NONE;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		protected AudioClipID reloadAudioClipID = AudioClipID.NONE;

		protected List<IPoolable> poolables = new List<IPoolable>();

		protected PoolableObjectToName bullet;

		protected PoolableObjectToName muzzleFlash;

		protected AudioManager audioManager;

		protected EnemyPositioningInfo positioningInfo;

		protected float timeBetweenshots;

		protected float reloadTime;

		protected bool canShoot = true;

		protected int shotsFired;

		protected bool equipped = true;

		internal BasicEnemy enemy;

		public float FireInterval => fireInterval;

		public float ReloadDuration => reloadDuration;

		public int ClipSize
		{
			get
			{
				return clipSize;
			}
			set
			{
				clipSize = value;
			}
		}

		public bool CanShoot => canShoot;

		public bool Reloading => reloadTime > 0f;

		public float TimeReloading => reloadDuration - reloadTime;

		public float RemainingTimeBetweenShots => timeBetweenshots;

		public Transform Muzzle => muzzle;

		public float MaxAttackRange => maxAttackRange;

		public float MinAttackRange => minAttackRange;

		public float DesiredAttackRange => desiredAttackRange;

		public bool Equipped => equipped;

		public GameObject VisualRoot => visualRoot;

		public int ShotsFired => shotsFired;

		public void Init(BasicEnemy enemy, EnemyPositioningInfo positioning)
		{
			this.enemy = enemy;
			enemy.OnUpdateActive.AddListener(UpdateFirearm);
			audioManager = Core.Instance.AudioManager;
			if (bullet == null)
			{
				bullet = new PoolableObjectToName(bulletPrefab, bulletPrefab.name);
			}
			if (muzzleFlashPrefab != null && muzzleFlash == null)
			{
				muzzleFlash = new PoolableObjectToName(muzzleFlashPrefab, muzzleFlashPrefab.name);
			}
			positioningInfo = positioning;
		}

		public void Equip(bool equip)
		{
			equipped = equip;
			if (visualRoot != null)
			{
				visualRoot.SetActive(equip);
			}
		}

		public virtual void UpdateFirearm()
		{
			if (reloadTime > 0f)
			{
				reloadTime -= Core.dt;
			}
			if (timeBetweenshots > 0f)
			{
				timeBetweenshots -= Core.dt;
			}
			canShoot = reloadTime <= 0f && timeBetweenshots <= 0f && equipped;
		}

		public virtual void Shoot(Vector3 shootDirection, float spread)
		{
			Shoot(muzzle.position, shootDirection, spread);
		}

		public virtual void Shoot(Vector3 startPosition, Vector3 shootDirection, float spread)
		{
			if (!canShoot)
			{
				return;
			}
			if (muzzleFlashPrefab != null)
			{
				GetPoolableFromPoolOrInstantiate(muzzleFlash).InitPoolable(muzzle.position, muzzle.rotation);
			}
			GetPoolableFromPoolOrInstantiate(bullet).InitPoolable(startPosition, Quaternion.LookRotation(shootDirection) * Quaternion.Euler(GetSpread(spread)));
			if (audioSourceGunFire != null && gunfireAudioClipID != AudioClipID.NONE)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, gunfireAudioClipID, audioSourceGunFire, 0.05f);
			}
			shotsFired++;
			canShoot = false;
			if (shotsFired < clipSize)
			{
				timeBetweenshots = fireInterval;
				return;
			}
			if (audioSourceReload != null && reloadAudioClipID != AudioClipID.NONE)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, reloadAudioClipID, audioSourceReload);
			}
			reloadTime = reloadDuration;
			shotsFired = 0;
		}

		public void ReloadManual()
		{
			shotsFired = 0;
			reloadTime = reloadDuration;
		}

		public bool IsInRangeFlat()
		{
			if (positioningInfo.SqrFlatDistToPlayer < MaxAttackRange * MaxAttackRange)
			{
				return positioningInfo.SqrFlatDistToPlayer > MinAttackRange * MinAttackRange;
			}
			return false;
		}

		public bool IsInRange()
		{
			if (positioningInfo.SqrDistToPlayer < MaxAttackRange * MaxAttackRange)
			{
				return positioningInfo.SqrDistToPlayer > MinAttackRange * MinAttackRange;
			}
			return false;
		}

		public void SetMuzzle(Transform newMuzzle)
		{
			muzzle = newMuzzle;
		}

		protected Vector3 GetSpread(float spread)
		{
			float num = Mathf.Lerp(minSpread, maxSpread, spread);
			return new Vector3(Mathf.Sin(Time.time * 2f), Mathf.Cos(Time.time * 2f), Mathf.Sin(Time.time * 2f)) * num;
		}

		protected IPoolable GetPoolableFromPoolOrInstantiate(PoolableObjectToName poolableObject)
		{
			IPoolable poolable = null;
			for (int i = 0; i < poolables.Count; i++)
			{
				if (poolables[i].PoolableName == poolableObject.name && poolables[i].State == PoolableState.Pooled)
				{
					return poolables[i];
				}
			}
			if (poolable == null)
			{
				poolable = Object.Instantiate(poolableObject.obj).GetComponent<IPoolable>();
				poolable.PoolableName = poolableObject.name;
				poolables.Add(poolable);
			}
			return poolable;
		}
	}
}
