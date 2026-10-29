using UnityEngine.Timeline;

namespace Reptile
{
	[TrackColor(0.855f, 0.903f, 0.87f)]
	[TrackClipType(typeof(CameraShakeClip))]
	[TrackBindingType(typeof(CameraShakeComponent))]
	public class CameraShakeTrack : TrackAsset
	{
	}
}
