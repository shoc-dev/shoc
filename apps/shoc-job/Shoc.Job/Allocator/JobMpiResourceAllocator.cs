using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Shoc.Core;
using Shoc.Core.Kubernetes;
using Shoc.Job.Model;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.Job;

namespace Shoc.Job.Allocator;

/// <summary>
/// The MPI job resource allocator
/// </summary>
public class JobMpiResourceAllocator : JobResourceAllocatorBase
{
    /// <summary>
    /// The default CPU allocation for launcher pod 1 core
    /// </summary>
    public const long DEFAULT_LAUNCHER_CPU = 1000;
    
    /// <summary>
    /// The default Memory allocation for launcher pod 1Gi
    /// </summary>
    public const long DEFAULT_LAUNCHER_MEMORY = 1_073_741_824;

    /// <summary>
    /// The default worker replicas if not deduced otherwise
    /// </summary>
    public const long DEFAULT_WORKER_REPLICAS = 1;
    
    /// <summary>
    /// The default CPU allocation for worker pod 1 core
    /// </summary>
    public const long DEFAULT_WORKER_CPU = 1000;
    
    /// <summary>
    /// The default Memory allocation for worker pod 1Mi
    /// </summary>
    public const long DEFAULT_WORKER_MEMORY = 1_073_741_824;
    
    /// <summary>
    /// The default Nvidia GPU allocation for worker (no GPU by default)
    /// </summary>
    public static readonly long? DEFAULT_WORKER_NVIDIA_GPU = null;
    
    /// <summary>
    /// The default AMD GPU allocation for worker (no GPU by default)
    /// </summary>
    public static readonly long? DEFAULT_WORKER_AMD_GPU = null;

    /// <summary>
    /// The base service for resource handling 
    /// </summary>
    /// <param name="jobResourceFormatter">The resource formatter</param>
    /// <param name="nodes">The nodes</param>
    public JobMpiResourceAllocator(JobResourceFormatter jobResourceFormatter, List<ClusterNodeResourcesModel> nodes) : base(jobResourceFormatter, nodes)
    {
    }

    /// <summary>
    /// Returns new manifest instance with enriched resource definitions
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns></returns>
    public override JobRunManifestModel Allocate(JobRunManifestModel input)
    {
        // make a copy for output
        var output = JsonSerializer.Deserialize<JobRunManifestModel>(JsonSerializer.Serialize(input));

        // initialize default values
        output.Resources ??= new JobRunManifestResourcesModel();
        
        // initialize mpi spec
        output.Spec.Mpi ??= new JobRunManifestSpecMpiModel();
        output.Spec.Mpi.Workers ??= new JobRunManifestSpecMpiWorkersModel();
        output.Spec.Mpi.Workers.DistributionUnit ??= JobTaskMpiDistributionUnits.CPU;
        output.Spec.Mpi.Workers.Resources ??= new JobRunManifestResourcesModel();
        output.Spec.Mpi.Launcher ??= new JobRunManifestSpecMpiLauncherModel();
        output.Spec.Mpi.Launcher.Resources ??= new JobRunManifestResourcesModel();
        output.Spec.Mpi.Launcher.Resources.Cpu ??= output.Spec.Mpi.Launcher.Dedicated ? DEFAULT_LAUNCHER_CPU : null;
        output.Spec.Mpi.Launcher.Resources.Memory ??= output.Spec.Mpi.Launcher.Dedicated ? DEFAULT_LAUNCHER_MEMORY : null;

        // shortcut references
        var launcher = output.Spec.Mpi.Launcher;
        var workers = output.Spec.Mpi.Workers;
        
        Console.WriteLine("Initialized Manifest: {0}", JsonSerializer.Serialize(output, new JsonSerializerOptions{WriteIndented = true}));
        
        // validating total resource requirements
        this.ValidateResourcesRangeValidity(output.Resources);
        
        // validate worker resources requirements 
        this.ValidateResourcesRangeValidity(launcher.Resources);
        
        // validate worker resources requirements 
        this.ValidateResourcesRangeValidity(workers.Resources);

        // validate contradictions between resources
        this.ValidateResourceContradictions(output);
        
        // verify resource proportions if given
        this.ValidateResourceProportions(output);
        
        // if dedicated launcher is requested, ensure it can be scheduled
        if (launcher.Dedicated)
        {
            this.ValidateAnyNodeFeasibility(launcher.Resources);
        }
        
        // ensure a single worker can be scheduled
        this.ValidateAnyNodeFeasibility(workers.Resources);

        // compute proportions
        var computedProportions = this.ComputeProportions(output);
        
        // get integral proportion if any
        var proportion = this.GetIntegralProportion(computedProportions);

        // use valid integral proportion as number of workers (if not given manually)
        if (proportion.HasValue && !workers.Replicas.HasValue)
        {
            workers.Replicas = proportion.Value;
        }
        
        // check if workers computational resources are fixed 
        var workerDefined = IsComputableUnitDefined(workers.Resources);
        
        // worker is defined then we always distribute statically 
        if (workerDefined)
        {
            // enrich starting from worker to total
            var enriched = this.EnrichWorkerFirst(output, computedProportions, proportion);
            
            // validate enriched worker resource again
            this.ValidateAnyNodeFeasibility(enriched.Spec.Mpi.Workers.Resources);
            
            // validate enriched output total resources against the cluster
            this.ValidateClusterFeasibility(enriched.Resources);

            // returns the result
            return enriched;
        }
        
        
        throw ErrorDefinition.Validation(JobErrors.UNKNOWN_ERROR, "Don't supported yet").AsException();

        // return modified output
        return output;
    }
    

