using System;
using System.Collections.Generic;

namespace Reptile
{
	public static class ContainerExtensions
	{
		public static int FindIndexOf<T>(this List<T> list, T objectToFind, Predicate<T> match = null)
		{
			return list.FindIndex((match != null) ? match : ((Predicate<T>)((T objectInList) => objectInList.Equals(objectToFind))));
		}
	}
}
