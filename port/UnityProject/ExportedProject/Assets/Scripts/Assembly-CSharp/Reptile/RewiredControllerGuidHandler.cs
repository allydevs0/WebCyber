using System;
using Rewired;

namespace Reptile
{
	public class RewiredControllerGuidHandler : IRewiredControllerGuidHandler, IDisposable
	{
		protected InputControllerGuidData inputControllerGuidData;

		private APlatform currentPlatform;

		public RewiredControllerGuidHandler(InputControllerGuidData inputControllerGuidData, APlatform platform)
		{
			this.inputControllerGuidData = inputControllerGuidData;
			currentPlatform = platform;
		}

		public virtual Guid GetControllerGuid(Controller controller)
		{
			return controller.hardwareTypeGuid;
		}

		public virtual Guid GetDefaultControllerGuid()
		{
			return currentPlatform.PlatformDefaultControllerGuid;
		}

		public virtual void Dispose()
		{
		}
	}
}
