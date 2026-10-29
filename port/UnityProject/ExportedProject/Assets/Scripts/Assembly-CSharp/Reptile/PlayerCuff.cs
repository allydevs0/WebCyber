using UnityEngine;

namespace Reptile
{
	public class PlayerCuff : MonoBehaviour
	{
		public enum CuffState
		{
			Hidden = 0,
			Connected = 1,
			Free = 2
		}

		[SerializeField]
		private GameObject visual;

		[SerializeField]
		private Transform chain;

		private CuffState state;

		public CuffState State => state;

		public void SetState(CuffState newState)
		{
			state = newState;
			switch (state)
			{
			case CuffState.Hidden:
				visual.SetActive(value: false);
				break;
			case CuffState.Connected:
				visual.SetActive(value: true);
				chain.localScale = Vector3.zero;
				break;
			case CuffState.Free:
				visual.SetActive(value: true);
				chain.localScale = Vector3.one;
				break;
			}
		}
	}
}
