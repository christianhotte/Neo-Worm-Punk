
using System;
using UnityEngine;

//CREDIT: This code is borrowed wholesale from GitHub user Frarees: https://gist.github.com/frarees/9791517

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class MinMaxSliderAttribute : PropertyAttribute
{
	public readonly float min;
	public readonly float max;

	public MinMaxSliderAttribute() : this(0, 1) { }

	public MinMaxSliderAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}