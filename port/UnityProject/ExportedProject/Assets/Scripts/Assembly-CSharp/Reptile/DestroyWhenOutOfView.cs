using UnityEngine;

namespace Reptile
{
	public class DestroyWhenOutOfView : MonoBehaviour
	{
		private Transform tf;

		private void Awake()
		{
			tf = base.transform;
			Core.OnUpdate += UpdateClass;
		}

		private void Start()
		{
		}

		private void UpdateClass()
		{
			if (!WorldHandler.instance.IsReallyInScreen(tf.position))
			{
				Core.OnUpdate -= UpdateClass;
				Object.Destroy(base.gameObject);
			}
		}
	}
}
