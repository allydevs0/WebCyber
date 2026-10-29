namespace Reptile
{
	public interface IError
	{
		AErrorHandler.ErrorType ErrorType { get; }

		bool IsDone { get; }

		ErrorHandlingResult ErrorHandlingResult { get; }

		event ErrorHandlerFinishedDelegate OnErrorHandlerFinished;

		void HandleError();

		void UpdateError();
	}
}
