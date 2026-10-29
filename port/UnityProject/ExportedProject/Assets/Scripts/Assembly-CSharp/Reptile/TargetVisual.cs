using UnityEngine;

namespace Reptile
{
	public class TargetVisual
	{
		public Transform tf;

		public GameObject gameObject;

		public MeshRenderer rend;

		public Vector3 pos;

		public Vector3 vel;

		public bool off;

		private Vector3 TARGET_VISUAL_OFF_POS = new Vector3(0f, -10000f, 0f);

		public TargetVisual(Transform setTf, MeshRenderer setRend)
		{
			tf = setTf;
			gameObject = tf.gameObject;
			rend = setRend;
			SetOff();
		}

		public void SetPos(Vector3 set)
		{
			off = false;
			tf.position = (pos = set);
		}

		public void SetOff()
		{
			gameObject.SetActive(value: false);
			off = true;
		}

		public void SetOn()
		{
			gameObject.SetActive(value: true);
			off = false;
		}
	}
}
