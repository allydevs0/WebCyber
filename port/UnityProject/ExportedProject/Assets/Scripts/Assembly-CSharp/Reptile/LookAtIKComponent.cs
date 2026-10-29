using UnityEngine;

namespace Reptile
{
	public class LookAtIKComponent : MonoBehaviour
	{
		internal Vector3 targetPosition;

		internal float weight;

		private Animator animator;

		private void Awake()
		{
			animator = base.gameObject.GetComponent<Animator>();
		}

		private void OnAnimatorIK(int layerIndex)
		{
			if (targetPosition != Vector3.zero)
			{
				animator.SetLookAtPosition(targetPosition);
				animator.SetLookAtWeight(weight);
			}
			else
			{
				animator.SetLookAtWeight(0f);
			}
			targetPosition = Vector3.zero;
		}
	}
}
