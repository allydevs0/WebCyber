using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class ErrorQueue : MonoBehaviour, IInterruptable
	{
		private Queue<IError> errorQueue;

		private IError currentError;

		public bool IsHandlingError => currentError != null;

		public void InterruptWork()
		{
			if (errorQueue != null)
			{
				errorQueue.Clear();
			}
		}

		public void Dispose()
		{
			errorQueue = null;
			currentError = null;
		}

		public bool AddError(IError error)
		{
			if (errorQueue.Contains(error) || (IsHandlingError && currentError.ErrorType == error.ErrorType))
			{
				return false;
			}
			AddToQueue(error);
			return true;
		}

		private void Awake()
		{
			errorQueue = new Queue<IError>();
		}

		private void Update()
		{
			UpdateQueue();
		}

		private void OnDestroy()
		{
			errorQueue = null;
			currentError = null;
		}

		private void AddToQueue(IError error)
		{
			errorQueue.Enqueue(error);
		}

		private void UpdateQueue()
		{
			if (IsHandlingError)
			{
				UpdateHandlingError();
			}
			else
			{
				AttemptHandleNextError();
			}
		}

		private void UpdateHandlingError()
		{
			currentError.UpdateError();
			if (currentError.IsDone)
			{
				currentError = null;
				AttemptHandleNextError();
			}
		}

		private void AttemptHandleNextError()
		{
			HandleNextError();
		}

		private void HandleNextError()
		{
			if (errorQueue.Count != 0)
			{
				currentError = errorQueue.Dequeue();
				currentError.HandleError();
			}
		}
	}
}
