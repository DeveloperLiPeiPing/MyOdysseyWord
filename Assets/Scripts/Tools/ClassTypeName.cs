using System;
using UnityEngine;

namespace Odyssey.Script
{
	public class ClassTypeName : PropertyAttribute
	{
		public Type type;

		public ClassTypeName(Type type)
		{
			this.type = type;
		}
	}
}
