using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shoc.Job.Model.Job;

/// <summary>
/// The distribution strategy of MPI workers resources
/// </summary>
public static class JobTaskMpiDistributionStrategies
{
    /// <summary>
    /// The resource packing strategy (bigger workers when possible)
    /// </summary>
    public const string PACK = "pack";

    /// <summary>
    /// The resource spreading strategy (smaller workers uniformly distributed)
    /// </summary>
    public const string SPREAD = "spread";
    
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
        return typeof(JobTaskMpiDistributionStrategies)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => !f.IsInitOnly && f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue() as string)
            .ToHashSet();
    }
}