    /// <summary>
    /// Enrich the output when workers computation unit is defined
    /// </summary>
    /// <param name="manifest">The manifest</param>
    /// <param name="computedProportions">The computed proportions</param>
    /// <param name="proportion">The defined proportion</param>
    private JobRunManifestModel EnrichWorkerFirst(JobRunManifestModel manifest, JobRunManifestSpecMpiResourceProportions computedProportions, long? proportion)
    {
        // spec shortcuts
        var launcher = manifest.Spec.Mpi.Launcher;
        var workers = manifest.Spec.Mpi.Workers;
        
        // in case if worker is defined initialize replicas by default (if not otherwise determined)
        workers.Replicas ??= proportion ?? DEFAULT_WORKER_REPLICAS;

        // the total resources when defined (subtracted launcher resources)
        var totalMemory = computedProportions.TotalSubtractedMemory;
        var totalCpu = computedProportions.TotalSubtractedCpu;
        var totalNvidiaGpu = computedProportions.TotalSubtractedNvidiaGpu;
        var totalAmdGpu = computedProportions.TotalSubtractedAmdGpu;

        // the replicas defined
        var replicas = workers.Replicas.Value;
        
        // no memory defined in fixed worker
        workers.Resources.Memory ??= totalMemory.HasValue ? totalMemory.Value / replicas : DEFAULT_WORKER_MEMORY;
        workers.Resources.Cpu ??= totalCpu.HasValue ? totalCpu.Value / replicas : DEFAULT_WORKER_CPU;
        workers.Resources.NvidiaGpu ??= totalNvidiaGpu.HasValue ? totalNvidiaGpu.Value / replicas : DEFAULT_WORKER_NVIDIA_GPU;
        workers.Resources.AmdGpu ??= totalAmdGpu.HasValue ? totalAmdGpu.Value / replicas : DEFAULT_WORKER_AMD_GPU;

        // select the smallest defined full unit as a number of slots for the worker
        var slots = workers.Resources.Cpu.Value / SINGLE_CPU_VALUE;

        // use number of Nvidia GPUs as slots if defined and is less than CPU slots
        if (workers.Resources.NvidiaGpu.HasValue && workers.Resources.NvidiaGpu.Value < slots)
        {
            slots = workers.Resources.NvidiaGpu.Value;
        }
        
        // use number of AMD GPUs as slots if defined and is less than CPU slots
        if (workers.Resources.AmdGpu.HasValue && workers.Resources.AmdGpu.Value < slots)
        {
            slots = workers.Resources.AmdGpu.Value;
        }

        // use the minimum of the computed slots and the given slots
        workers.SlotsPerWorker = Math.Min(workers.SlotsPerWorker ?? slots, slots);
        
        // once worker resources are initialized, fill the total missing resources
        manifest.Resources.Memory ??= workers.Resources.Memory * replicas + (launcher.Dedicated ? launcher.Resources.Memory ?? 0 : 0);
        manifest.Resources.Cpu ??= workers.Resources.Cpu * replicas + (launcher.Dedicated ? launcher.Resources.Cpu ?? 0 : 0);
        manifest.Resources.NvidiaGpu ??=  workers.Resources.NvidiaGpu.HasValue ? workers.Resources.NvidiaGpu * replicas + (launcher.Dedicated ? launcher.Resources.NvidiaGpu ?? 0 : 0) : null;
        manifest.Resources.AmdGpu ??=  workers.Resources.AmdGpu.HasValue ? workers.Resources.AmdGpu * replicas + (launcher.Dedicated ? launcher.Resources.AmdGpu ?? 0 : 0) : null;

        return manifest;
    }

