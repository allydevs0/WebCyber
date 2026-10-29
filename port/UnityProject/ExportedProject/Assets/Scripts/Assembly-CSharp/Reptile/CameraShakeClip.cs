using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class CameraShakeClip : PlayableAsset, ITimelineClipAsset
	{
		public ScreenShakeType curShakeType;

		public Vector3 shakeDir = Vector3.up;

		public bool animatedCamera = true;

		public ClipCaps clipCaps => ClipCaps.None;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			CameraShakeBehaviour cameraShakeBehaviour = new CameraShakeBehaviour();
			cameraShakeBehaviour.curShakeType = curShakeType;
			cameraShakeBehaviour.animatedCamera = animatedCamera;
			cameraShakeBehaviour.shakeDir = shakeDir;
			return ScriptPlayable<CameraShakeBehaviour>.Create(graph, cameraShakeBehaviour);
		}
	}
}
