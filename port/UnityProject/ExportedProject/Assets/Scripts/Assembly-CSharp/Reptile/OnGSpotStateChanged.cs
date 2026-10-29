using System;
using UnityEngine.Events;

namespace Reptile
{
	[Serializable]
	public class OnGSpotStateChanged : UnityEvent<GraffitiSpotFinisher, GraffitiState>
	{
	}
}
