using UnityEngine;

namespace Reptile
{
	public class CopAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			new AnimInfo(target, "copIdle");
			int key = Animator.StringToHash("shieldCopIdle");
			int key2 = Animator.StringToHash("copIdle");
			int key3 = Animator.StringToHash("copAgro");
			int key4 = Animator.StringToHash("copRun");
			int key5 = Animator.StringToHash("hitBounce");
			int key6 = Animator.StringToHash("copAttackMelee");
			int key7 = Animator.StringToHash("copShuffle");
			new AnimInfo(target, "shieldCopIdle");
			new AnimInfo(target, "copShuffle")
			{
				layer = 0,
				crossFades = 
				{
					{ key2, 0.2f },
					{ key, 0.2f },
					{ key3, 0.2f },
					{ key4, 0.2f },
					{ key5, 0.2f },
					{ key6, 0.2f }
				}
			};
			new AnimInfo(target, "copRun")
			{
				layer = 0,
				crossFades = 
				{
					{ key2, 0.2f },
					{ key3, 0.2f },
					{ key7, 0.2f },
					{ key5, 0.2f },
					{ key6, 0.2f }
				}
			};
			new AnimInfo(target, "copAgro")
			{
				layer = 0,
				crossFades = 
				{
					{ key2, 0.2f },
					{ key, 0.2f },
					{ key7, 0.2f },
					{ key4, 0.2f },
					{ key5, 0.2f },
					{ key6, 0.2f }
				}
			};
			new AnimInfo(target, "copGetHit").layer = 0;
			new AnimInfo(target, "copGetHit2").layer = 0;
			new AnimInfo(target, "copGetHitHard").layer = 0;
			new AnimInfo(target, "lieDown").layer = 0;
			new AnimInfo(target, "copGetHitLyingDown").layer = 0;
			int key8 = Animator.StringToHash("copGetHitHard");
			int key9 = Animator.StringToHash("sniperAim");
			int key10 = Animator.StringToHash("copShootIdle");
			new AnimInfo(target, "hitBounce")
			{
				layer = 0,
				crossFades = { { key8, 0.2f } }
			};
			new AnimInfo(target, "copShootIdle")
			{
				layer = 1,
				crossFades = { { key9, 0.5f } }
			};
			new AnimInfo(target, "sniperAim")
			{
				layer = 1,
				crossFades = { { key10, 0.5f } }
			};
			new AnimInfo(target, "copAttackMelee").layer = 0;
			new AnimInfo(target, "copGrabGun");
			new AnimInfo(target, "copGrabBaton");
			new AnimInfo(target, "copReload")
			{
				layer = 1,
				crossFades = { { key9, 0.5f } }
			};
			new AnimInfo(target, "copShieldIdle").layer = 2;
			int key11 = Animator.StringToHash("copShieldAttack");
			new AnimInfo(target, "copShieldBash").crossFades.Add(key11, 0.5f);
			new AnimInfo(target, "copShieldBeat").crossFades.Add(key11, 0.5f);
			new AnimInfo(target, "copShieldLookAround");
			new AnimInfo(target, "copShieldAttack");
			new AnimInfo(target, "copShieldOpenIdle");
			new AnimInfo(target, "enemyDirectionX");
			new AnimInfo(target, "enemyDirectionY");
			new AnimInfo(target, "sniperAimAngle");
			int key12 = Animator.StringToHash("hover");
			int key13 = Animator.StringToHash("policeCaptainIreneAttack");
			int key14 = Animator.StringToHash("policeCaptainIreneAttackHit");
			new AnimInfo(target, "policeCaptainIreneAttack").crossFades.Add(key12, 0.2f);
			new AnimInfo(target, "policeCaptainIreneAttackHit").crossFades.Add(key13, 0.05f);
			new AnimInfo(target, "hover").crossFades.Add(key14, 0.2f);
			new AnimInfo(target, "aim");
		}
	}
}
