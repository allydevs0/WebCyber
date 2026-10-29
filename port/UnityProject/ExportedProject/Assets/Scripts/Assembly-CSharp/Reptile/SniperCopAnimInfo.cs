using UnityEngine;

namespace Reptile
{
	public class SniperCopAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			int key = Animator.StringToHash("idleAgro");
			int key2 = Animator.StringToHash("moveRun");
			int key3 = Animator.StringToHash("moveShuffle");
			int key4 = Animator.StringToHash("vaultFence");
			int key5 = Animator.StringToHash("getUp");
			new AnimInfo(target, "idleTube");
			new AnimInfo(target, "idleAgro")
			{
				layer = 0,
				crossFades = 
				{
					{ key3, 0.2f },
					{ key2, 0.2f },
					{ key5, 0.2f },
					{ key4, 0.15f }
				}
			};
			new AnimInfo(target, "idleRunAway").layer = 0;
			new AnimInfo(target, "moveShuffle")
			{
				layer = 0,
				crossFades = 
				{
					{ key, 0.2f },
					{ key2, 0.2f },
					{ key5, 0.2f },
					{ key4, 0.15f }
				}
			};
			new AnimInfo(target, "moveRun")
			{
				layer = 0,
				crossFades = 
				{
					{ key, 0.2f },
					{ key3, 0.2f },
					{ key5, 0.2f },
					{ key4, 0.15f }
				}
			};
			new AnimInfo(target, "runAway").layer = 0;
			new AnimInfo(target, "runAwayB").layer = 0;
			new AnimInfo(target, "vaultFence")
			{
				layer = 0,
				crossFades = 
				{
					{ key3, 0.15f },
					{ key2, 0.15f }
				}
			};
			new AnimInfo(target, "jumpDown").layer = 0;
			new AnimInfo(target, "jumpDownLand")
			{
				layer = 0,
				crossFades = 
				{
					{ key3, 0.15f },
					{ key2, 0.15f }
				}
			};
			int key6 = Animator.StringToHash("getHitA");
			int key7 = Animator.StringToHash("getHitB");
			new AnimInfo(target, "getHitSpray");
			int key8 = Animator.StringToHash("getHitFlyBack");
			int key9 = Animator.StringToHash("getHitLyingDown");
			Animator.StringToHash("getHitJuggle");
			Animator.StringToHash("land");
			Animator.StringToHash("lieDown");
			new AnimInfo(target, "getHitA")
			{
				layer = 0,
				crossFades = { { key7, 0.2f } }
			};
			new AnimInfo(target, "getHitB")
			{
				layer = 0,
				crossFades = { { key6, 0.2f } }
			};
			new AnimInfo(target, "getHitJuggle").layer = 0;
			new AnimInfo(target, "getHitFlyBack").layer = 0;
			new AnimInfo(target, "getHitLyingDown").layer = 0;
			new AnimInfo(target, "land").layer = 0;
			new AnimInfo(target, "lieDown")
			{
				layer = 0,
				crossFades = { { key9, 0.2f } }
			};
			new AnimInfo(target, "getUp")
			{
				layer = 0,
				crossFades = 
				{
					{ key8, 0.2f },
					{ key9, 0.2f }
				}
			};
			int key10 = Animator.StringToHash("gunLowered");
			int key11 = Animator.StringToHash("gunReload");
			int key12 = Animator.StringToHash("gunAim");
			new AnimInfo(target, "gunLowered")
			{
				layer = 1,
				crossFades = 
				{
					{ key12, 0.5f },
					{ key11, 0.2f }
				}
			};
			new AnimInfo(target, "gunAim")
			{
				layer = 1,
				crossFades = 
				{
					{ key10, 0.5f },
					{ key11, 0.2f }
				}
			};
			new AnimInfo(target, "gunShoot").layer = 1;
			new AnimInfo(target, "gunReload")
			{
				layer = 1,
				crossFades = { { key12, 0.5f } }
			};
			new AnimInfo(target, "moveDirectionX");
			new AnimInfo(target, "moveDirectionY");
			new AnimInfo(target, "aimAngle");
			new AnimInfo(target, "fallAngle");
		}
	}
}