    /// <summary>
    /// Validate given resource don't contradict to each other
    /// </summary>
    /// <param name="manifest">The manifest</param>
    private void ValidateResourceContradictions(JobRunManifestModel manifest)
    {
        // self-consistency of resource requirements
        this.ValidateSelfContradiction(manifest.Resources);
        this.ValidateSelfContradiction(manifest.Spec.Mpi.Launcher.Resources);
        this.ValidateSelfContradiction(manifest.Spec.Mpi.Workers.Resources);
        
        // ensure single GPU vendor is involved
        this.ValidateSingleGpuVendor(manifest);
    }

    /// <summary>
    /// Validate the resource proportions if given 
    /// </summary>
    /// <param name="manifest">The manifest</param>
    private void ValidateResourceProportions(JobRunManifestModel manifest)
    {
        // get the proportions
        var proportions = this.ComputeProportions(manifest);
        
        // total cpu is given but negative after subtracting the launcher resources
        if (proportions.TotalSubtractedCpu is <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "The dedicated launcher CPU is too big").AsException();
        }
        
        // total memory is given but negative after subtracting the launcher resources
        if (proportions.TotalSubtractedMemory is <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "The dedicated launcher Memory is too big").AsException();
        }
        
        // total Nvidia GPU is given, but negative after subtracting the launcher resources
        if (proportions.TotalSubtractedNvidiaGpu is <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "The dedicated launcher Nvidia GPU is too big").AsException();
        }
        
        // total AMD GPU is given, but negative after subtracting the launcher resources
        if (proportions.TotalSubtractedAmdGpu is <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "The dedicated launcher AMD GPU is too big").AsException();
        }

        // validate and get integral proportion
        var proportion = GetIntegralProportion(proportions);
        
        // take number of workers (maybe defined)
        var replicas = manifest.Spec.Mpi.Workers.Replicas;
        
        // if both resource-based proportion is known and also predefined number of workers, ensure they match
        if (proportions.DefiniteProportions.Count > 0 && replicas.HasValue && proportions.DefiniteProportions.Values.First() != replicas)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Given resource proportions are contradicting with given number of workers").AsException();
        }

        // make sure if replica is given it should be positive
        if (replicas is <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, $"The replica number should be positive (current: {replicas})").AsException();
        }

        // deduce final proportion
        var finalProportion = replicas ?? proportion ?? DEFAULT_WORKER_REPLICAS;
        
        // total budget (subtracted) is defined, make sure it can be divided to proportion and the result will be a full core at least
        if (proportions.TotalSubtractedCpu.HasValue && proportions.TotalSubtractedCpu.Value % (finalProportion * SINGLE_CPU_VALUE) != 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Proportional distribution of the CPU budget requires resulting full cores").AsException();
        }
        
        // total budget (subtracted) is defined, make sure it can be divided to proportion and the result will be a full core at least
        if (proportions.TotalSubtractedNvidiaGpu.HasValue && proportions.TotalSubtractedNvidiaGpu.Value % finalProportion != 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Proportional distribution of the Nvidia GPU budget requires resulting full cores").AsException();
        }
        
        // total budget (subtracted) is defined, make sure it can be divided to proportion and the result will be a full core at least
        if (proportions.TotalAmdGpu.HasValue && proportions.TotalAmdGpu.Value % finalProportion != 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Proportional distribution of the AMD GPU budget requires resulting full cores").AsException();
        }
        
    }

    /// <summary>
    /// Ensure only one GPU vendor is used if any
    /// </summary>
    /// <param name="manifest">The manifest</param>
    private void ValidateSingleGpuVendor(JobRunManifestModel manifest)
    {
        // check if Nvidia GPU is defined 
        var isNvidia = manifest.Resources.NvidiaGpu is > 0 || 
                       manifest.Spec.Mpi.Launcher.Resources.NvidiaGpu is > 0 ||
                       manifest.Spec.Mpi.Workers.Resources.NvidiaGpu is > 0;

        // check if AMD GPU is defined
        var isAmd = manifest.Resources.AmdGpu is > 0 ||
                    manifest.Spec.Mpi.Launcher.Resources.AmdGpu is > 0 ||
                    manifest.Spec.Mpi.Workers.Resources.AmdGpu is > 0;

        // only one vendor is allowed
        if (isNvidia && isAmd)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Using multiple GPU vendors is not allowed").AsException();
        }
    }

    /// <summary>
    /// Compute proportions between total, launcher and worker resources
    /// </summary>
    /// <param name="manifest">The input manifest</param>
    /// <returns></returns>
    private JobRunManifestSpecMpiResourceProportions ComputeProportions(JobRunManifestModel manifest)
    {
        // the proportions
        var result = new JobRunManifestSpecMpiResourceProportions
        {
            TotalCpu = manifest.Resources.Cpu,
            TotalMemory = manifest.Resources.Memory,
            TotalNvidiaGpu = manifest.Resources.NvidiaGpu,
            TotalAmdGpu = manifest.Resources.AmdGpu,
            LauncherCpu = manifest.Spec.Mpi.Launcher.Dedicated ? manifest.Spec.Mpi.Launcher.Resources.Cpu : null,
            LauncherMemory = manifest.Spec.Mpi.Launcher.Dedicated ? manifest.Spec.Mpi.Launcher.Resources.Memory : null,
            LauncherNvidiaGpu = manifest.Spec.Mpi.Launcher.Dedicated ? manifest.Spec.Mpi.Launcher.Resources.NvidiaGpu : null,
            LauncherAmdGpu = manifest.Spec.Mpi.Launcher.Dedicated ? manifest.Spec.Mpi.Launcher.Resources.AmdGpu : null,
            WorkerCpu = manifest.Spec.Mpi.Workers.Resources.Cpu,
            WorkerMemory = manifest.Spec.Mpi.Workers.Resources.Memory,
            WorkerNvidiaGpu = manifest.Spec.Mpi.Workers.Resources.NvidiaGpu,
            WorkerAmdGpu = manifest.Spec.Mpi.Workers.Resources.AmdGpu
        };

        // take off launcher resources if any
        result.TotalSubtractedCpu = result.TotalCpu.HasValue ? result.TotalCpu.Value - (result.LauncherCpu ?? 0) : null;
        result.TotalSubtractedMemory = result.TotalMemory.HasValue ? result.TotalMemory.Value - (result.LauncherMemory ?? 0) : null;
        result.TotalSubtractedNvidiaGpu = result.TotalNvidiaGpu.HasValue ? result.TotalNvidiaGpu.Value - (result.LauncherNvidiaGpu ?? 0) : null;
        result.TotalSubtractedAmdGpu = result.TotalAmdGpu.HasValue ? result.TotalAmdGpu.Value - (result.LauncherAmdGpu ?? 0) : null;
        
        // the proportions for resources
        result.CpuProportion = result.TotalSubtractedCpu.HasValue && result.WorkerCpu.HasValue ? result.TotalSubtractedCpu.Value * 1.0m / result.WorkerCpu.Value : default(decimal?);
        result.MemoryProportion = result.TotalSubtractedMemory.HasValue && result.WorkerMemory.HasValue ? result.TotalSubtractedMemory.Value * 1.0m / result.WorkerMemory.Value : default(decimal?);
        result.NvidiaGpuProportion = result.TotalSubtractedNvidiaGpu.HasValue && result.WorkerNvidiaGpu.HasValue ? result.TotalSubtractedNvidiaGpu.Value * 1.0m / result.WorkerNvidiaGpu.Value : default(decimal?);
        result.AmdGpuProportion = result.TotalSubtractedAmdGpu.HasValue && result.WorkerAmdGpu.HasValue ? result.TotalSubtractedAmdGpu.Value * 1.0m / result.WorkerAmdGpu.Value : default(decimal?);

        // init definite proportions collection
        result.DefiniteProportions = new Dictionary<string, decimal>();

        // cpu proportion is definite
        if (result.CpuProportion.HasValue)
        {
            result.DefiniteProportions[WellKnownResources.CPU] = result.CpuProportion.Value;
        }
        
        // memory proportion is definite
        if (result.MemoryProportion.HasValue)
        {
            result.DefiniteProportions[WellKnownResources.MEMORY] = result.MemoryProportion.Value;
        }
        
        // nvidia gpu proportion is definite
        if (result.NvidiaGpuProportion.HasValue)
        {
            result.DefiniteProportions[WellKnownResources.NVIDIA_GPU] = result.NvidiaGpuProportion.Value;
        }
        
        // amd gpu proportion is definite
        if (result.AmdGpuProportion.HasValue)
        {
            result.DefiniteProportions[WellKnownResources.AMD_GPU] = result.AmdGpuProportion.Value;
        }
        
        Console.WriteLine($"Proportions: {JsonSerializer.Serialize(result, new JsonSerializerOptions{WriteIndented = true})}");
        
        return result;
    }
    

    /// <summary>
    /// Checks if we should consider resources as defined
    /// </summary>
    /// <param name="resources">The resources</param>
    /// <returns></returns>
    private static string GetDefinedDistributionUnit(JobRunManifestResourcesModel resources)
    {
        // if Nvidia GPU is defined, use as a distribution unit
        if (resources.NvidiaGpu is > 0)
        {
            return JobTaskMpiDistributionUnits.NVIDIA_GPU;
        }
        
        // if AMD GPU is defined, use as a distribution unit
        if (resources.AmdGpu is > 0)
        {
            return JobTaskMpiDistributionUnits.AMD_GPU;
        }
        
        // if CPU is defined, use as a distribution unit
        if (resources.AmdGpu is > 0)
        {
            return JobTaskMpiDistributionUnits.CPU;
        }

        return null;
    }

    /// <summary>
    /// Gets the distribution unit value
    /// </summary>
    /// <param name="unit">The distribution unit</param>
    /// <param name="resources">The resources</param>
    /// <returns></returns>
    private static long GetDistributionValue(string unit, JobRunManifestResourcesModel resources)
    {
        // get the value based on the unit
        var value =  unit switch
        {
            JobTaskMpiDistributionUnits.CPU => resources.Cpu,
            JobTaskMpiDistributionUnits.NVIDIA_GPU => resources.NvidiaGpu,
            JobTaskMpiDistributionUnits.AMD_GPU => resources.AmdGpu,
            _ => throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, $"The '{unit}' is not a valid unit").AsException()
        };
        
        // make sure unit is defined
        if (value is not > 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, $"The value of the unit '{unit}' should positive").AsException();
        }

        return value.Value;
    }

    /// <summary>
    /// For the given resources check if computational unit is already defined
    /// </summary>
    /// <param name="resources">The resources</param>
    /// <returns></returns>
    private bool IsComputableUnitDefined(JobRunManifestResourcesModel resources)
    {
        // no resources at all
        if (resources == null)
        {
            return false;
        }
        
        // one of the computable resources is defined
        return resources.Cpu.HasValue || resources.NvidiaGpu.HasValue || resources.AmdGpu.HasValue;
    }

    /// <summary>
    /// Gets the integral proportion if everything is valid
    /// </summary>
    /// <param name="proportions">The computed proportions</param>
    /// <returns></returns>
    private long? GetIntegralProportion(JobRunManifestSpecMpiResourceProportions proportions)
    {
        // if there are some definite proportions
        if (proportions.DefiniteProportions.Count > 0)
        {
            // take first
            var first = proportions.DefiniteProportions.Values.First();
            
            // make sure all elements are equal to the first
            var allEqual = proportions.DefiniteProportions.Values.All(value => value == first);

            // if there is a deviation, report disproportion
            if (!allEqual)
            {
                throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Given resources are disproportional").AsException();
            }
        }

        // get the proportion if any
        var decimalProportion = proportions.DefiniteProportions.Count > 0 ? proportions.DefiniteProportions.Values.First() : default(decimal?);
        
        // report if there is (total - launcher) < single worker situation 
        if (decimalProportion is < 1)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Given total budget is smaller than given than a single worker budget").AsException();
        }
        
        // ensure that the proportion (if any) is integral
        if (decimalProportion.HasValue && decimal.Truncate(decimalProportion.Value) != decimalProportion.Value)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "The resources proportion is not an integral value").AsException();
        }

        // once ensured proportion is definite and integral, convert to long otherwise null
        return decimalProportion.HasValue ? decimal.ToInt64(decimalProportion.Value) : default(long?); 
    }
}