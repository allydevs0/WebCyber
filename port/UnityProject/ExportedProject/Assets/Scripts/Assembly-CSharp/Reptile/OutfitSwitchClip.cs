using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class OutfitSwitchClip : PlayableAsset, ITimelineClipAsset
	{
		public OutfitSwitchBehaviour settings = new OutfitSwitchBehaviour();

		public ClipCaps clipCaps => ClipCaps.None;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<OutfitSwitchBehaviour>.Create(graph, settings);
		}
	}
}
