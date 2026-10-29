using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class SfxTimelineClip : AudioPlayableAsset
	{
		[SerializeField]
		public SfxClip sfxClip;

		[SerializeField]
		[Range(-1f, 24f)]
		public int indexOfAudioClip = -1;

		[SerializeField]
		public bool randomAudioClip;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			if (Core.Instance.AudioManager != null)
			{
				base.clip = AudioManager.GetAudioClip(sfxClip, randomAudioClip, indexOfAudioClip);
			}
			if (base.clip == null)
			{
				return Playable.Null;
			}
			return base.CreatePlayable(graph, go);
		}
	}
}
