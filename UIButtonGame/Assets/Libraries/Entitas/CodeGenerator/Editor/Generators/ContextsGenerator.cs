using System.Linq;

namespace Entitas.CodeGenerator {

    public class ContextsGenerator : IContextCodeGenerator {

        const string CLASS_TEMPLATE = @"namespace Entitas {{

    public partial class Contexts {{
{0}
        public Context[] allContexts {{ get {{ return new [] {{ {1} }}; }} }}

{2}

        public void SetAllContexts() {{
{3}
        }}
    }}
}}
";

        const string CREATE_CONTEXT_TEMPLATE = @"
        public static Context Create{1}Context() {{
            return CreateContext(""{0}"", {2}.TotalComponents, {2}.componentNames, {2}.componentTypes);
        }}
";

        public CodeGenFile[] Generate(string[] contextNames) {
            var createContextMethods = contextNames.Aggregate(string.Empty, (acc, contextName) =>
                acc + string.Format(CREATE_CONTEXT_TEMPLATE, contextName, contextName.ContextPrefix(), contextName.ContextPrefix() + CodeGenerator.DEFAULT_COMPONENT_LOOKUP_TAG)
            );

            var allContextsList = string.Join(", ", contextNames.Select(contextName => contextName.LowercaseFirst()).ToArray());
            var contextFields = string.Join("\n", contextNames.Select(contextName =>
                "        public Context " + contextName.LowercaseFirst() + ";").ToArray());

            var setAllContexts = string.Join("\n", contextNames.Select(contextName =>
                "            " + contextName.LowercaseFirst() + " = Create" + contextName.ContextPrefix() + "Context();").ToArray());

            return new [] { new CodeGenFile(
                "Contexts",
                string.Format(CLASS_TEMPLATE, createContextMethods, allContextsList, contextFields, setAllContexts),
                GetType().FullName
            )};
        }
    }
}
