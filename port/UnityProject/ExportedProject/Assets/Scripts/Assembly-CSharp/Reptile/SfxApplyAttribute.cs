using System;

namespace Reptile
{
	public class SfxApplyAttribute : ApplyToChildAttribute
	{
		public readonly string SfxSearchString;

		public SfxApplyAttribute(string sfxSearchString, Type typeOfChildrenToApplyTo, string propertyNameOfVariable = null)
			: base(typeOfChildrenToApplyTo, propertyNameOfVariable)
		{
			SfxSearchString = sfxSearchString;
			ChildrenType = typeOfChildrenToApplyTo;
			propertyName = propertyNameOfVariable;
		}
	}
}
