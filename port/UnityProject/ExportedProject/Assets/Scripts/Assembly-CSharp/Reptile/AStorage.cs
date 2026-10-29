using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Reptile
{
	public abstract class AStorage : IInterruptable, ISubsystem
	{
		public const string BACKUP_AFFIX = "_backup";

		private const string TRANSACTION_THREAD_NAME = "TransactionThread";

		protected string storageDirectory = string.Empty;

		protected Queue<ATransaction> transactionQueue = new Queue<ATransaction>();

		protected ATransaction activeTransaction;

		private Thread transactionThread;

		private bool isApplicationRunning = true;

		internal bool isUserSaveDataInitialized;

		public bool IsBusy => activeTransaction != null;

		public bool IsAnyTransactionPending => transactionQueue.Count > 0;

		public bool IsSaving
		{
			get
			{
				if (IsBusy)
				{
					return activeTransaction.GetType().GetGenericTypeDefinition() == typeof(SaveTransaction<>);
				}
				return false;
			}
		}

		public bool IsLoading
		{
			get
			{
				if (IsBusy)
				{
					return activeTransaction.GetType().GetGenericTypeDefinition() == typeof(LoadTransaction<>);
				}
				return false;
			}
		}

		public bool IsAllowedToDoFileOperations
		{
			get
			{
				if (transactionThread != null)
				{
					return transactionThread.IsAlive;
				}
				return false;
			}
		}

		public bool IsDisposed => !isApplicationRunning;

		public AStorage(string storageDirectory)
		{
			this.storageDirectory = storageDirectory;
			transactionThread = new Thread(UpdateStorage);
			transactionThread.IsBackground = true;
			transactionThread.Name = "TransactionThread";
			transactionThread.Start();
		}

		public string GetSubSystemInformation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("** AStorage **");
			stringBuilder.AppendLine("*");
			stringBuilder.AppendLine("* Current transaction: " + ((activeTransaction == null) ? "No transaction active" : activeTransaction.ToString()));
			stringBuilder.AppendLine("* Transactions pending: " + transactionQueue.Count);
			ATransaction[] array = transactionQueue.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.AppendLine("* Transactions in queue: " + array[i].ToString());
			}
			return stringBuilder.ToString();
		}

		public virtual void Dispose()
		{
			TerminateStorageThread();
		}

		public virtual void InterruptWork()
		{
			if (transactionQueue != null)
			{
				transactionQueue.Clear();
			}
		}

		private void UpdateStorage()
		{
			RunStorageLoop();
			CleanUpStorage();
		}

		protected virtual void CleanUpStorage()
		{
			transactionQueue.Clear();
			transactionQueue = null;
		}

		protected void TerminateStorageThread()
		{
			isApplicationRunning = false;
		}

		protected virtual void StorageUpdate()
		{
		}

		private void RunStorageLoop()
		{
			while (isApplicationRunning)
			{
				if (IsBusy)
				{
					HandleActiveTransaction();
				}
				else
				{
					Sleep();
				}
				StorageUpdate();
			}
		}

		private void HandleActiveTransaction()
		{
			if (activeTransaction.IsDone)
			{
				if (transactionQueue.Count > 0)
				{
					ScheduleNextTransaction();
				}
				else
				{
					activeTransaction = null;
				}
			}
		}

		private void Sleep()
		{
			if (transactionQueue.Count > 0)
			{
				ScheduleNextTransaction();
			}
			else
			{
				Thread.Sleep(100);
			}
		}

		public SaveTransaction<T> SaveDataAtStorage<T>(T data, string fileName, string relativeDirectoryName, bool isLocal, DataSerializerFactory.FormatType formatType) where T : class
		{
			if (data == null)
			{
				return null;
			}
			SaveTransaction<T> saveTransaction = CreateSaveTransaction(data, fileName, relativeDirectoryName, isLocal, formatType);
			EnqueueTransaction(saveTransaction);
			return saveTransaction;
		}

		public SaveTransaction<T> SaveBackupDataAtStorage<T>(T data, string fileName, string relativeDirectoryName, bool isLocal, DataSerializerFactory.FormatType formatType) where T : class
		{
			if (data == null)
			{
				return null;
			}
			SaveTransaction<T> saveTransaction = CreateSaveTransaction(data, fileName, relativeDirectoryName, isLocal, formatType);
			saveTransaction.SetTransactionAsBackup();
			EnqueueTransaction(saveTransaction);
			return saveTransaction;
		}

		public SaveTransaction<T> SaveDataAsSaveSlotAtStorage<T>(T data, string fileName, string relativeDirectoryName, DataSerializerFactory.FormatType formatType) where T : class
		{
			if (data == null)
			{
				return null;
			}
			SaveTransaction<T> saveTransaction = CreateSaveTransaction(data, fileName, relativeDirectoryName, isLocal: false, formatType);
			saveTransaction.SetTransactionAsSaveSlot();
			EnqueueTransaction(saveTransaction);
			return saveTransaction;
		}

		public SaveTransaction<T> SaveBackupDataAsSaveSlotAtStorage<T>(T data, string fileName, string relativeDirectoryName, DataSerializerFactory.FormatType formatType) where T : class
		{
			if (data == null)
			{
				return null;
			}
			SaveTransaction<T> saveTransaction = CreateSaveTransaction(data, fileName, relativeDirectoryName, isLocal: false, formatType);
			saveTransaction.SetTransactionAsSaveSlot();
			saveTransaction.SetTransactionAsBackup();
			EnqueueTransaction(saveTransaction);
			return saveTransaction;
		}

		private SaveTransaction<T> CreateSaveTransaction<T>(T data, string fileName, string relativeDirectoryName, bool isLocal, DataSerializerFactory.FormatType formatType) where T : class
		{
			string directoryName = Path.Combine(storageDirectory, relativeDirectoryName);
			return new SaveTransaction<T>(data, fileName, directoryName, isLocal, formatType);
		}

		public LoadTransaction<T> LoadDataFromStorage<T>(T data, string fileName, string relativeDirectoryName, bool isLocal, DataSerializerFactory.FormatType formatType) where T : class
		{
			if (data == null)
			{
				return null;
			}
			LoadTransaction<T> loadTransaction = CreateLoadTransaction(data, fileName, relativeDirectoryName, isLocal, formatType);
			EnqueueTransaction(loadTransaction);
			return loadTransaction;
		}

		public LoadTransaction<T> LoadSaveSlotFromStorage<T>(T data, string fileName, string relativeDirectoryName, DataSerializerFactory.FormatType formatType) where T : class
		{
			if (data == null)
			{
				return null;
			}
			LoadTransaction<T> loadTransaction = CreateLoadTransaction(data, fileName, relativeDirectoryName, isLocal: false, formatType);
			loadTransaction.SetTransactionAsSaveSlot();
			EnqueueTransaction(loadTransaction);
			return loadTransaction;
		}

		private LoadTransaction<T> CreateLoadTransaction<T>(T data, string fileName, string directoryName, bool isLocal, DataSerializerFactory.FormatType formatType) where T : class
		{
			string directoryName2 = Path.Combine(storageDirectory, directoryName);
			return new LoadTransaction<T>(data, fileName, directoryName2, isLocal, formatType);
		}

		public void EnqueueTransaction(ATransaction transaction)
		{
			transactionQueue.Enqueue(transaction);
		}

		public static string GetBackupFileName(string fileName)
		{
			return fileName.Insert(fileName.LastIndexOf('.'), "_backup");
		}

		protected void SetTransactionExceptionStatus(ATransaction transaction, Exception exception)
		{
			transaction.Exception = exception;
			transaction.ChangeStatus(ATransaction.TransactionStatus.EXCEPTION);
		}

		public ATransaction CreateDirectoryForTransaction<T>(SaveTransaction<T> transaction) where T : class
		{
			DirectoryCreateTransaction directoryCreateTransaction = new DirectoryCreateTransaction(transaction.DirectoryName, transaction.IsLocal);
			directoryCreateTransaction.StartTransaction(this);
			return directoryCreateTransaction;
		}

		public DirectoryCreateTransaction CreateDirectory(string subDirectoryName, bool isLocal)
		{
			DirectoryCreateTransaction directoryCreateTransaction = new DirectoryCreateTransaction(Path.Combine(storageDirectory, subDirectoryName), isLocal);
			EnqueueTransaction(directoryCreateTransaction);
			return directoryCreateTransaction;
		}

		public RenameFileTransaction RenameFile(string oldFilePath, string newFilePath)
		{
			RenameFileTransaction renameFileTransaction = new RenameFileTransaction(oldFilePath, newFilePath);
			EnqueueTransaction(renameFileTransaction);
			return renameFileTransaction;
		}

		protected void SetNextTransaction()
		{
			ATransaction aTransaction = transactionQueue.Dequeue();
			aTransaction.StartTransaction(this);
			activeTransaction = aTransaction;
		}

		protected void ReadFromStream<T>(Stream stream, LoadTransaction<T> loadTransaction) where T : class
		{
			DataSerializerFactory.CreateFormatSerializer(loadTransaction.FormatType).Read(stream, loadTransaction);
		}

		protected void WriteToStream<T>(Stream stream, SaveTransaction<T> saveTransaction) where T : class
		{
			DataSerializerFactory.CreateFormatSerializer(saveTransaction.FormatType).Write(stream, saveTransaction);
		}

		public DeleteTransaction DeleteDataFromStorage(string fileName, string relativeDirectoryName, bool isLocal)
		{
			DeleteTransaction deleteTransaction = CreateDeleteTransaction(fileName, relativeDirectoryName, isLocal);
			EnqueueTransaction(deleteTransaction);
			return deleteTransaction;
		}

		public DeleteTransaction DeleteDataFromStorage(string filePath, bool isLocal)
		{
			string fileName = Path.GetFileName(filePath);
			string directoryName = Path.GetDirectoryName(filePath);
			DeleteTransaction deleteTransaction = new DeleteTransaction(fileName, directoryName, isLocal);
			EnqueueTransaction(deleteTransaction);
			return deleteTransaction;
		}

		public DeleteTransaction DeleteSaveSlotDataFromStorage(string fileName, string relativeDirectoryName)
		{
			DeleteTransaction deleteTransaction = CreateDeleteTransaction(fileName, relativeDirectoryName, isLocal: false);
			deleteTransaction.SetTransactionAsSaveSlot();
			EnqueueTransaction(deleteTransaction);
			return deleteTransaction;
		}

		private DeleteTransaction CreateDeleteTransaction(string fileName, string directoryName, bool isLocal)
		{
			string directoryName2 = Path.Combine(storageDirectory, directoryName);
			return new DeleteTransaction(fileName, directoryName2, isLocal);
		}

		public DirectoryInfoTransaction GetDirectoryInfoFromStorage(string subDirectory)
		{
			DirectoryInfoTransaction directoryInfoTransaction = CreateDirectoryInfoTransaction(subDirectory);
			EnqueueTransaction(directoryInfoTransaction);
			return directoryInfoTransaction;
		}

		public DirectoryInfoTransaction GetSlotsInfoFromStorage(string subDirectory)
		{
			DirectoryInfoTransaction directoryInfoTransaction = CreateDirectoryInfoTransaction(subDirectory);
			directoryInfoTransaction.SetSaveSlotTransaction();
			EnqueueTransaction(directoryInfoTransaction);
			return directoryInfoTransaction;
		}

		private DirectoryInfoTransaction CreateDirectoryInfoTransaction(string subDirectory)
		{
			return new DirectoryInfoTransaction(Path.Combine(storageDirectory, subDirectory), subDirectory);
		}

		private void ScheduleNextTransaction()
		{
			if (transactionQueue.Count != 0)
			{
				ATransaction aTransaction = transactionQueue.Peek();
				if (aTransaction.IsScheduledForReset)
				{
					aTransaction.ResetTransaction();
				}
				PlatformStartNextTransaction();
			}
		}

		public abstract void PlatformSaveData<T>(SaveTransaction<T> saveTransaction) where T : class;

		public abstract void PlatformLoadData<T>(LoadTransaction<T> loadTransaction) where T : class;

		public abstract void PlatformDeleteData(DeleteTransaction deleteTransaction);

		public abstract bool PlatformInitUserSaveData(IUserId userId);

		public abstract void PlatformClearUserSaveData(IUserId userId);

		public abstract void PlatformStartNextTransaction();

		public abstract void PlatformRetrieveDirectoryInfo(DirectoryInfoTransaction directoryInfoTransaction);

		public abstract void PlatformCreateDirectory(DirectoryCreateTransaction directoryCreateTransaction);

		public abstract void PlatformRenameFile(RenameFileTransaction renameFileTransaction);
	}
}
