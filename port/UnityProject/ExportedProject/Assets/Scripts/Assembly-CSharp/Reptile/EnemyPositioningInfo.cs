using UnityEngine;

namespace Reptile
{
	public class EnemyPositioningInfo : MonoBehaviour
	{
		private Player player;

		private BasicEnemy enemy;

		private bool diffToPlayerSet;

		private Vector3 diffToPlayer;

		private bool dirToPlayerSet;

		private Vector3 dirToPlayer;

		private bool distToPlayerSet;

		private float distToPlayer;

		private bool sqrDistToPlayerSet;

		private float sqrDistToPlayer;

		private bool sqrFlatDistToPlayerSet;

		private float sqrFlatDistToPlayer;

		private bool flatDirToPlayerSet;

		private Vector3 flatDirToPlayer;

		public Vector3 DiffToPlayer
		{
			get
			{
				if (diffToPlayerSet)
				{
					return diffToPlayer;
				}
				return CalcDiffToPlayer();
			}
		}

		public Vector3 DirToPlayer
		{
			get
			{
				if (dirToPlayerSet)
				{
					return dirToPlayer;
				}
				return CalcDirToPlayer();
			}
		}

		public float DistToPlayer
		{
			get
			{
				if (distToPlayerSet)
				{
					return distToPlayer;
				}
				return CalcDistToPlayer();
			}
		}

		public float SqrDistToPlayer
		{
			get
			{
				if (sqrDistToPlayerSet)
				{
					return sqrDistToPlayer;
				}
				return CalcSqrDistToPlayer();
			}
		}

		public float SqrFlatDistToPlayer
		{
			get
			{
				if (sqrFlatDistToPlayerSet)
				{
					return sqrFlatDistToPlayer;
				}
				return CalcSqrFlatDistToPlayer();
			}
		}

		public Vector3 FlatDirToPlayer
		{
			get
			{
				if (flatDirToPlayerSet)
				{
					return flatDirToPlayer;
				}
				return CalcFlatDirToPlayer();
			}
		}

		public void Init(Player targetPlayer, BasicEnemy targetEnemy)
		{
			player = targetPlayer;
			enemy = targetEnemy;
			Core.OnLateUpdate += LateUpdateInfo;
		}

		public void OnDestroy()
		{
			Core.OnLateUpdate -= LateUpdateInfo;
		}

		private Vector3 CalcDiffToPlayer()
		{
			diffToPlayer = player.tf.position - enemy.tf.position;
			diffToPlayerSet = true;
			return diffToPlayer;
		}

		private Vector3 CalcDirToPlayer()
		{
			dirToPlayer = DiffToPlayer.normalized;
			dirToPlayerSet = true;
			return dirToPlayer;
		}

		private Vector3 CalcFlatDirToPlayer()
		{
			flatDirToPlayer = DirToPlayer;
			flatDirToPlayer.y = 0f;
			flatDirToPlayer.Normalize();
			flatDirToPlayerSet = true;
			return flatDirToPlayer;
		}

		private float CalcDistToPlayer()
		{
			distToPlayer = DiffToPlayer.magnitude;
			distToPlayerSet = true;
			return distToPlayer;
		}

		private float CalcSqrDistToPlayer()
		{
			sqrDistToPlayer = DiffToPlayer.sqrMagnitude;
			sqrDistToPlayerSet = true;
			return sqrDistToPlayer;
		}

		private float CalcSqrFlatDistToPlayer()
		{
			Vector3 vector = DiffToPlayer;
			vector.y = 0f;
			sqrFlatDistToPlayer = vector.sqrMagnitude;
			sqrFlatDistToPlayerSet = true;
			return sqrFlatDistToPlayer;
		}

		private void LateUpdateInfo()
		{
			diffToPlayerSet = (dirToPlayerSet = (distToPlayerSet = (sqrDistToPlayerSet = (flatDirToPlayerSet = (sqrFlatDistToPlayerSet = false)))));
		}
	}
}
