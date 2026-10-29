using UnityEngine;

namespace Reptile
{
	public class MoveAlongPoints : MonoBehaviour
	{
		private BaseMoveHandler handler;

		public float acc = 1f;

		public float decc = 1f;

		public float collisionExtents;

		public float speed = 1f;

		public Transform path;

		public float waitAtEnd;

		public float waitAtStart;

		public bool doNotRotate;

		public bool popBack;

		public int turnSpeed = 3;

		public float delay;

		public float delayMultiplier = 1f;

		public bool stopForPlayer;

		public bool stopForStreetLife;

		public bool stopForEnemies;

		public bool startOnRandomPoint;

		internal bool partOfCluster;

		internal MoveAlongPoints_Hot InitHotData(BaseMoveHandler moveHandler, MoveAlongPoints_Hot hotData)
		{
			handler = moveHandler;
			hotData.moverLOD = MoverLOD.DETAILS;
			hotData.activelyMoving = true;
			hotData.fracJourney = 0f;
			hotData.hasBody = false;
			hotData.posCount = 0;
			hotData.gameObject = base.gameObject;
			hotData.checkNotToHitOthersOnThePath = collisionExtents > 0f;
			hotData.tf = base.transform;
			hotData.body = GetComponent<Rigidbody>();
			if (hotData.body == null)
			{
				hotData.body = GetComponentInChildren<Rigidbody>();
			}
			hotData.hasBody = hotData.body != null;
			hotData.acc = acc;
			hotData.decc = decc;
			hotData.collisionExtents = collisionExtents;
			hotData.speed = speed;
			hotData.path = path;
			hotData.waitAtEnd = waitAtEnd;
			hotData.waitAtStart = waitAtStart;
			hotData.doNotRotate = doNotRotate;
			hotData.popBack = popBack;
			hotData.turnSpeed = turnSpeed;
			return hotData;
		}

		private void OnDestroy()
		{
			handler = null;
			path = null;
		}

		public void HitboxHitPlayer(Player player)
		{
			handler.HitboxHitPlayer(player, this);
		}

		public void TriggerDetectLayer(int layer)
		{
			handler.TriggerDetectLayer(layer, this);
		}
	}
}
