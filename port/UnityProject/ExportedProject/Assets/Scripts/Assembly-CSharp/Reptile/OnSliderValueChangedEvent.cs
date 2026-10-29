using System;
using UnityEngine.Events;

namespace Reptile
{
	[Serializable]
	public class OnSliderValueChangedEvent : UnityEvent<float>
	{
	}
}
