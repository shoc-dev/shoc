using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shoc.Core.Kubernetes;

/// <summary>
/// The well-known resources
/// </summary>
public static class WellKnownResources
{
    /// <summary>
    /// The gpu resource key
    /// </summary>
    public const string CPU = "cpu";
    
    /// <summary>
    /// The memory resource key
    /// </summary>
    public const string MEMORY = "memory";

    /// <summary>
    /// The nvidia.com gpu resource key
    /// </summary>
    public const string NVIDIA_GPU = "nvidia.com/gpu";

    /// <summary>
    /// The amd.com gpu resource key
    /// </summary>
    public const string AMD_GPU = "amd.com/gpu";
    
    /// <summary>
    /// Get and initialize all the constants
    /// </summary>
    public static readonly ISet<string> ALL = GetAll();

    /// <summary>
    /// Gets all the constant values
    /// </summary>
    /// <returns></returns>
    private static ISet<string> GetAll()
    {
        return typeof(WellKnownResources)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => !f.IsInitOnly && f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue() as string)
            .ToHashSet();
    }
}