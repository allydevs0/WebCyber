using UnityEngine;

namespace Reptile
{
	public class InverseKinematicsRelay : MonoBehaviour
	{
		protected CharacterVisual characterVisual;

		private void Awake()
		{
			characterVisual = GetComponentInParent<CharacterVisual>();
		}

		private void OnAnimatorIK(int layerIndex)
		{
			characterVisual.InverseKinematics(layerIndex);
		}
	}
}
