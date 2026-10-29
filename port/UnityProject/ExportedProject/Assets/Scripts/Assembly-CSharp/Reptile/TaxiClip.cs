using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class TaxiClip : PlayableAsset, ITimelineClipAsset
	{
		public TaxiBehaviour settings = new TaxiBehaviour();

		public ClipCaps clipCaps => ClipCaps.None;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<TaxiBehaviour>.Create(graph, settings);
		}
	}
}
