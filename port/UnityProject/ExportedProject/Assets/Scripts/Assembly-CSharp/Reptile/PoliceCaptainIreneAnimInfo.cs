using UnityEngine;

namespace Reptile
{
	public class PoliceCaptainIreneAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			Animator.StringToHash("move");
			int key = Animator.StringToHash("moveGun");
			int key2 = Animator.StringToHash("getUp");
			new AnimInfo(target, "move")
			{
				layer = 0,
				crossFades = { { key2, 0.2f } }
			};
			new AnimInfo(target, "moveGun")
			{
				layer = 0,
				crossFades = { { key2, 0.2f } }
			};
			int key3 = Animator.StringToHash("getHitA");
			int key4 = Animator.StringToHash("getHitB");
			int key5 = Animator.StringToHash("getHitFlyBack");
			int key6 = Animator.StringToHash("getHitLyingDown");
			Animator.StringToHash("getHitJuggle");
			Animator.StringToHash("land");
			Animator.StringToHash("lieDown");
			new AnimInfo(target, "getHitA")
			{
				layer = 0,
				crossFades = { { key4, 0.2f } }
			};
			new AnimInfo(target, "getHitB")
			{
				layer = 0,
				crossFades = { { key3, 0.2f } }
			};
			new AnimInfo(target, "getHitJuggle").layer = 0;
			new AnimInfo(target, "getHitFlyBack").layer = 0;
			new AnimInfo(target, "getHitLyingDown").layer = 0;
			new AnimInfo(target, "land").layer = 0;
			new AnimInfo(target, "lieDown")
			{
				layer = 0,
				crossFades = { { key6, 0.2f } }
			};
			new AnimInfo(target, "getUp")
			{
				layer = 0,
				crossFades = 
				{
					{ key5, 0.2f },
					{ key6, 0.2f }
				}
			};
			new AnimInfo(target, "aim").crossFades.Add(key, 0.3f);
			new AnimInfo(target, "shoot");
			new AnimInfo(target, "attackMelee");
			new AnimInfo(target, "attackMeleeHit");
			new AnimInfo(target, "moveDirectionX");
			new AnimInfo(target, "moveDirectionY");
			new AnimInfo(target, "aimAngle");
		}
	}
}
