using System;
using UnityEngine;

namespace Reptile
{
	public class ApplyToChildAttribute : PropertyAttribute
	{
		public Type ChildrenType;

		public string propertyName;

		public ApplyToChildAttribute(Type typeOfChildrenToApplyTo, string propertyNameOfVariable = null)
		{
			ChildrenType = typeOfChildrenToApplyTo;
			propertyName = propertyNameOfVariable;
		}
	}
}
