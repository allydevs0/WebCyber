namespace Reptile
{
	public abstract class ABaseError : IError
	{
		private bool isDone;

		private AErrorHandler.ErrorType errorType;

		private ErrorHandlingResult errorHandlingResult;

		public AErrorHandler.ErrorType ErrorType => errorType;

		public bool IsDone => isDone;

		public ErrorHandlingResult ErrorHandlingResult => errorHandlingResult;

		public event ErrorHandlerFinishedDelegate OnErrorHandlerFinished;

		public ABaseError(AErrorHandler.ErrorType errorType)
		{
			this.errorType = errorType;
		}

		public abstract void HandleError();

		public virtual void UpdateError()
		{
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ABaseError))
			{
				return false;
			}
			return errorType == (obj as ABaseError).errorType;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		protected void ChangeStatus(ErrorHandlingResult errorHandlingResult)
		{
			this.errorHandlingResult = errorHandlingResult;
		}

		protected void FinishErrorHandling()
		{
			isDone = true;
			if (OnErrorHandlerFinished != null)
			{
				OnErrorHandlerFinished(this);
			}
		}
	}
}
