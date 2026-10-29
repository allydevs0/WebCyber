using System;
using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class PhotosManager
	{
		internal delegate void DeletePhotoCallback();

		internal delegate void PhotosUpdatedDelegate(ATransaction.TransactionStatus lastTransactionStatus);

		internal delegate void GetPhotosDelegate(int photosLoadedStartIndex, int photoCountTotal, PhonePictureData[] photosBytes);

		internal delegate void LoadPhotoCallback(int loadInfoIndex, int photoIndex, PhonePictureData pictureData);

		public const int MaxPhotoCount = 50;

		public const string PhotosSubfolder = "Photos";

		public const uint COR_E_DIRECTORYNOTFOUND = 2147942403u;

		public const string PHOTO_DATA_LOCALIZATION_KEY = "ERROR_PHOTO_DATA";

		public const string PHOTO_DATA_ERROR_LOCALIZATION_KEY = "ERROR_NO_ACCESS_INVESTIGATE";

		private AStorage storage;

		private AErrorHandler errorHandler;

		private UIManager uIManager;

		private SaveManager saveManager;

		private IGameTextLocalizer localizer;

		internal int availablePhotosCount;

		internal bool errorOnLastPhotoCountRefresh;

		internal ATransaction.TransactionDoneDelegate onTransactionDone;

		internal Coroutine photosLoadCoroutine;

		internal bool initialized;

		private ATransaction directoryTransaction;

		internal ATransaction.TransactionStatus lastSaveResult;

		internal event PhotosUpdatedDelegate OnPhotosRefresh;

		private event DeletePhotoCallback OnDeletedPhoto;

		public void Init(AStorage storage, AErrorHandler errorHandler, UIManager uIManager, SaveManager saveManager, IGameTextLocalizer localizer)
		{
			this.storage = storage;
			this.errorHandler = errorHandler;
			this.uIManager = uIManager;
			this.saveManager = saveManager;
			this.localizer = localizer;
			initialized = true;
			RefreshPhotoCountAsync();
		}

		public void Terminate()
		{
			initialized = false;
			StopPhotosLoadCoroutine();
			storage = null;
		}

		internal void RefreshPhotoCountAsync()
		{
			if (directoryTransaction == null)
			{
				directoryTransaction = Core.Instance.Platform.Storage.CreateDirectory("Photos", isLocal: false);
			}
			directoryTransaction.OnTransactionDone += CreatePhotosDirectoryResult;
		}

		internal void RetryTransaction(ATransaction transaction, ATransaction.TransactionDoneDelegate doneCallback)
		{
			transaction.OnTransactionDone += doneCallback;
			transaction.ScheduleForReset();
			storage.EnqueueTransaction(transaction);
		}

		private void CreatePhotosDirectoryResult(ATransaction transaction)
		{
			directoryTransaction.OnTransactionDone -= CreatePhotosDirectoryResult;
			if (!initialized)
			{
				return;
			}
			if (directoryTransaction.Status == ATransaction.TransactionStatus.RETRY)
			{
				RetryTransaction(directoryTransaction, CreatePhotosDirectoryResult);
			}
			else if (directoryTransaction.HasError)
			{
				if (errorHandler == null)
				{
					return;
				}
				switch (directoryTransaction.Status)
				{
				case ATransaction.TransactionStatus.NO_STORAGE:
				{
					IError error3 = errorHandler.HandleNoDiskSpaceSaveError(transaction, uIManager.Overlay);
					if (error3 != null)
					{
						error3.OnErrorHandlerFinished += HandleCreatePhotosDirectoryError;
					}
					break;
				}
				case ATransaction.TransactionStatus.EXCEPTION:
					PhotosManagerUnhandledError(null);
					break;
				case ATransaction.TransactionStatus.NO_ACCESS:
				{
					IError error2 = new NoAccessToDataError(transaction, uIManager.Overlay);
					if (error2 != null)
					{
						error2.OnErrorHandlerFinished += HandleCreatePhotosDirectoryError;
					}
					break;
				}
				case ATransaction.TransactionStatus.CORRUPTED_DATA:
				{
					IError error = errorHandler.HandleCorruptedDataLoadError(transaction, uIManager.Overlay, string.Empty, string.Empty);
					if (error != null)
					{
						error.OnErrorHandlerFinished += HandleCreatePhotosDirectoryError;
					}
					break;
				}
				case ATransaction.TransactionStatus.FILE_NOT_FOUND:
					break;
				}
			}
			else
			{
				directoryTransaction = Core.Instance.Platform.Storage.GetDirectoryInfoFromStorage("Photos");
				directoryTransaction.OnTransactionDone += RefreshPhotoCountResult;
			}
		}

		private void RefreshPhotoCountResult(ATransaction transaction)
		{
			transaction.OnTransactionDone -= RefreshPhotoCountResult;
			if (!initialized || !(transaction is DirectoryInfoTransaction))
			{
				return;
			}
			if (transaction.IsSuccess)
			{
				int photosCountFromDirectoryFileNames = UserPhotosDataLoader.GetPhotosCountFromDirectoryFileNames((transaction as DirectoryInfoTransaction).DirectoryFileNames);
				RefreshPhotoCountSuccesful(transaction, photosCountFromDirectoryFileNames);
				return;
			}
			if (transaction.Status == ATransaction.TransactionStatus.RETRY)
			{
				RetryFailedTransaction(transaction, RefreshPhotoCountResult);
				return;
			}
			string userInterfaceText = Core.Instance.Localizer.GetUserInterfaceText("ERROR_PHOTO_DATA");
			string userInterfaceText2 = Core.Instance.Localizer.GetUserInterfaceText("ERROR_NO_ACCESS_INVESTIGATE");
			IError error = Core.Instance.Platform.ErrorHandler.HandleLoadTransactionError(transaction, uIManager.Overlay, Core.Instance.SaveManager, userInterfaceText, userInterfaceText2);
			if (error == null)
			{
				PhotosManagerUnhandledError("PHOTOS_ERROR_LOAD");
			}
			else
			{
				error.OnErrorHandlerFinished += HandleRefreshPhotoCountError;
			}
		}

		private void SetPhotoCountResult(ATransaction transaction, int photosCount)
		{
			availablePhotosCount = photosCount;
			if (directoryTransaction.Equals(transaction))
			{
				directoryTransaction = null;
			}
		}

		private void PhotosManagerUnhandledError(string errorMessage)
		{
			Core.Instance.ReturnToIntroFromThread(errorMessage);
		}

		private void RefreshPhotoCountSuccesful(ATransaction transaction, int count)
		{
			SetPhotoCountResult(transaction, count);
			if (OnPhotosRefresh != null)
			{
				OnPhotosRefresh(transaction.Status);
			}
		}

		internal static void RetryFailedTransaction(ATransaction transaction, ATransaction.TransactionDoneDelegate onDoneCallback)
		{
			transaction.ScheduleForReset();
			transaction.OnTransactionDone += onDoneCallback;
			Core.Instance.Platform.Storage.EnqueueTransaction(transaction);
		}

		private void HandleRefreshPhotoCountError(IError error)
		{
			error.OnErrorHandlerFinished -= HandleRefreshPhotoCountError;
			if (initialized && error is ITransactionError)
			{
				ITransactionError obj = (ITransactionError)error;
				ATransaction transaction = obj.Transaction;
				if (obj.Transaction is DirectoryInfoTransaction && error.ErrorHandlingResult == ErrorHandlingResult.RESOLVED)
				{
					int photosCountFromDirectoryFileNames = UserPhotosDataLoader.GetPhotosCountFromDirectoryFileNames((transaction as DirectoryInfoTransaction).DirectoryFileNames);
					RefreshPhotoCountSuccesful(transaction, photosCountFromDirectoryFileNames);
				}
				else if (error.ErrorHandlingResult == ErrorHandlingResult.RETRY)
				{
					RetryFailedTransaction(transaction, RefreshPhotoCountResult);
				}
				else
				{
					PhotosManagerUnhandledError("PHOTOS_ERROR_LOAD");
				}
			}
		}

		private void HandleCreatePhotosDirectoryError(IError error)
		{
			error.OnErrorHandlerFinished -= HandleCreatePhotosDirectoryError;
			if (initialized && error is ITransactionError)
			{
				ITransactionError transactionError = (ITransactionError)error;
				if (error.ErrorHandlingResult == ErrorHandlingResult.RETRY || error.ErrorHandlingResult == ErrorHandlingResult.RESOLVED)
				{
					RetryFailedTransaction(transactionError.Transaction, CreatePhotosDirectoryResult);
				}
				else if (error.ErrorHandlingResult == ErrorHandlingResult.FAILED)
				{
					PhotosManagerUnhandledError("PHOTOS_ERROR_LOAD");
				}
			}
		}

		public bool ArePhotosLoaded()
		{
			bool result = true;
			if (directoryTransaction != null)
			{
				result = directoryTransaction.IsDone;
			}
			return result;
		}

		public bool IsPhotoStorageFull()
		{
			return availablePhotosCount >= 50;
		}

		internal void StopPhotosLoadCoroutine()
		{
			if (photosLoadCoroutine != null)
			{
				if (Core.Instance != null)
				{
					Core.Instance.StopCoroutine(photosLoadCoroutine);
				}
				photosLoadCoroutine = null;
			}
		}

		internal void LoadPhotoAsync(int loadInfoIndex, int photoToLoadIndex, LoadPhotoCallback callback)
		{
			photosLoadCoroutine = Core.Instance.StartCoroutine(LoadPhotosFromUserData(loadInfoIndex, availablePhotosCount - photoToLoadIndex - 1, callback));
		}

		private IEnumerator LoadPhotosFromUserData(int loadInfoIndex, int photoToLoadIndex, LoadPhotoCallback loadCallback)
		{
			UserPhotosDataLoader userPhotosDataLoader = new UserPhotosDataLoader(saveManager, storage, errorHandler, uIManager, localizer, "Photos", photoToLoadIndex, 1);
			yield return userPhotosDataLoader.LoadSaveDataASync();
			if (initialized)
			{
				PhonePictureData pictureData = null;
				if (userPhotosDataLoader.IsSuccess)
				{
					pictureData = userPhotosDataLoader.PhonePictureData[0];
				}
				else
				{
					PhotosManagerUnhandledError("PHOTOS_ERROR_LOAD");
				}
				photoToLoadIndex = availablePhotosCount - photoToLoadIndex - 1;
				loadCallback(loadInfoIndex, photoToLoadIndex, pictureData);
			}
		}

		internal void DeletePhotoAsync(PhonePictureData pictureData, DeletePhotoCallback deletePhotoCallback)
		{
			if (pictureData != null)
			{
				OnDeletedPhoto = deletePhotoCallback;
				storage.DeleteDataFromStorage(pictureData.pictureFilename, "Photos", isLocal: false).OnTransactionDone += DeletePhotoResult;
			}
			else
			{
				deletePhotoCallback();
			}
		}

		private void SetDeletePhotoResult(bool success)
		{
			if (success)
			{
				availablePhotosCount--;
			}
			if (OnDeletedPhoto != null)
			{
				OnDeletedPhoto();
			}
		}

		private void DeletePhotoResult(ATransaction transaction)
		{
			transaction.OnTransactionDone -= DeletePhotoResult;
			if (!initialized)
			{
				return;
			}
			if (transaction.IsSuccess)
			{
				SetDeletePhotoResult(success: true);
				return;
			}
			if (transaction.Status == ATransaction.TransactionStatus.RETRY)
			{
				RetryFailedTransaction(transaction, DeletePhotoResult);
				return;
			}
			IError error = errorHandler.HandleDeleteTransactionError(transaction, uIManager.overlay);
			if (error == null)
			{
				PhotosManagerUnhandledError("PHOTOS_ERROR_DELETE");
			}
			else
			{
				error.OnErrorHandlerFinished += HandleDeletePhotoError;
			}
		}

		private void HandleDeletePhotoError(IError error)
		{
			error.OnErrorHandlerFinished -= HandleDeletePhotoError;
			if (initialized && error is ITransactionError)
			{
				ITransactionError transactionError = (ITransactionError)error;
				if (error.ErrorHandlingResult == ErrorHandlingResult.RESOLVED || error.ErrorHandlingResult == ErrorHandlingResult.RETRY)
				{
					RetryFailedTransaction(transactionError.Transaction, DeletePhotoResult);
				}
				else
				{
					PhotosManagerUnhandledError("PHOTOS_ERROR_DELETE");
				}
			}
		}

		public bool SavePhoto(RenderTexture textureToSave, Vector2Int textureSize, out Texture2D outTexture)
		{
			bool result = false;
			if (availablePhotosCount >= 50)
			{
				outTexture = null;
				lastSaveResult = ATransaction.TransactionStatus.NO_STORAGE;
			}
			else
			{
				lastSaveResult = ATransaction.TransactionStatus.BUSY;
				RenderTexture.active = textureToSave;
				Texture2D texture2D = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGB24, mipChain: false);
				texture2D.ReadPixels(new Rect(0f, 0f, textureSize.x, textureSize.y), 0, 0);
				texture2D.Apply();
				RenderTexture.active = null;
				byte[] pictureData = texture2D.EncodeToPNG();
				string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
				PhonePictureData data = new PhonePictureData(pictureData);
				storage.SaveDataAtStorage(data, fileName, "Photos", isLocal: false, DataSerializerFactory.FormatType.RAW_DATA).OnTransactionDone += SaveTransaction_OnTransactionDone;
				availablePhotosCount++;
				outTexture = texture2D;
				result = true;
			}
			return result;
		}

		private void SaveTransaction_OnTransactionDone(ATransaction saveTransaction)
		{
			saveTransaction.OnTransactionDone -= SaveTransaction_OnTransactionDone;
			if (initialized)
			{
				if (saveTransaction.Status == ATransaction.TransactionStatus.SUCCESS)
				{
					SetSaveResult(saveTransaction);
				}
				else if (saveTransaction.Status == ATransaction.TransactionStatus.RETRY)
				{
					RetryFailedTransaction(saveTransaction, SaveTransaction_OnTransactionDone);
				}
				else
				{
					HandleSaveTransactionError(saveTransaction);
				}
			}
		}

		private void SetSaveResult(ATransaction saveTransaction)
		{
			lastSaveResult = saveTransaction.Status;
		}

		private void HandleSaveTransactionError(ATransaction transaction)
		{
			IError error = errorHandler.HandleSaveTransactionError(transaction, uIManager.Overlay);
			if (error == null)
			{
				Debug.Log("Photo manager received no error to handle!");
				PhotosManagerUnhandledError("PHOTOS_ERROR_SAVE");
			}
			else
			{
				error.OnErrorHandlerFinished += OnSavePhotoErrorHandlingFinished;
			}
		}

		private void OnSavePhotoErrorHandlingFinished(IError error)
		{
			error.OnErrorHandlerFinished -= OnSavePhotoErrorHandlingFinished;
			if (initialized && error is ITransactionError)
			{
				ATransaction transaction = ((ITransactionError)error).Transaction;
				if (error.ErrorHandlingResult == ErrorHandlingResult.RETRY)
				{
					RetryFailedTransaction(transaction, SaveTransaction_OnTransactionDone);
					return;
				}
				if (error.ErrorHandlingResult == ErrorHandlingResult.RESOLVED)
				{
					lastSaveResult = ATransaction.TransactionStatus.SUCCESS;
					return;
				}
				Debug.Log("Photo manager error handling could not resolve issue!");
				PhotosManagerUnhandledError("PHOTOS_ERROR_SAVE");
			}
		}
	}
}
