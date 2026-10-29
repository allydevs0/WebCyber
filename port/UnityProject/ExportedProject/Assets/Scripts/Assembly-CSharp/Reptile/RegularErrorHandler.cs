using UnityEngine;

namespace Reptile
{
	public class RegularErrorHandler : AErrorHandler
	{
		private AStorage storage;

		public RegularErrorHandler(GameObject errorQueuePrefab, AStorage storage)
			: base(errorQueuePrefab)
		{
			this.storage = storage;
		}

		public override IError HandleNoDiskSpaceSaveError(ATransaction saveTransaction, Overlay overlay)
		{
			NoDiskSpaceSaveError noDiskSpaceSaveError = new NoDiskSpaceSaveError(saveTransaction, overlay);
			if (AddErrorToQueue(noDiskSpaceSaveError))
			{
				return noDiskSpaceSaveError;
			}
			return null;
		}

		public override IError HandleCorruptedDataLoadError(ATransaction loadTransaction, Overlay overlay, string dataObjectName, string customPopupMessage)
		{
			CorruptedDataLoadError corruptedDataLoadError = new CorruptedDataLoadError(overlay, loadTransaction, storage, dataObjectName, customPopupMessage);
			if (AddErrorToQueue(corruptedDataLoadError))
			{
				return corruptedDataLoadError;
			}
			return null;
		}

		public override IError HandleCorruptedSaveSlotLoadError(ATransaction loadTransaction, Overlay overlay, SaveManager saveManager)
		{
			if (loadTransaction is LoadTransaction<SaveSlotData>)
			{
				return AddCorruptedSaveSlotLoadError(loadTransaction as LoadTransaction<SaveSlotData>, overlay, saveManager);
			}
			return null;
		}

		public override IError HandleCorruptedDataSaveError(ATransaction transaction, Overlay overlay)
		{
			CorruptedDataSaveError corruptedDataSaveError = new CorruptedDataSaveError(transaction, storage);
			if (AddErrorToQueue(corruptedDataSaveError))
			{
				return corruptedDataSaveError;
			}
			return null;
		}

		private IError AddCorruptedSaveSlotLoadError(LoadTransaction<SaveSlotData> loadTransaction, Overlay overlay, SaveManager saveManager)
		{
			CorruptedSaveSlotLoadError corruptedSaveSlotLoadError = new CorruptedSaveSlotLoadError(overlay, saveManager, loadTransaction, storage, Core.Instance.Localizer);
			if (AddErrorToQueue(corruptedSaveSlotLoadError))
			{
				return corruptedSaveSlotLoadError;
			}
			return null;
		}
	}
}
