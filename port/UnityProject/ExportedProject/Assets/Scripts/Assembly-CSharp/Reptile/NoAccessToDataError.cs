namespace Reptile
{
	public class NoAccessToDataError : ABaseError, ITransactionError
	{
		private const int MAX_ATTEMPTS = 3;

		private ATransaction transaction;

		private Overlay overlay;

		public ATransaction Transaction => transaction;

		public NoAccessToDataError(ATransaction transaction, Overlay overlay)
			: base(AErrorHandler.ErrorType.NO_ACCESS)
		{
			this.transaction = transaction;
			this.overlay = overlay;
		}

		public override void HandleError()
		{
			if (transaction.Attempts < 3)
			{
				ShowRetryPopup();
			}
			else
			{
				ShowStopPopup();
			}
		}

		private void ShowRetryPopup()
		{
			overlay.ShowPopup("ERROR_NO_ACCESS_EXT", "PROMPT_RETRY", "BT_CANCEL", OnPressedTryAgainButton, OnPressedCancelButton);
		}

		private void ShowStopPopup()
		{
			overlay.ShowPopup("ERROR_NO_ACCESS", "BT_OK", OnPressedCancelButton);
		}

		private void OnPressedTryAgainButton()
		{
			ChangeStatus(ErrorHandlingResult.RETRY);
			FinishErrorHandling();
		}

		private void OnPressedCancelButton()
		{
			ChangeStatus(ErrorHandlingResult.FAILED);
			FinishErrorHandling();
		}
	}
}
