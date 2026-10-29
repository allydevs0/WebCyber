using UnityEngine;

namespace Reptile
{
	public static class ApplicationUtility
	{
		public static void QuitApplication(int exitCode = 0)
		{
			Application.Quit(exitCode);
		}
	}
}
