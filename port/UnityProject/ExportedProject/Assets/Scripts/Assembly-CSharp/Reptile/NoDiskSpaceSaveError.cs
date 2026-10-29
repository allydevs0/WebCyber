namespace Reptile
{
	public class NoDiskSpaceSaveError : ABaseError, ITransactionError
	{
		private ATransaction transaction;

		private Overlay overlay;

		public ATransaction Transaction => transaction;

		public NoDiskSpaceSaveError(ATransaction transaction, Overlay overlay)
			: base(AErrorHandler.ErrorType.NO_DISK_SPACE)
		{
			this.transaction = transaction;
			this.overlay = overlay;
		}

		public override void HandleError()
		{
			overlay.ShowPopup("ERROR_NO_DISK_SPACE", "BT_OK", OnClickedButton);
		}

		private void OnClickedButton()
		{
			ChangeStatus(ErrorHandlingResult.RESOLVED);
			FinishErrorHandling();
		}
	}
}
