namespace Shoc.Job.K8s;

/// <summary>
/// The Kubernetes constants
/// </summary>
public static class K8sConstants
{
    /// <summary>
    /// The service account name used inside the job namespace
    /// </summary>
    public const string SHOC_SERVICE_ACCOUNT_NAME = "shoc-sa";
    
    /// <summary>
    /// The role name for the shoc service accounts
    /// </summary>
    public const string SHOC_ROLE_NAME = "shoc-role";
    
    /// <summary>
    /// The role binding name for shoc roles
    /// </summary>
    public const string SHOC_ROLE_BINDING_NAME = "shoc-role-binding";
    
    /// <summary>
    /// The shared set of secrets
    /// </summary>
    public const string SHOC_SHARED_SECRET_ALL = "shared-secret-all";

    /// <summary>
    /// The shared set of configs
    /// </summary>
    public const string SHOC_SHARED_CONFIGS_ALL = "shared-configmap-all";
    
    /// <summary>
    /// The shared pull secret
    /// </summary>
    public const string SHOC_SHARED_PULL_SECRET = "shared-pull-secret";

    /// <summary>
    /// The shared runtime config name
    /// </summary>
    public const string SHOC_SHARED_RUNTIME_CONFIG = "shared-runtime-config";
    
    /// <summary>
    /// The shared runtime config name
    /// </summary>
    public const string SHOC_RUNTIME_PATH = "/shoc-runtime";

    /// <summary>
    /// The entrypoint key
    /// </summary>
    public const string SHOC_SHARED_RUNTIME_CONFIG_ENTRYPOINT_KEY = "entrypoint.sh";
    
    /// <summary>
    /// The default version for managed objects
    /// </summary>
    public const string SHOC_DEFAULT_OBJECT_VERSION = "0.0.0";

    /// <summary>
    /// The managed by default value
    /// </summary>
    public const string SHOC_MANAGED_BY = "Shoc";
}