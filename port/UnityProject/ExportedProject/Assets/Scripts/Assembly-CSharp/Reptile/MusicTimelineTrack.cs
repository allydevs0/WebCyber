using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	[TrackClipType(typeof(MusicTimelineClip), false)]
	[TrackColor(0.75f, 0.34f, 0.1f)]
	public class MusicTimelineTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			ScriptPlayable<MusicTimelineMixer> scriptPlayable = ScriptPlayable<MusicTimelineMixer>.Create(graph, inputCount);
			MusicTimelineMixer behaviour = scriptPlayable.GetBehaviour();
			MusicTimelineClip clipToUseAsstageMusicAtEnd = null;
			double num = 0.0;
			foreach (TimelineClip clip in GetClips())
			{
				MusicTimelineClip musicTimelineClip = clip.asset as MusicTimelineClip;
				if (musicTimelineClip.playAsStageMusic && num < clip.end)
				{
					num = clip.end;
					clipToUseAsstageMusicAtEnd = musicTimelineClip;
				}
			}
			if (num != 0.0)
			{
				behaviour.clipToUseAsstageMusicAtEnd = clipToUseAsstageMusicAtEnd;
			}
			return scriptPlayable;
		}
	}
}
