using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class StyleSwitchClip : PlayableAsset, ITimelineClipAsset
	{
		public StyleSwitchBehaviour settings = new StyleSwitchBehaviour();

		public ClipCaps clipCaps => ClipCaps.None;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<StyleSwitchBehaviour>.Create(graph, settings);
		}
	}
}
