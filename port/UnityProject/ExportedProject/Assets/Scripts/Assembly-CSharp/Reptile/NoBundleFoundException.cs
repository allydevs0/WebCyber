using System;

namespace Reptile
{
	public class NoBundleFoundException : Exception
	{
		private string assetBundleName = string.Empty;

		public NoBundleFoundException(string assetBundleName)
			: base("Could not find asset bundle with name: " + assetBundleName + ", make sure it is loaded before using it")
		{
			this.assetBundleName = assetBundleName;
		}

		public override string ToString()
		{
			return "Could not find asset bundle with name: " + assetBundleName + ", make sure it is loaded before using it \n" + StackTrace.ToString();
		}
	}
}
