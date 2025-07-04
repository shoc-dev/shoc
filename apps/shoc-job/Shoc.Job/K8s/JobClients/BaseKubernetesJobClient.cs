using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Shoc.Core.Kubernetes;
using Shoc.Job.K8s.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s.JobClients;

/// <summary>
/// The kubernetes job client
/// </summary>
public class BaseKubernetesJobClient : KubernetesClientBase, IKubernetesJobClient
{
    /// <summary>
    /// The unspecific container registry
    /// </summary>
    private const string UNSPECIFIED_CONTAINER_REGISTRY = "docker.io";

    /// <summary>
    /// The kubernetes client for job operations
    /// </summary>
    /// <param name="config">The cluster config for authentication</param>
    protected BaseKubernetesJobClient(string config) : base(config)
    {
    }
    
    /// <summary>
    /// Initializes the namespace object for the job
    /// </summary>
    /// <param name="job">The job object</param>
    /// <returns></returns>
    public virtual async Task<InitNamespaceResult> InitNamespace(JobModel job)
    {
        // create default labels 
        var labels = CreateManagedLabels(new ManagedMetadata
        {
            Name = job.Namespace,
            Component = ShocK8sComponents.JOB,
            PartOf = ShocK8sComponents.JOB,
            WorkspaceId = job.WorkspaceId,
            JobId = job.Id
        });

        // make sure istio is disabled in any case for the target namespace of the job
        labels["istio-injection"] = "disabled";
        
        // create a namespace object
        return new InitNamespaceResult
        {
            Namespace = await this.client.CreateNamespaceAsync(new V1Namespace 
            {
                Metadata = new V1ObjectMeta
                {
                    Name = job.Namespace,
                    Labels = labels
                }
            })
        };
    }

    /// <summary>
    /// Initializes the k8s essentials for the given job
    /// </summary>
    /// <param name="job">The job object</param>
    /// <returns></returns>
    public virtual async Task<InitServiceAccountResult> InitServiceAccount(JobModel job)
    {
        // create default labels 
        var labels = CreateManagedLabels(new ManagedMetadata
        {
            Name = job.Id,
            Component = ShocK8sComponents.SECURITY,
            PartOf = ShocK8sComponents.JOB,
            WorkspaceId = job.WorkspaceId,
            JobId = job.Id
        });
        
        // create a ServiceAccount object to work inside the namespace
        var sa = await client.CoreV1.CreateNamespacedServiceAccountAsync(new V1ServiceAccount
        {
            Metadata = new V1ObjectMeta
            {
                Name = K8sConstants.SHOC_SERVICE_ACCOUNT_NAME, 
                NamespaceProperty = job.Namespace,
                Labels = new Dictionary<string, string>(labels)
                {
                    [WellKnownLabels.NAME] = K8sConstants.SHOC_SERVICE_ACCOUNT_NAME
                }
            }
        }, job.Namespace);
        
        // create Role with restrictions (only allows actions within the namespace)
        var role = await client.RbacAuthorizationV1.CreateNamespacedRoleAsync(new V1Role
        {
            Metadata = new V1ObjectMeta
            {
                Name = K8sConstants.SHOC_ROLE_NAME, 
                NamespaceProperty = job.Namespace,
                Labels = new Dictionary<string, string>(labels)
                {
                    [WellKnownLabels.NAME] = K8sConstants.SHOC_ROLE_NAME
                }
            },
            Rules = new List<V1PolicyRule>
            {
                new()
                {
                    ApiGroups = new [] { "" },
                    Resources = new[] { "pods", "services", "configmaps", "secrets", "jobs", "logs" },
                    Verbs = new[] { "get", "list", "watch", "create", "update", "delete" }
                }
            }
        }, job.Namespace);
        
        // create a role binding to add service account to the role
        var roleBinding = await client.RbacAuthorizationV1.CreateNamespacedRoleBindingAsync(new V1RoleBinding
        {
            Metadata = new V1ObjectMeta
            {
                Name = K8sConstants.SHOC_ROLE_BINDING_NAME, 
                NamespaceProperty = job.Namespace,
                Labels = new Dictionary<string, string>(labels)
                {
                    [WellKnownLabels.NAME] = K8sConstants.SHOC_ROLE_BINDING_NAME
                }
            },
            Subjects = new List<Rbacv1Subject>
            {
                new()
                {
                    Kind = "ServiceAccount",
                    Name = sa.Name(),
                    NamespaceProperty = job.Namespace
                }
            },
            RoleRef = new V1RoleRef
            {
                ApiGroup = "rbac.authorization.k8s.io",
                Kind = "Role",
                Name = role.Name()
            }
        }, job.Namespace);

        return new InitServiceAccountResult
        {
            ServiceAccount = sa,
            Role = role,
            RoleBinding = roleBinding
        };
    }
    
