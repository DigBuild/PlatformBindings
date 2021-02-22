using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigBuildPlatformSourceGen
{
    [Generator]
    public class UniformSourceGenerator : ISourceGenerator
    {
        private const string UniformType = "DigBuildPlatformCS.IUniform";

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var comp = context.Compilation;
            var uniformType = comp.GetTypeByMetadataName(UniformType + "`1")!;
            var uniformInterfaces = new List<InterfaceDeclarationSyntax>();

            foreach (var tree in comp.SyntaxTrees)
            {
                var semanticModel = comp.GetSemanticModel(tree);

                foreach (var itf in tree.GetRoot().DescendantNodesAndSelf().OfType<InterfaceDeclarationSyntax>())
                {
                    if (itf.BaseList == null)
                        continue;

                    foreach (var gns in itf.BaseList.Types.Select(t => t.Type).OfType<GenericNameSyntax>())
                    {
                        var type = semanticModel.GetTypeInfo(gns).Type!;
                        if (type.OriginalDefinition.Equals(uniformType))
                        {
                            uniformInterfaces.Add(itf);
                            break;
                        }
                    }
                }
            }

            if (uniformInterfaces.Count == 0)
                return;

            var sb = new StringBuilder();

            foreach (var itf in uniformInterfaces)
            {
                var parent = itf.Parent as NamespaceDeclarationSyntax;
                if (parent == null) continue;

                var nsName = parent.Name;
                var itfName = itf.Identifier.Text;
                var itfFqn = nsName + "." + itfName;
                var structName = itfName.Substring(1);

                var props = new List<PropertyInfo>();
                foreach (var property in itf.Members.OfType<PropertyDeclarationSyntax>())
                {
                    if (property.AccessorList == null)
                        continue;

                    var semanticModel = comp.GetSemanticModel(property.SyntaxTree);
                    var type = semanticModel.GetTypeInfo(property.Type).Type!;
                    var typeName = type.Name;
                    var ns = type.ContainingNamespace;
                    while (ns != null && ns.Name.Length > 0)
                    {
                        typeName = ns.Name + "." + typeName;
                        ns = ns.ContainingNamespace;
                    }

                    bool hasGetter = false, hasSetter = false;
                    foreach (var accessor in property.AccessorList.Accessors)
                    {
                        switch (accessor.Keyword.Text)
                        {
                            case "get":
                                hasGetter = true;
                                break;
                            case "set":
                                hasSetter = true;
                                break;
                        }
                    }

                    props.Add(new PropertyInfo(
                        property.Identifier.Text,
                        typeName,
                        hasGetter, hasSetter
                    ));
                }

                //[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = { structSize})]
                sb.Append(@$"
namespace {nsName}.GeneratedUniforms
{{
    public struct {structName} : {itfFqn}
    {{");

                foreach (var prop in props)
                {
                    sb.Append(@$"
        private {prop.Type} _{prop.Name};");
                }

                sb.AppendLine();

                foreach (var prop in props)
                {
                    sb.Append(@$"
        public {prop.Type} {prop.Name} {{");
                    if (prop.HasGetter)
                        sb.Append(@$"
            get => _{prop.Name}; ");
                    if (prop.HasSetter)
                        sb.Append(@$"
            set => _{prop.Name} = value;");
                    sb.Append(@$"
        }}");
                }

                sb.Append(@$"
    }}
}}
");
            }

// #if DB_DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif 
//             Debug.WriteLine(sb.ToString());
            context.AddSource("CBPUniforms.cs", sb.ToString());
        }

        private sealed class PropertyInfo
        {
            public readonly string Name;
            public readonly string Type;
            public readonly bool HasGetter, HasSetter;

            public PropertyInfo(string name, string type, bool hasGetter, bool hasSetter)
            {
                Name = name;
                Type = type;
                HasGetter = hasGetter;
                HasSetter = hasSetter;
            }
        }
    }
}