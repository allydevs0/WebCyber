using UnityEngine;
using UnityEngine.Timeline;

namespace Reptile
{
	[TrackColor(0.21f, 0.32f, 0.87f)]
	[TrackClipType(typeof(StyleSwitchClip))]
	[TrackBindingType(typeof(GameObject))]
	public class StyleSwitchTrack : TrackAsset
	{
	}
}
