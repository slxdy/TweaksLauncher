using System;

namespace TweaksLauncher;

/// <summary>
/// Automatically loads a Unity component after the first game scene load
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UnitySingletonAttribute : Attribute
{
}
