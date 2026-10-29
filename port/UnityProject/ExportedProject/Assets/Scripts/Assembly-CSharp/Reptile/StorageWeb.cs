using System;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class StorageWeb : AStorage
	{
		public StorageWeb(string storageDirectory) : base(storageDirectory)
		{
		}

		public override void PlatformSaveData<T>(SaveTransaction<T> saveTransaction)
		{
			try
			{
				string dir = Path.GetDirectoryName(saveTransaction.FullPath);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}

				using (FileStream fileStream = File.Create(saveTransaction.FullPath))
				{
					WriteToStream(fileStream, saveTransaction);
				}
				saveTransaction.ChangeStatus(ATransaction.TransactionStatus.SUCCESS);
			}
			catch (Exception ex)
			{
				Debug.LogError("[StorageWeb] SaveData error: " + ex);
				saveTransaction.ChangeStatus(ATransaction.TransactionStatus.EXCEPTION);
			}
		}

		public override void PlatformLoadData<T>(LoadTransaction<T> loadTransaction)
		{
			try
			{
				if (!File.Exists(loadTransaction.FullPath))
				{
					loadTransaction.ChangeStatus(ATransaction.TransactionStatus.FILE_NOT_FOUND);
					return;
				}

				using (FileStream fileStream = File.OpenRead(loadTransaction.FullPath))
				{
					ReadFromStream(fileStream, loadTransaction);
				}
				loadTransaction.ChangeStatus(ATransaction.TransactionStatus.SUCCESS);
			}
			catch (Exception ex)
			{
				Debug.LogError("[StorageWeb] LoadData error: " + ex);
				loadTransaction.ChangeStatus(ATransaction.TransactionStatus.CORRUPTED_DATA);
			}
		}

		public override void PlatformDeleteData(DeleteTransaction deleteTransaction)
		{
			try
			{
				if (File.Exists(deleteTransaction.FullPath))
				{
					File.Delete(deleteTransaction.FullPath);
				}
				deleteTransaction.ChangeStatus(ATransaction.TransactionStatus.SUCCESS);
			}
			catch (Exception ex)
			{
				Debug.LogError("[StorageWeb] DeleteData error: " + ex);
				deleteTransaction.ChangeStatus(ATransaction.TransactionStatus.EXCEPTION);
			}
		}

		public override bool PlatformInitUserSaveData(IUserId userId)
		{
			return true;
		}

		public override void PlatformClearUserSaveData(IUserId userId)
		{
		}

		public override void PlatformStartNextTransaction()
		{
			SetNextTransaction();
		}

		public override void PlatformRetrieveDirectoryInfo(DirectoryInfoTransaction directoryInfoTransaction)
		{
			try
			{
				if (Directory.Exists(storageDirectory))
				{
					string[] files = Directory.GetFiles(storageDirectory);
					string[] fileNames = new string[files.Length];
					for (int i = 0; i < files.Length; i++)
					{
						fileNames[i] = Path.GetFileName(files[i]);
					}
					directoryInfoTransaction.DirectoryFileNames = fileNames;
				}
				else
				{
					directoryInfoTransaction.DirectoryFileNames = new string[0];
				}
				directoryInfoTransaction.ChangeStatus(ATransaction.TransactionStatus.SUCCESS);
			}
			catch (Exception ex)
			{
				Debug.LogError("[StorageWeb] RetrieveDirectoryInfo error: " + ex);
				directoryInfoTransaction.ChangeStatus(ATransaction.TransactionStatus.EXCEPTION);
			}
		}

		public override void PlatformCreateDirectory(DirectoryCreateTransaction directoryCreateTransaction)
		{
			try
			{
				if (!Directory.Exists(directoryCreateTransaction.DirectoryName))
				{
					Directory.CreateDirectory(directoryCreateTransaction.DirectoryName);
				}
				directoryCreateTransaction.ChangeStatus(ATransaction.TransactionStatus.SUCCESS);
			}
			catch (Exception ex)
			{
				Debug.LogError("[StorageWeb] CreateDirectory error: " + ex);
				directoryCreateTransaction.ChangeStatus(ATransaction.TransactionStatus.EXCEPTION);
			}
		}

		public override void PlatformRenameFile(RenameFileTransaction renameFileTransaction)
		{
			try
			{
				if (File.Exists(renameFileTransaction.OldFullPath))
				{
					if (File.Exists(renameFileTransaction.NewFullPath))
					{
						File.Delete(renameFileTransaction.NewFullPath);
					}
					File.Move(renameFileTransaction.OldFullPath, renameFileTransaction.NewFullPath);
				}
				renameFileTransaction.ChangeStatus(ATransaction.TransactionStatus.SUCCESS);
			}
			catch (Exception ex)
			{
				Debug.LogError("[StorageWeb] RenameFile error: " + ex);
				renameFileTransaction.ChangeStatus(ATransaction.TransactionStatus.EXCEPTION);
			}
		}
	}
}
