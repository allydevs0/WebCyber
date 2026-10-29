using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class BlackBarClip : PlayableAsset, ITimelineClipAsset
	{
		public BlackBarBehaviour template = new BlackBarBehaviour();

		public ClipCaps clipCaps => ClipCaps.None;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<BlackBarBehaviour>.Create(graph, template);
		}
	}
}
