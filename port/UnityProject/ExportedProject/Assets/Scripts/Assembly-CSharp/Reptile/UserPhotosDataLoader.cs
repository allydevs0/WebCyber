using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class UserPhotosDataLoader : AUserSaveDataLoader
	{
		private PhonePictureData[] phonePictureData;

		private DirectoryInfoTransaction photoDirectoryInfoTransaction;

		private string photosSubFolder = string.Empty;

		private int startIndex;

		private int maxPhotosToLoad;

		public PhonePictureData[] PhonePictureData => phonePictureData;

		public int PhotoCount
		{
			get
			{
				int result = 0;
				if (photoDirectoryInfoTransaction != null && photoDirectoryInfoTransaction.IsSuccess)
				{
					result = GetPhotosCountFromDirectoryFileNames(photoDirectoryInfoTransaction.DirectoryFileNames);
				}
				return result;
			}
		}

		public UserPhotosDataLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer, string photosSubFolder, int startIndex, int maxPhotosToLoad)
			: base(saveManager, storage, errorHandler, uIManager, localizer)
		{
			this.photosSubFolder = photosSubFolder;
			this.startIndex = startIndex;
			this.maxPhotosToLoad = maxPhotosToLoad;
		}

		public override IEnumerator LoadSaveDataASync()
		{
			yield return LoadPhotoDirectoryInfo();
			if (photoDirectoryInfoTransaction.IsSuccess)
			{
				yield return LoadPhotosData();
			}
			else
			{
				HandleFailedLoadingDirectoryInfo(photoDirectoryInfoTransaction);
			}
		}

		private IEnumerator LoadPhotoDirectoryInfo()
		{
			photoDirectoryInfoTransaction = storage.GetDirectoryInfoFromStorage(photosSubFolder);
			yield return DoTransactionASync(photoDirectoryInfoTransaction, base.HandleLoadUserSaveDataErrorASync, base.DoTransactionASync);
		}

		internal static int GetPhotosCountFromDirectoryFileNames(string[] directoryFileNames)
		{
			int num = 0;
			for (int i = 0; i < directoryFileNames.Length; i++)
			{
				ReadOnlySpan<char> extension = Path.GetExtension(directoryFileNames[i].AsSpan());
				num += Convert.ToInt32(MemoryExtensions.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase));
			}
			return num;
		}

		internal static string[] GetPhotoNamesFromDirectoryFileNames(string[] directoryFileNames)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < directoryFileNames.Length; i++)
			{
				if (MemoryExtensions.Equals(Path.GetExtension(directoryFileNames[i].AsSpan()), ".png", StringComparison.OrdinalIgnoreCase))
				{
					list.Add(directoryFileNames[i]);
				}
			}
			return list.ToArray();
		}

		private IEnumerator LoadPhotosData()
		{
			string[] photoFilenames = GetPhotoNamesFromDirectoryFileNames(photoDirectoryInfoTransaction.DirectoryFileNames);
			int num = photoFilenames.Length;
			int photosToLoad = Mathf.Min(num - startIndex, maxPhotosToLoad);
			this.phonePictureData = new PhonePictureData[photosToLoad];
			LoadTransaction<PhonePictureData>[] photoLoadTransactions = new LoadTransaction<PhonePictureData>[photosToLoad];
			byte[] photoDataBuffer = new byte[1048576];
			int i = 0;
			while (i < photosToLoad)
			{
				PhonePictureData phonePictureData = new PhonePictureData(photoDataBuffer);
				phonePictureData.pictureFilename = Path.GetFileName(photoFilenames[startIndex + i]);
				photoLoadTransactions[i] = storage.LoadDataFromStorage(phonePictureData, photoFilenames[startIndex + i], photosSubFolder, isLocal: false, DataSerializerFactory.FormatType.RAW_DATA);
				yield return null;
				int num2 = i + 1;
				i = num2;
			}
			int totalLoadedPhotos = 0;
			i = 0;
			while (i < photosToLoad)
			{
				LoadTransaction<PhonePictureData> photoLoadTask = photoLoadTransactions[i];
				yield return DoTransactionASync(photoLoadTask, HandleLoadPhotoDataErrorASync, base.DoTransactionASync);
				int num2;
				if (photoLoadTask.IsSuccess)
				{
					this.phonePictureData[totalLoadedPhotos] = photoLoadTask.Data;
					num2 = totalLoadedPhotos + 1;
					totalLoadedPhotos = num2;
				}
				num2 = i + 1;
				i = num2;
			}
		}

		private IEnumerator HandleLoadPhotoDataErrorASync(ATransaction loadTransaction, RetryDelegateASync retryDelegateASync, FailureDelegateASync failureDelegateASync)
		{
			if (loadTransaction.Status == ATransaction.TransactionStatus.FILE_NOT_FOUND)
			{
				errorHandlingResult = ErrorHandlingResult.RESOLVED;
			}
			else if (loadTransaction.Status == ATransaction.TransactionStatus.CORRUPTED_DATA)
			{
				LoadTransaction<PhonePictureData> loadTransaction2 = loadTransaction as LoadTransaction<PhonePictureData>;
				DeleteTransaction deleteTransaction = Core.Instance.Platform.Storage.DeleteDataFromStorage(loadTransaction2.FileName, loadTransaction2.DirectoryName, isLocal: false);
				while (!deleteTransaction.IsDone)
				{
					yield return null;
				}
				errorHandlingResult = ErrorHandlingResult.DELETED;
			}
			else
			{
				IError error = errorHandler.HandleCorruptedDataLoadError(loadTransaction, uIManager.Overlay, "Photo data", string.Empty);
				if (error != null)
				{
					yield return new ErrorYieldInstruction(error);
					yield return HandleErrorHandlingResultASync(error, loadTransaction, retryDelegateASync, failureDelegateASync);
				}
			}
		}

		private void HandleFailedLoadingDirectoryInfo(ATransaction transaction)
		{
			errorHandler.HandleLoadTransactionError(transaction, uIManager.Overlay, saveManager, localizer.GetUserInterfaceText("ERROR_PHOTO_DATA"), localizer.GetUserInterfaceText("ERROR_NO_ACCESS_INVESTIGATE"));
			isSuccess = false;
		}
	}
}
