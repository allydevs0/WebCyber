using UnityEngine;

namespace Reptile
{
	public class BasicCopAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			int key = Animator.StringToHash("idleTube");
			int key2 = Animator.StringToHash("idleAgro");
			int key3 = Animator.StringToHash("moveRun");
			Animator.StringToHash("runAway");
			int key4 = Animator.StringToHash("moveShuffle");
			int key5 = Animator.StringToHash("vaultFence");
			int key6 = Animator.StringToHash("jumpDown");
			int key7 = Animator.StringToHash("getUp");
			int key8 = Animator.StringToHash("attackMeleeA");
			int key9 = Animator.StringToHash("attackMeleeB");
			int key10 = Animator.StringToHash("getHitA");
			int key11 = Animator.StringToHash("getHitB");
			int key12 = Animator.StringToHash("getHitFlyBack");
			int key13 = Animator.StringToHash("getHitLyingDown");
			Animator.StringToHash("getHitJuggle");
			Animator.StringToHash("land");
			Animator.StringToHash("lieDown");
			int key14 = Animator.StringToHash("gunLowered");
			int key15 = Animator.StringToHash("gunLower");
			int key16 = Animator.StringToHash("gunReload");
			int key17 = Animator.StringToHash("gunAim");
			new AnimInfo(target, "idleTube");
			new AnimInfo(target, "idleRunAway")
			{
				layer = 0,
				crossFades = { { key7, 0.2f } }
			};
			new AnimInfo(target, "idleAgro")
			{
				layer = 0,
				crossFades = 
				{
					{ key, 0.15f },
					{ key4, 0.15f },
					{ key3, 0.15f },
					{ key7, 0.2f },
					{ key8, 0.2f },
					{ key9, 0.2f },
					{ key10, 0.2f },
					{ key11, 0.2f },
					{ key5, 0.15f },
					{ key6, 0.15f }
				}
			};
			new AnimInfo(target, "moveShuffle")
			{
				layer = 0,
				crossFades = 
				{
					{ key, 0.15f },
					{ key2, 0.15f },
					{ key3, 0.15f },
					{ key7, 0.2f },
					{ key8, 0.2f },
					{ key9, 0.2f },
					{ key10, 0.2f },
					{ key11, 0.2f },
					{ key5, 0.15f },
					{ key6, 0.15f }
				}
			};
			new AnimInfo(target, "moveRun")
			{
				layer = 0,
				crossFades = 
				{
					{ key, 0.15f },
					{ key2, 0.15f },
					{ key4, 0.15f },
					{ key7, 0.2f },
					{ key8, 0.2f },
					{ key9, 0.2f },
					{ key10, 0.2f },
					{ key11, 0.2f },
					{ key5, 0.15f },
					{ key6, 0.15f }
				}
			};
			new AnimInfo(target, "runAway")
			{
				layer = 0,
				crossFades = { { key7, 0.2f } }
			};
			new AnimInfo(target, "runAwayB")
			{
				layer = 0,
				crossFades = { { key7, 0.2f } }
			};
			new AnimInfo(target, "vaultFence")
			{
				layer = 0,
				crossFades = 
				{
					{ key4, 0.15f },
					{ key3, 0.15f }
				}
			};
			new AnimInfo(target, "jumpDown").layer = 0;
			new AnimInfo(target, "jumpDownLand")
			{
				layer = 0,
				crossFades = 
				{
					{ key4, 0.15f },
					{ key3, 0.15f }
				}
			};
			new AnimInfo(target, "grabBaton");
			new AnimInfo(target, "grabGun");
			new AnimInfo(target, "attackMeleeA").layer = 0;
			new AnimInfo(target, "attackMeleeB").layer = 0;
			new AnimInfo(target, "getHitA")
			{
				layer = 0,
				crossFades = { { key11, 0.2f } }
			};
			new AnimInfo(target, "getHitB")
			{
				layer = 0,
				crossFades = { { key10, 0.2f } }
			};
			new AnimInfo(target, "getHitSpray");
			new AnimInfo(target, "getHitJuggle").layer = 0;
			new AnimInfo(target, "getHitFlyBack").layer = 0;
			new AnimInfo(target, "getHitLyingDown").layer = 0;
			new AnimInfo(target, "land").layer = 0;
			new AnimInfo(target, "lieDown")
			{
				layer = 0,
				crossFades = { { key13, 0.2f } }
			};
			new AnimInfo(target, "getUp")
			{
				layer = 0,
				crossFades = 
				{
					{ key12, 0.2f },
					{ key13, 0.2f }
				}
			};
			new AnimInfo(target, "gunLowered")
			{
				layer = 1,
				crossFades = 
				{
					{ key17, 0.5f },
					{ key16, 0.2f }
				}
			};
			new AnimInfo(target, "gunLower")
			{
				layer = 1,
				crossFades = 
				{
					{ key17, 0.5f },
					{ key16, 0.2f }
				}
			};
			new AnimInfo(target, "gunAim")
			{
				layer = 1,
				crossFades = 
				{
					{ key15, 0.5f },
					{ key14, 0.5f },
					{ key16, 0.5f }
				}
			};
			new AnimInfo(target, "gunShoot").layer = 1;
			new AnimInfo(target, "gunReload")
			{
				layer = 1,
				crossFades = 
				{
					{ key17, 0.5f },
					{ key14, 0.5f },
					{ key15, 0.5f }
				}
			};
			new AnimInfo(target, "moveDirectionX");
			new AnimInfo(target, "moveDirectionY");
			new AnimInfo(target, "aimAngle");
			new AnimInfo(target, "fallAngle");
			new AnimInfo(target, "dropDuration");
		}
	}
}
