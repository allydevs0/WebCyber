using UnityEngine;

namespace Reptile
{
	public class SwitchErrorHandler : AErrorHandler
	{
		private AStorage storage;

		public SwitchErrorHandler(GameObject errorQueuePrefab, AStorage storage)
			: base(errorQueuePrefab)
		{
			this.storage = storage;
		}

		public override IError HandleCorruptedDataLoadError(ATransaction loadTransaction, Overlay overlay, string dataObjectName, string customPopupMessage)
		{
			SwitchCorruptedDataLoadError switchCorruptedDataLoadError = new SwitchCorruptedDataLoadError(loadTransaction, overlay, storage, dataObjectName, customPopupMessage);
			if (AddErrorToQueue(switchCorruptedDataLoadError))
			{
				return switchCorruptedDataLoadError;
			}
			return null;
		}

		public override IError HandleCorruptedDataSaveError(ATransaction saveTransaction, Overlay overlay)
		{
			SwitchCorruptedDataSaveError switchCorruptedDataSaveError = new SwitchCorruptedDataSaveError(saveTransaction);
			if (AddErrorToQueue(switchCorruptedDataSaveError))
			{
				return switchCorruptedDataSaveError;
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

		public override IError HandleNoDiskSpaceSaveError(ATransaction saveTransaction, Overlay overlay)
		{
			NoDiskSpaceSaveError noDiskSpaceSaveError = new NoDiskSpaceSaveError(saveTransaction, overlay);
			if (AddErrorToQueue(noDiskSpaceSaveError))
			{
				return noDiskSpaceSaveError;
			}
			return null;
		}

		private IError AddCorruptedSaveSlotLoadError(LoadTransaction<SaveSlotData> loadTransaction, Overlay overlay, SaveManager saveManager)
		{
			SwitchCorruptedSaveSlotLoadError switchCorruptedSaveSlotLoadError = new SwitchCorruptedSaveSlotLoadError(overlay, saveManager, loadTransaction, storage);
			if (AddErrorToQueue(switchCorruptedSaveSlotLoadError))
			{
				return switchCorruptedSaveSlotLoadError;
			}
			return null;
		}
	}
}
