using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shoc.Job.Model.Job;

/// <summary>
/// The distribution units of MPI workers
/// </summary>
public static class JobTaskMpiDistributionUnits
{
    /// <summary>
    /// The distribution should be done by CPU
    /// </summary>
    public const string CPU = "cpu";

    /// <summary>
    /// The distribution should be done by Nvidia GPU
    /// </summary>
    public const string NVIDIA_GPU = "nvidia.com/gpu";
    
    /// <summary>
    /// The distribution should be done by AMD GPU
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
        return typeof(JobTaskMpiDistributionUnits)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => !f.IsInitOnly && f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue() as string)
            .ToHashSet();
    }
}