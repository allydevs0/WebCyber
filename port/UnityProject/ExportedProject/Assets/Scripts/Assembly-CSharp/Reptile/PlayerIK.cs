using UnityEngine;

namespace Reptile
{
	public class PlayerIK : MonoBehaviour
	{
		protected Animator animator;

		public bool ikActive;

		public Transform handRightTarget;

		public Transform handLeftTarget;

		private void Start()
		{
			animator = GetComponent<Animator>();
		}

		private void OnAnimatorIK()
		{
			if (!animator)
			{
				return;
			}
			if (ikActive)
			{
				if (handRightTarget != null)
				{
					animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
					animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
					animator.SetIKPosition(AvatarIKGoal.RightHand, handRightTarget.position);
					animator.SetIKRotation(AvatarIKGoal.RightHand, handRightTarget.rotation);
				}
				if (handLeftTarget != null)
				{
					animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
					animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
					animator.SetIKPosition(AvatarIKGoal.LeftHand, handLeftTarget.position);
					animator.SetIKRotation(AvatarIKGoal.LeftHand, handLeftTarget.rotation);
				}
			}
			else
			{
				animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
				animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
				animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
				animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
			}
		}
	}
}
