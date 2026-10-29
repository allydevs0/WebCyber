using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class CuffMissileLauncher : EnemyFirearm
	{
		internal List<Transform> activeMissiles = new List<Transform>();

		public override void UpdateFirearm()
		{
			base.UpdateFirearm();
		}

		public override void Shoot(Vector3 startPosition, Vector3 shootDirection, float spread)
		{
			if (!canShoot)
			{
				return;
			}
			if (muzzleFlashPrefab != null)
			{
				GetPoolableFromPoolOrInstantiate(muzzleFlash).InitPoolable(muzzle.position, muzzle.rotation);
			}
			IPoolable poolableFromPoolOrInstantiate = GetPoolableFromPoolOrInstantiate(bullet);
			poolableFromPoolOrInstantiate.InitPoolable(startPosition, Quaternion.LookRotation(shootDirection) * Quaternion.Euler(GetSpread(spread)));
			CuffMissileEnemy cuffMissileEnemy = (CuffMissileEnemy)poolableFromPoolOrInstantiate;
			cuffMissileEnemy.Init(this, muzzle);
			activeMissiles.Add(cuffMissileEnemy.transform);
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

		public void ClearMissiles()
		{
			if (activeMissiles.Count > 0)
			{
				for (int num = activeMissiles.Count - 1; num >= 0; num--)
				{
					activeMissiles[num].GetComponent<CuffMissileEnemy>().SetState(CuffMissileEnemy.CuffMissileState.RETURNING);
				}
			}
			WorldHandler.instance.GetCurrentPlayer().RemoveAllCuffs(this);
		}
	}
}
