using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class GraffitiInfoPanelTargetSet : MonoBehaviour
	{
		[SerializeField]
		private GraffitiSize m_GraffitiSize = GraffitiSize.M;

		public GraffitiSize GraffitiSize => m_GraffitiSize;

		public Image[] Targets { get; private set; }

		public void Initalize()
		{
			Targets = base.transform.GetComponentsInChildren<Image>();
		}
	}
}
