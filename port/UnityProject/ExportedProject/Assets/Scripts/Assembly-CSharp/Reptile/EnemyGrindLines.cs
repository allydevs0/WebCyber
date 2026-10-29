using UnityEngine;

namespace Reptile
{
	public class EnemyGrindLines : MonoBehaviour
	{
		private bool beingGrindedByPlayer;

		private float playerOnBodyTimer;

		public bool BeingGrindedByPlayer
		{
			get
			{
				return beingGrindedByPlayer;
			}
			set
			{
				beingGrindedByPlayer = value;
			}
		}

		public bool PlayerActiveOnEnemy
		{
			get
			{
				if (!beingGrindedByPlayer)
				{
					return playerOnBodyTimer > 0f;
				}
				return true;
			}
		}

		public void Init()
		{
			Core.OnUpdate += PlayerOnBodyUpdate;
		}

		public virtual void OnDestroy()
		{
			Core.OnUpdate -= PlayerOnBodyUpdate;
		}

		private void PlayerOnBodyUpdate()
		{
			if (beingGrindedByPlayer)
			{
				playerOnBodyTimer = 2f;
			}
			else if (playerOnBodyTimer > 0f)
			{
				playerOnBodyTimer -= Core.dt;
			}
		}
	}
}
