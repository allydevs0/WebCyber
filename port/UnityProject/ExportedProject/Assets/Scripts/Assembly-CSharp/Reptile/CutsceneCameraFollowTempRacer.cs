using UnityEngine;

namespace Reptile
{
	public class CutsceneCameraFollowTempRacer : MonoBehaviour
	{
		public Transform lookAt;

		internal Transform tf;

		private void Start()
		{
			tf = base.transform;
		}

		private void Update()
		{
			if (lookAt != null)
			{
				tf.LookAt(lookAt);
			}
		}
	}
}