    /// <summary>
    /// Initialize the shared environment such as secrets and config maps
    /// </summary>
    /// <param name="job">The job object</param>
    /// <param name="envs">The environment input</param>
    /// <returns></returns>
    public virtual async Task<InitSharedEnvsResult> InitSharedEnvironment(JobModel job, JobTaskEnvModel envs)
    {
        // create default labels 
        var labels = CreateManagedLabels(new ManagedMetadata
        {
            Name = job.Id,
            Component = ShocK8sComponents.ENVIRONMENT,
            PartOf = ShocK8sComponents.JOB,
            WorkspaceId = job.WorkspaceId,
            JobId = job.Id
        });
        
        // creates a new secret for encrypted values
        var secret = await this.client.CreateNamespacedSecretAsync(new V1Secret
        {
            Immutable = null,
            Metadata = new V1ObjectMeta
            {
                Name = K8sConstants.SHOC_SHARED_SECRET_ALL,
                NamespaceProperty = job.Namespace,
                Labels = new Dictionary<string, string>(labels)
                {
                    [WellKnownLabels.NAME] = K8sConstants.SHOC_SHARED_SECRET_ALL
                }
            },
            StringData = envs.Encrypted ?? new Dictionary<string, string>(),
            Type = "Opaque"
        }, job.Namespace);
        
        // creates a new config map for plain values
        var configMap = await this.client.CreateNamespacedConfigMapAsync(new V1ConfigMap
        {
            Immutable = null,
            Metadata = new V1ObjectMeta
            {
                Name = K8sConstants.SHOC_SHARED_CONFIGS_ALL,
                NamespaceProperty = job.Namespace,
                Labels = new Dictionary<string, string>(labels)
                {
                    [WellKnownLabels.NAME] = K8sConstants.SHOC_SHARED_CONFIGS_ALL
                }
            },
            Data = envs.Plain
        }, job.Namespace);

        return new InitSharedEnvsResult
        {
            Secrets = [secret],
            ConfigMaps = [configMap]
        };
    }
    
    /// <summary>
    /// Initialize the pull secret for the target image
    /// </summary>
    /// <param name="job">The job object</param>
    /// <param name="packageReference">The package reference</param>
    /// <returns></returns>
    public virtual async Task<InitPullSecretResult> InitPullSecret(JobModel job, JobTaskPackageReferenceModel packageReference)
    {
        // create default labels 
        var labels = CreateManagedLabels(new ManagedMetadata
        {
            Name = K8sConstants.SHOC_SHARED_PULL_SECRET,
            Component = ShocK8sComponents.SECURITY,
            PartOf = ShocK8sComponents.JOB,
            WorkspaceId = job.WorkspaceId,
            JobId = job.Id
        });
        
        // resolve the registry
        var registry = ResolveRegistry(packageReference.Image);
        
        // build the docker config JSON object
        var dockerConfigJson = JsonSerializer.Serialize(new
        {
            auths = new Dictionary<string, object>
            {
                [registry] = new
                {
                    auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{packageReference.PullUsername}:{packageReference.PullPasswordPlain}"))
                }
            }
        });
        
        // create and return the result
        return new InitPullSecretResult
        {
            PullSecret = await this.client.CreateNamespacedSecretAsync(new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = K8sConstants.SHOC_SHARED_PULL_SECRET,
                    NamespaceProperty = job.Namespace,
                    Labels = labels
                },
                Type = "kubernetes.io/dockerconfigjson",
                Data = new Dictionary<string, byte[]>
                {
                    { ".dockerconfigjson", Encoding.UTF8.GetBytes(dockerConfigJson) }
                }
            }, job.Namespace),
            Image = packageReference.Image
        };
    }

    /// <summary>
    /// Initializes the shared runtime
    /// </summary>
    /// <param name="job">The job object</param>
    /// <returns></returns>
    public virtual Task<InitSharedRuntimeResult> InitSharedRuntime(JobModel job)
    {
        return Task.FromResult(new InitSharedRuntimeResult
        {
            RuntimeConfig = null
        });
    }

    /// <summary>
    /// Performs a K8s action with namespace cleanup fallback
    /// </summary>
    /// <param name="ns">The namespace</param>
    /// <param name="action">The action to perform</param>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <returns></returns>
    public async Task<TResult> WithCleanup<TResult>(string ns, Func<Task<TResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception)
        {
            await this.client.DeleteNamespaceAsync(ns, new V1DeleteOptions
            {
                GracePeriodSeconds = 0
            });
            throw;
        }
    }
    
    /// <summary>
    /// Resolves the registry URL by the given image
    /// </summary>
    /// <param name="image">The image</param>
    /// <returns></returns>
    private static string ResolveRegistry(string image)
    {
        // split the image path
        var parts = image.Split('/');

        // consider unspecified
        if (parts.Length == 1)
        {
            return UNSPECIFIED_CONTAINER_REGISTRY;
        }

        // if it contains a domain (e.g., ghcr.io/org/image or registry.example.com/image)
        return parts[0].Contains('.') || parts[0].Contains(':') ? parts[0] : UNSPECIFIED_CONTAINER_REGISTRY;
    }
    
    /// <summary>
    /// Loads the file with the give path or null if does not exist
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <returns></returns>
    protected static async Task<string> LoadFileOrNull(string path)
    {
        // file does not exist
        if (!File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllTextAsync(path);
    }
    
    /// <summary>
    /// Gets the templates directory
    /// </summary>
    /// <returns></returns>
    protected static string GetScriptsPath(params string[] nested)
    {
        // get the execution directory
        var sourceDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        return Path.Combine(new [] {sourceDirectory}.Union(nested).ToArray());
    }

    /// <summary>
    /// Disposes the client
    /// </summary>
    public void Dispose()
    {
        this.client?.Dispose();
        GC.SuppressFinalize(this);
    }
}