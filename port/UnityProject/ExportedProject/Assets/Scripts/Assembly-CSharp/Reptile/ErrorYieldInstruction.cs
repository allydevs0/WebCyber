using UnityEngine;

namespace Reptile
{
	public class ErrorYieldInstruction : CustomYieldInstruction
	{
		private IError error;

		public override bool keepWaiting => !error.IsDone;

		public ErrorYieldInstruction(IError error)
		{
			this.error = error;
		}
	}
}
