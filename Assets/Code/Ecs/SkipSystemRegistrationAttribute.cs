using System;

/// <summary>
/// Excludes an ISystem or IFeature class from the generated GeneratedSystemRegistrar, for types that
/// are constructed with explicit arguments or that only exist for tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SkipSystemRegistrationAttribute : Attribute { }
