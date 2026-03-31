using System.Diagnostics;
using System.IO;
using System.Text.Json;
using AuthenticodeExaminer;
using steamcito.Helpers;
using steamcito.Models;
using steamcito.Models.Dtos;
using steamcito.Models.Enum;

namespace steamcito.Services;

public class DllAnalizerService
{
    private readonly RulesConfig _rules;

    public DllAnalizerService(string rulesPath = "rules.json")
    {
        var json = File.ReadAllText(rulesPath);
        _rules = JsonSerializer.Deserialize<RulesConfig>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
    

    public DllAnalizerResult? AnalizeDll(string filePath, DllAnalizerConfig config)
    {
        var fileName = Path.GetFileName(filePath);

        if (!IsTargetDll(fileName))          return null;
        if (!IsPortableExecutable(filePath)) return null;

        var storeType = config.CheckStore ? GetStoreType(fileName) : StoreType.Other;
        var isSigned  = (config.CheckSignature || storeType != StoreType.Other)
                        && IsValidSignature(filePath);
        var role      = GetDllRole(filePath, isSigned, out var configFiles);

        return new DllAnalizerResult
        {
            FilePath    = filePath,
            StoreType   = storeType,
            Role        = role,
            IsSigned    = isSigned,
            ConfigFiles = configFiles,
        };
    }

    // ─── Rule helpers ─────────────────────────────────────────────────────────

    private bool IsTargetDll(string fileName) =>
        _rules.StoreRules.Any(r =>
            fileName.Contains(r.Contains, StringComparison.OrdinalIgnoreCase));

    private StoreType GetStoreType(string fileName)
    {
        var match = _rules.StoreRules
            .FirstOrDefault(r =>
                fileName.Contains(r.Contains, StringComparison.OrdinalIgnoreCase));

        return match is null
            ? StoreType.Other
            : Enum.Parse<StoreType>(match.Store);
    }

    private DllRole GetDllRole(string filePath, bool isSigned, out List<string> configFiles)
    {
        configFiles = [];
        var dir  = Path.GetDirectoryName(filePath) ?? "";
        var info = FileVersionInfo.GetVersionInfo(filePath);

        foreach (var rule in _rules.RoleRules)
        {
            // 1. Signature gate
            if (rule.RequiresSigned && !isSigned) continue;

            // 2. Version-info matchers (matchAny) — if any are declared, at least one must hit
            if (rule.MatchAny.Count > 0 && !rule.MatchAny.Any(m => FieldContains(info, m)))
                continue;

            // 3. Sibling-file patterns (anyFile) — collect ALL matches; skip rule if none found
            List<string> found = [];
            if (rule.AnyFile.Count > 0)
            {
                found = rule.AnyFile
                    .SelectMany(p => Directory.GetFiles(dir, p, SearchOption.TopDirectoryOnly))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (found.Count == 0) continue;
            }

            configFiles = found;
            return Enum.Parse<DllRole>(rule.Role);
        }

        return DllRole.Unknown;
    }

    // ─── Matchers ─────────────────────────────────────────────────────────────

    private static bool FieldContains(FileVersionInfo info, FieldMatcher m)
    {
        var value = m.Field switch
        {
            "FileDescription" => info.FileDescription,
            "ProductName"     => info.ProductName,
            "LegalCopyright"  => info.LegalCopyright,
            "ProductVersion"  => info.ProductVersion,
            _                 => null
        };

        return value?.Contains(m.Contains, StringComparison.OrdinalIgnoreCase) == true;
    }


    // ─── Low-level PE / signature checks ─────────────────────────────────────

    private static bool IsPortableExecutable(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new BinaryReader(stream);
            return reader.ReadUInt16() == 0x5A4D; // "MZ"
        }
        catch { return false; }
    }

    private static bool IsValidSignature(string filePath)
    {
        try { return SignatureHelper.IsWindowsSignatureValid(filePath); }
        catch { return false; }
    }
}
