using System;
using Rewired;

namespace Reptile
{
	public interface IRewiredControllerGuidHandler : IDisposable
	{
		Guid GetControllerGuid(Controller controller);

		Guid GetDefaultControllerGuid();
	}
}
