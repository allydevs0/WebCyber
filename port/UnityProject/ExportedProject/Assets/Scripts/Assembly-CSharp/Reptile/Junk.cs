using UnityEngine;

namespace Reptile
{
	public class Junk : MonoBehaviour
	{
		public enum Interact
		{
			ON_HITBOX = 0,
			ON_RUN_IN_AND_HITBOX = 1,
			ON_AWAKE = 2,
			ON_TOUCH = 3
		}

		public Interact interactOn = Interact.ON_RUN_IN_AND_HITBOX;

		public float destroyTimer = -1f;

		public SfxClip audioClips = SfxCollectionID.EnvironmentSfx;

		internal JunkBehaviour junkBehaviour;

		internal JunkHolder junkHolder;

		internal Rigidbody rigidBody;

		internal Quaternion originRotation;

		internal Vector3 originPosition;

		internal Vector3 hitPosition;

		internal Vector3 hitForce;

		internal float rehitTimer;

		internal float hitPauseTimer;

		internal float hitpauseDuration = 0.04f;

		internal int deactiveCounter;

		internal const float DEFAULT_KICK_DIRECTION_FORCE = 20f;

		internal const float SPECIAL_KICK_DIRECTION_FORCE = 4f;

		internal const float DEFAULT_KICK_UP_FORCE = 5f;

		internal const float SPECIAL_KICK_UP_FORCE = 11f;

		internal int _index;

		internal int _startIndex;

		internal void Init(JunkBehaviour junkBehaviour, int index)
		{
			this.junkBehaviour = junkBehaviour;
			_index = index;
			_startIndex = index;
			junkHolder = GetComponentInParent<JunkHolder>();
			originPosition = base.transform.position;
			originRotation = base.transform.rotation;
			rigidBody = GetComponent<Rigidbody>();
			rigidBody.isKinematic = true;
			if (interactOn == Interact.ON_AWAKE)
			{
				JunkBehaviour.FallApart(this.junkBehaviour, this, byBump: false);
			}
		}

		public void FallApart(bool byBump = true)
		{
			JunkBehaviour.FallApartFromJunk(junkBehaviour, this, byBump);
		}

		public void Restore()
		{
			JunkBehaviour.RestoreSingle(junkBehaviour, this);
		}

		public void OnTriggerStay(Collider other)
		{
			if (junkBehaviour == null || other.gameObject.layer == 25 || JunkBehaviour.IsJunkKicked(junkBehaviour.kickedJunkIndex, junkBehaviour.nonupdatingJunkIndex, _index) || rehitTimer != 0f)
			{
				return;
			}
			int layer = other.gameObject.layer;
			if (layer != 18 && layer != 20)
			{
				return;
			}
			bool flag = false;
			Vector3 position = other.transform.position;
			Vector3 position2 = base.transform.position;
			if (layer == 18)
			{
				Player componentInParent = other.transform.GetComponentInParent<Player>();
				if (componentInParent == null)
				{
					return;
				}
				flag = !componentInParent.isAI && componentInParent.ability is GroundTrickAbility && componentInParent.groundTrickAbility.IsCurrentTrickSpecialKick;
				position = componentInParent.tf.position;
				Core.Instance.AudioManager.PlaySfxGameplay(audioClips, WorldHandler.instance.GetSpatialAudioSource(position2));
			}
			JunkBehaviour.MoveToKickedJunk(junkBehaviour, this, hitpauseDuration, 0.5f);
			Vector3 normalized = (position2 - position).normalized;
			normalized = Vector3.ProjectOnPlane(normalized, Vector3.up);
			if (flag)
			{
				hitForce = normalized * 20f + Vector3.up * 5f;
				hitPosition = rigidBody.ClosestPointOnBounds(position);
			}
			else
			{
				hitForce = normalized * 4f + Vector3.up * 11f;
				hitPosition = (position2 + position) * 0.5f;
			}
		}
	}
}
