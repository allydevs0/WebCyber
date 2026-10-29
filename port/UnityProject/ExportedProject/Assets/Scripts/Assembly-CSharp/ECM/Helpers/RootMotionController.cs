using UnityEngine;

namespace ECM.Helpers
{
	[RequireComponent(typeof(Animator))]
	public sealed class RootMotionController : MonoBehaviour
	{
		private Animator _animator;

		public Vector3 animVelocity { get; private set; }

		public void Awake()
		{
			_animator = GetComponent<Animator>();
			_ = _animator == null;
		}

		public void OnAnimatorMove()
		{
			float deltaTime = Time.deltaTime;
			if (deltaTime > 0f)
			{
				animVelocity = _animator.deltaPosition / deltaTime;
			}
		}
	}
}
