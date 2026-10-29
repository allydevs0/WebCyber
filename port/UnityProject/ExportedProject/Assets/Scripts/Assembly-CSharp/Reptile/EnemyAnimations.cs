using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class EnemyAnimations : MonoBehaviour
	{
		[SerializeField]
		private Animator animator;

		[SerializeField]
		private EnemyAnimInfos enemyAnimationInfos;

		private Dictionary<int, AnimInfo> animInfos = new Dictionary<int, AnimInfo>();

		private int curAnim;

		private List<int> curAnims = new List<int>();

		private bool pausedForSequence;

		private float startWeight;

		private float targetWeight;

		private int targetLayer;

		public Dictionary<int, AnimInfo> EnemyAnimationInfos
		{
			get
			{
				return animInfos;
			}
			set
			{
				animInfos = value;
			}
		}

		public int CurrentAnim => curAnim;

		public bool Paused => pausedForSequence;

		public Animator Animator => animator;

		public void Init(BasicEnemy enemy)
		{
			enemyAnimationInfos.Init(this);
			for (int i = 0; i < animator.layerCount; i++)
			{
				curAnims.Add(0);
			}
			enemy.OnPause.AddListener(Pause);
			enemy.OnUnPause.AddListener(UnPause);
		}

		public void PlayAnim(int anim)
		{
			PlayAnim(anim, restart: false);
		}

		public void PlayAnim(int anim, bool restart)
		{
			PlayAnim(anim, restart, 0f);
		}

		public void PlayAnim(int anim, bool restart, float offset)
		{
			if (curAnims[animInfos[anim].layer] != anim || restart)
			{
				if (curAnims[animInfos[anim].layer] == 0)
				{
					curAnims[animInfos[anim].layer] = anim;
				}
				if (animInfos.ContainsKey(anim) && animInfos[anim].crossFades.ContainsKey(curAnims[animInfos[anim].layer]) && curAnims[animInfos[anim].layer] != anim)
				{
					animator.CrossFadeInFixedTime(animInfos[anim].hashedName, animInfos[anim].crossFades[curAnims[animInfos[anim].layer]], animInfos[anim].layer, offset);
					curAnims[animInfos[anim].layer] = anim;
				}
				else if (animInfos.ContainsKey(anim))
				{
					animator.Play(animInfos[anim].hashedName, animInfos[anim].layer, offset);
					curAnims[animInfos[anim].layer] = anim;
				}
			}
		}

		public void Pause()
		{
			animator.speed = 0f;
			pausedForSequence = true;
		}

		public void UnPause()
		{
			animator.speed = 1f;
			pausedForSequence = false;
		}

		public void SetLayerActivation(int layer, bool value)
		{
			if (targetWeight != (float)(value ? 1 : 0))
			{
				StopCoroutine("TransitionLayerActivationOut");
				targetWeight = (value ? 1 : 0);
				targetLayer = layer;
				StartCoroutine("TransitionLayerActivationOut");
			}
		}

		private IEnumerator TransitionLayerActivationOut()
		{
			float transition = 0f;
			startWeight = animator.GetLayerWeight(targetLayer);
			while (transition < 1f)
			{
				animator.SetLayerWeight(targetLayer, Mathf.Lerp(startWeight, targetWeight, transition));
				transition += Core.dt * 5f;
				yield return null;
			}
			animator.SetLayerWeight(targetLayer, targetWeight);
		}

		public void SetFloatParameter(int parameter, float value)
		{
			if (animInfos.ContainsKey(parameter))
			{
				animator.SetFloat(animInfos[parameter].hashedName, value);
			}
		}

		public float GetFloatParameter(int parameter)
		{
			if (animInfos.ContainsKey(parameter))
			{
				return animator.GetFloat(animInfos[parameter].hashedName);
			}
			return 0f;
		}
	}
}
