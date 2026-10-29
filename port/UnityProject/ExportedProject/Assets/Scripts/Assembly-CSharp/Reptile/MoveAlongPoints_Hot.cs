using UnityEngine;

namespace Reptile
{
	public struct MoveAlongPoints_Hot
	{
		public float acc;

		public float decc;

		public float collisionExtents;

		public float speed;

		public Transform path;

		public float waitAtEnd;

		public float waitAtStart;

		public bool doNotRotate;

		public bool popBack;

		public int turnSpeed;

		internal Transform tf;

		internal bool turnedOff;

		internal Vector3 lastPos;

		internal MoverLOD moverLOD;

		internal bool activelyMoving;

		internal bool hasBody;

		internal float timer;

		internal bool checkNotToHitOthersOnThePath;

		internal float stopTimer;

		internal int posCount;

		internal float waitAtStartTimer;

		internal bool stoppedForPlayer;

		internal float timeSpeed;

		internal int moverInFront;

		internal float fracJourney;

		internal float leftOver;

		internal float journeyLength;

		internal float waitAtEndTimer;

		internal Rigidbody body;

		internal int lastPoint;

		internal Vector3 nextPointPos;

		internal Vector3 lastPointPos;

		internal GameObject gameObject;

		internal Vector3[] pathPositions;
	}
}
