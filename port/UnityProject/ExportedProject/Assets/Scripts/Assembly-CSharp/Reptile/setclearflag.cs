using UnityEngine;

namespace Reptile
{
	public class setclearflag : MonoBehaviour
	{
		private void Update()
		{
			if (Camera.main != null)
			{
				Camera.main.clearFlags = CameraClearFlags.Nothing;
				base.enabled = false;
			}
		}
	}
}
