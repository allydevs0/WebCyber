using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class RailRider : MonoBehaviour, IPoolable
	{
		[SerializeField]
		private float speed = 20f;

		[SerializeField]
		private GameObject hitbox;

		[SerializeField]
		private AudioSource audioSource;

		private PoolableState state = PoolableState.Pooled;

		private List<IPoolable> poolables = new List<IPoolable>();

		private string poolableName;

		private Transform tf;

		private GrindPath path;

		private GrindLine grindLine;

		public Vector3 preGrindDir;

		public GrindNode nextNode;

		public GrindPath lastPath;

		public float posOnLine;

		public Vector3 normal;

		private float lastSign;

		private float sign;

		private float lifeTime;

		public PoolableState State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
			}
		}

		public string PoolableName
		{
			get
			{
				return poolableName;
			}
			set
			{
				poolableName = value;
			}
		}

		public void InitPoolable(Vector3 position, Quaternion rotation)
		{
			if (state == PoolableState.Pooled)
			{
				base.gameObject.SetActive(value: true);
				tf = base.transform;
				tf.position = position;
				tf.rotation = rotation;
				state = PoolableState.Active;
				lifeTime = 0f;
				Core.OnUpdate += UpdateRailRider;
			}
		}

		public void Init(GrindPath path)
		{
			hitbox.SetActive(value: true);
			this.path = path;
			GrindLine[] componentsInChildren = path.GetComponentsInChildren<GrindLine>();
			float num = float.MaxValue;
			int num2 = -1;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Vector3 vector = componentsInChildren[i].SnapPosToLine(tf.position) - tf.position;
				vector.y *= 2f;
				float magnitude = vector.magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					num2 = i;
				}
			}
			grindLine = componentsInChildren[num2];
			tf.position = grindLine.SnapPosToLine(tf.position);
			tf.forward = grindLine.GetLineDir(tf.forward);
			sign = tf.localRotation.eulerAngles.x;
			Core.Instance.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.cuffMissile, audioSource);
		}

		private void UpdateRailRider()
		{
			Vector3 position = tf.position;
			Vector3 forward = tf.forward;
			_ = tf.right;
			lastSign = sign;
			sign = tf.localRotation.eulerAngles.x;
			lifeTime += Core.dt;
			if (((lastSign < 0f && sign > 0f) || (lastSign > 0f && sign < 0f) || (lastSign < 180f && sign > 180f) || (lastSign > 180f && sign < 180f)) && lifeTime > 2f)
			{
				ReturnToPool();
				return;
			}
			nextNode = grindLine.GetNextNode(forward);
			position = grindLine.SnapPosToLine(position);
			position += (nextNode - position).normalized * speed * Core.dt;
			posOnLine = grindLine.GetRelativePosOnLine(position, forward);
			grindLine.GetAbsoluteLinePos(position, forward);
			if (posOnLine >= 1f)
			{
				float num = posOnLine * grindLine.Length() - grindLine.Length();
				if (nextNode.IsEndpoint)
				{
					ReturnToPool();
					return;
				}
				GrindLine nextLine = grindLine.GetNextLine(forward);
				Vector3 normalized = (nextLine.GetOtherNode(nextNode) - nextNode).normalized;
				forward = normalized;
				position = nextNode.position + num * normalized;
				grindLine = nextLine;
				nextNode = grindLine.GetNextNode(normalized);
				posOnLine = grindLine.GetRelativePosOnLine(position, normalized);
				grindLine.GetAbsoluteLinePos(position, forward);
				normal = grindLine.GetNormalAtPos(position);
				tf.rotation = Quaternion.LookRotation(normalized, normal);
			}
			tf.position = position;
		}

		public virtual void ReturnToPool()
		{
			base.gameObject.SetActive(value: false);
			state = PoolableState.Pooled;
			Core.OnUpdate -= UpdateRailRider;
			Core.Instance.AudioManager.StopLoopingSfx(audioSource);
		}

		public void HitPlayer()
		{
			Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankwalkerPound, audioSource);
		}
	}
}
