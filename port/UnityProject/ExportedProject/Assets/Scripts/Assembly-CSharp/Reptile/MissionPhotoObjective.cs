using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class MissionPhotoObjective : BasePhotoObjective
	{
		private int succesfulPhotoCount;

		public int SuccesfulPhotoCount
		{
			get
			{
				return succesfulPhotoCount;
			}
			set
			{
				succesfulPhotoCount = value;
			}
		}

		public void ResetPhotoCount()
		{
			succesfulPhotoCount = 0;
		}

		protected override IEnumerator HandlePhotoSucces()
		{
			yield return new WaitForSeconds(1f);
			succesfulPhotoCount++;
		}
	}
}
