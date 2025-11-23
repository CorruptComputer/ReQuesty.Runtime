using System;
using System.Reflection;

namespace ReQuesty.Runtime.Helpers;

/// <summary>
///   Helper to replace the ReQuestyGenerated project
/// </summary>
public static class RuntimeHelper
{
    /// <summary>
    ///   Gets the runtime version
    /// </summary>
    /// <returns></returns>
    public static string GetRuntimeVersion()
    {
        Version? version = Assembly.GetAssembly(typeof(RuntimeHelper))?.GetName().Version;

        return version is null
            ? "0.0.0"
            : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
