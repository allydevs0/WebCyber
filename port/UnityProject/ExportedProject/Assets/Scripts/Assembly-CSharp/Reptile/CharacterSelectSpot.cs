namespace Reptile
{
	public class CharacterSelectSpot : AProgressable
	{
		public CharacterSelect characterSelectPrefab;

		public bool discovered;

		public override void ReadFromData()
		{
			ObjectProgress objectProgress = (ObjectProgress)progressableData;
			discovered = objectProgress.isDiscovered;
		}

		public override void WriteToData()
		{
			((ObjectProgress)progressableData).isDiscovered = discovered;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
			progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.OBJECT_PROGRESS);
			((ObjectProgress)progressableData).isDiscovered = discovered;
		}

		public void Discover()
		{
			discovered = true;
			WriteToData();
		}
	}
}
