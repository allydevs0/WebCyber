using UnityEngine;

namespace Reptile
{
	public struct StreetLifeHot
	{
		public int idleAnimation;

		public int moveAwayAnimation;

		public int walkAnimation;

		public Junk attachedJunk;

		internal bool isShook;

		internal bool wasShook;

		internal float moveTimer;

		internal float moveAwayDuration;

		internal float moveAwaySpeed;

		internal bool isGrounded;

		internal GameObject activationGameObject;

		internal Transform transform;

		internal Collider trigger;

		internal Vector3 verticalDir;

		internal float verticalSpeed;

		internal float verticalAcc;

		internal float maxVerticalSpeed;

		internal float curSpeed;

		internal bool collided;

		internal Vector3 moveDir;

		internal Animator anim;

		internal Vector3 startPos;

		internal Quaternion startRot;

		internal MoveAlongPoints moveAlongPoints;

		internal int currentAnim;
	}
}
