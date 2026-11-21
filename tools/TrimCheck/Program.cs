// Lists the external types/members a plugin DLL references that are missing from a set of (trimmed) assemblies.
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

string pluginPath = args[0];
string assembliesDir = args[1];

// ---- index of what the trimmed app contains: "Type" -> set of "member signature"
var types = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
foreach (string file in Directory.EnumerateFiles(assembliesDir, "*.dll")) {
	using FileStream fs = File.OpenRead(file);
	using PEReader pe = new(fs);
	if (!pe.HasMetadata) continue;
	MetadataReader md = pe.GetMetadataReader();
	var provider = new NameProvider(md);
	foreach (TypeDefinitionHandle th in md.TypeDefinitions) {
		TypeDefinition td = md.GetTypeDefinition(th);
		string typeName = NameProvider.DefName(md, th);
		if (!types.TryGetValue(typeName, out var members)) types[typeName] = members = new(StringComparer.Ordinal);
		foreach (MethodDefinitionHandle mh in td.GetMethods()) {
			MethodDefinition m = md.GetMethodDefinition(mh);
			var sig = m.DecodeSignature(provider, null);
			members.Add($"M {md.GetString(m.Name)}`{sig.GenericParameterCount}({string.Join(",", sig.ParameterTypes)}):{sig.ReturnType}");
		}
		foreach (FieldDefinitionHandle fh in td.GetFields()) {
			members.Add($"F {md.GetString(md.GetFieldDefinition(fh).Name)}");
		}
	}
}

// ---- what the plugin references
using FileStream pfs = File.OpenRead(pluginPath);
using PEReader ppe = new(pfs);
MetadataReader pmd = ppe.GetMetadataReader();
var pprov = new NameProvider(pmd);
var missing = new SortedSet<string>(StringComparer.Ordinal);
int checkedCount = 0;

foreach (TypeReferenceHandle trh in pmd.TypeReferences) {
	string name = NameProvider.RefName(pmd, trh);
	checkedCount++;
	if (!types.ContainsKey(name)) missing.Add($"TYPE   {name}");
}

foreach (MemberReferenceHandle mrh in pmd.MemberReferences) {
	MemberReference mr = pmd.GetMemberReference(mrh);
	string owner = mr.Parent.Kind switch {
		HandleKind.TypeReference => NameProvider.RefName(pmd, (TypeReferenceHandle) mr.Parent),
		HandleKind.TypeSpecification => pprov.GenericDefinitionOf((TypeSpecificationHandle) mr.Parent),
		_ => ""
	};
	if (owner.Length == 0 || owner.StartsWith("<", StringComparison.Ordinal)) continue;
	string name = pmd.GetString(mr.Name);
	checkedCount++;
	if (!types.TryGetValue(owner, out var members)) { missing.Add($"TYPE   {owner} (for {name})"); continue; }
	string key;
	if (mr.GetKind() == MemberReferenceKind.Method) {
		var sig = mr.DecodeMethodSignature(pprov, null);
		key = $"M {name}`{sig.GenericParameterCount}({string.Join(",", sig.ParameterTypes)}):{sig.ReturnType}";
	} else {
		key = $"F {name}";
	}
	if (!members.Contains(key)) missing.Add($"MEMBER {owner}::{key[2..]}");
}

Console.WriteLine($"checked {checkedCount} references against {types.Count} types");
foreach (string m in missing) Console.WriteLine(m);

// Turns signatures into comparable strings; generic parameters of the owning type stay "!n", of the method "!!n"
sealed class NameProvider(MetadataReader md) : ISignatureTypeProvider<string, object?> {
	public static string DefName(MetadataReader md, TypeDefinitionHandle h) {
		TypeDefinition td = md.GetTypeDefinition(h);
		string name = md.GetString(td.Name);
		TypeDefinitionHandle outer = td.GetDeclaringType();
		if (!outer.IsNil) return $"{DefName(md, outer)}/{name}";
		string ns = md.GetString(td.Namespace);
		return ns.Length > 0 ? $"{ns}.{name}" : name;
	}
	public static string RefName(MetadataReader md, TypeReferenceHandle h) {
		TypeReference tr = md.GetTypeReference(h);
		string name = md.GetString(tr.Name);
		if (tr.ResolutionScope.Kind == HandleKind.TypeReference) return $"{RefName(md, (TypeReferenceHandle) tr.ResolutionScope)}/{name}";
		string ns = md.GetString(tr.Namespace);
		return ns.Length > 0 ? $"{ns}.{name}" : name;
	}
	public string GenericDefinitionOf(TypeSpecificationHandle h) {
		BlobReader blob = md.GetBlobReader(md.GetTypeSpecification(h).Signature);
		var header = blob.ReadSignatureTypeCode();
		if (header != SignatureTypeCode.GenericTypeInstance) return "";
		blob.ReadCompressedInteger();
		EntityHandle generic = blob.ReadTypeHandle();
		return generic.Kind == HandleKind.TypeReference ? RefName(md, (TypeReferenceHandle) generic) : generic.Kind == HandleKind.TypeDefinition ? DefName(md, (TypeDefinitionHandle) generic) : "";
	}
	public string GetPrimitiveType(PrimitiveTypeCode t) => t.ToString();
	public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte k) => DefName(r, h);
	public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte k) => RefName(r, h);
	public string GetTypeFromSpecification(MetadataReader r, object? c, TypeSpecificationHandle h, byte k) => r.GetTypeSpecification(h).DecodeSignature(this, c);
	public string GetSZArrayType(string e) => e + "[]";
	public string GetArrayType(string e, ArrayShape s) => e + "[" + new string(',', s.Rank - 1) + "]";
	public string GetByReferenceType(string e) => e + "&";
	public string GetPointerType(string e) => e + "*";
	public string GetPinnedType(string e) => e;
	public string GetModifiedType(string m, string u, bool r) => u;
	public string GetGenericInstantiation(string g, ImmutableArray<string> a) => g;
	public string GetGenericTypeParameter(object? c, int i) => "!" + i;
	public string GetGenericMethodParameter(object? c, int i) => "!!" + i;
	public string GetFunctionPointerType(MethodSignature<string> s) => "fnptr";
}
