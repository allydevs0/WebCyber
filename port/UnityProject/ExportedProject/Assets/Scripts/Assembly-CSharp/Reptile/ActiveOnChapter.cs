using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class ActiveOnChapter : MonoBehaviour
	{
		public List<Story.Chapter> chapters;

		private void Awake()
		{
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		public void OnStageInitialized()
		{
			base.gameObject.SetActive(chapters.Contains(Story.GetCurrentObjectiveInfo().chapter));
		}

		public void OnDestroy()
		{
			StageManager.OnStageInitialized -= OnStageInitialized;
		}
	}
}
