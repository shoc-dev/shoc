namespace Shoc.Package.Templating.Model;

/// <summary>
/// The template runtime model
/// </summary>
public class TemplateRuntimeModel
{
    /// <summary>
    /// The default instance of the template runtime
    /// </summary>
    public static readonly TemplateRuntimeModel DEFAULT = new TemplateRuntimeModel
    {
        Type = "function",
        Args = true
    };
    
    /// <summary>
    /// The type of runtime
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Indicates if supplying extra arguments is permitted
    /// </summary>
    public bool Args { get; set; }
    
    /// <summary>
    /// The user id
    /// </summary>
    public long? Uid { get; set; }
    
    /// <summary>
    /// The username
    /// </summary>
    public string User { get; set; }
    
    /// <summary>
    /// The entrypoint array
    /// </summary>
    public string[] Entrypoint { get; set; }
    
    /// <summary>
    /// The implementation 
    /// </summary>
    public string Implementation { get; set; }
}