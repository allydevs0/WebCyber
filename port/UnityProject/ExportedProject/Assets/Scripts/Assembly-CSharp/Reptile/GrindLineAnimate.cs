using UnityEngine;

namespace Reptile
{
	public class GrindLineAnimate : MonoBehaviour
	{
		private GrindLine line;

		private void Awake()
		{
			line = GetComponent<GrindLine>();
			line.attachedToEnemyGrindLine = base.transform.parent.GetComponentInParent<EnemyGrindLines>();
		}

		private void Update()
		{
			line.Rebuild();
		}
	}
}
