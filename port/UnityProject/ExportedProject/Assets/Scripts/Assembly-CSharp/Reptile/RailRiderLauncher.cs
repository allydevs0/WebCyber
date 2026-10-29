using UnityEngine;

namespace Reptile
{
	public class RailRiderLauncher : EnemyFirearm
	{
		private GrindPath currentGrindPath;

		public override void UpdateFirearm()
		{
			base.UpdateFirearm();
		}

		public void SetGrindPath(GrindPath path)
		{
			currentGrindPath = path;
		}

		public override void Shoot(Vector3 startPosition, Vector3 shootDirection, float spread)
		{
			if (canShoot)
			{
				if (muzzleFlashPrefab != null)
				{
					GetPoolableFromPoolOrInstantiate(muzzleFlash).InitPoolable(muzzle.position, muzzle.rotation);
				}
				IPoolable poolableFromPoolOrInstantiate = GetPoolableFromPoolOrInstantiate(bullet);
				poolableFromPoolOrInstantiate.InitPoolable(startPosition, Quaternion.LookRotation(shootDirection, Vector3.up) * Quaternion.Euler(GetSpread(spread)));
				((RailRider)poolableFromPoolOrInstantiate).Init(currentGrindPath);
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, gunfireAudioClipID, audioSourceGunFire, 0.05f);
				shotsFired++;
				canShoot = false;
				if (shotsFired < clipSize)
				{
					timeBetweenshots = fireInterval;
					return;
				}
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, reloadAudioClipID, audioSourceReload);
				reloadTime = reloadDuration;
				shotsFired = 0;
			}
		}
	}
}
