namespace Reptile
{
	public class PhotoObjectiveProgressable : AProgressable
	{
		private bool hasPhotographedObject;

		public bool HasPhotoGraphedObject => hasPhotographedObject;

		public void MadePhotograph()
		{
			hasPhotographedObject = true;
			WriteToData();
		}

		public override void ReadFromData()
		{
			PhotoObjectProgress photoObjectProgress = (PhotoObjectProgress)progressableData;
			hasPhotographedObject = photoObjectProgress.hasPhotographedObject;
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
			progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.PHOTO_PROGRESS);
			((PhotoObjectProgress)progressableData).hasPhotographedObject = false;
		}

		public override void WriteToData()
		{
			((PhotoObjectProgress)progressableData).hasPhotographedObject = hasPhotographedObject;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}
	}
}
