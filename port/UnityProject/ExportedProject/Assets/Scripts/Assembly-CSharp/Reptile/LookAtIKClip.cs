using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class LookAtIKClip : PlayableAsset, ITimelineClipAsset
	{
		public ExposedReference<Transform> target;

		public float IKWeight = 0.75f;

		public ClipCaps clipCaps => ClipCaps.Blending;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			LookAtIKBehaviour lookAtIKBehaviour = new LookAtIKBehaviour();
			lookAtIKBehaviour.target = target.Resolve(graph.GetResolver());
			lookAtIKBehaviour.IKWeight = IKWeight;
			return ScriptPlayable<LookAtIKBehaviour>.Create(graph, lookAtIKBehaviour);
		}
	}
}
