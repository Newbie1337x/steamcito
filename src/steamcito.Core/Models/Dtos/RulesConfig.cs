namespace steamcito.Models.Dtos;

public class RulesConfig
{
    public List<StoreRule> StoreRules { get; set; } = [];
    public List<RoleRule>  RoleRules  { get; set; } = [];
}

/// <summary>A rule that maps a substring in the DLL filename to a store type.</summary>
public class StoreRule
{
    public string Contains { get; set; } = "";
    public string Store    { get; set; } = "";
}

/// <summary>
/// A rule that maps a DLL to a role.
/// A DLL matches this rule when ALL of the following are satisfied:
///   - If <see cref="RequiresSigned"/> is true, the DLL must be signed.
///   - Either <see cref="MatchAny"/> is empty OR at least one matcher in it hits.
///   - Either <see cref="AnyFile"/>  is empty OR at least one sibling file pattern matches.
/// </summary>
public class RoleRule
{
    /// <summary>The role to assign when this rule matches.</summary>
    public string Role { get; set; } = "";

    /// <summary>When true the DLL must carry a valid Authenticode signature.</summary>
    public bool RequiresSigned { get; set; }

    /// <summary>
    /// Optional list of version-info matchers.
    /// If non-empty, at least one must match.
    /// </summary>
    public List<FieldMatcher> MatchAny { get; set; } = [];

    /// <summary>
    /// Optional list of file-name glob patterns (e.g. "SmokeAPI.*").
    /// Matched against sibling files in the same directory as the DLL.
    /// If non-empty, at least one sibling must match.
    /// </summary>
    public List<string> AnyFile { get; set; } = [];
}

/// <summary>Matches a single FileVersionInfo field against a substring.</summary>
public class FieldMatcher
{
    /// <summary>One of: FileDescription, ProductName, LegalCopyright, ProductVersion.</summary>
    public string Field    { get; set; } = "";
    public string Contains { get; set; } = "";
}
