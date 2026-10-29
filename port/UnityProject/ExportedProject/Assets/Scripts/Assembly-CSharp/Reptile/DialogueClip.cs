using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class DialogueClip : PlayableAsset, ITimelineClipAsset
	{
		public ExposedReference<AudioSource> exposedAudioSource;

		public DialogueBehaviour template = new DialogueBehaviour();

		public ClipCaps clipCaps => ClipCaps.None;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			template.audioSource = exposedAudioSource.Resolve(graph.GetResolver());
			return ScriptPlayable<DialogueBehaviour>.Create(graph, template);
		}
	}
}
