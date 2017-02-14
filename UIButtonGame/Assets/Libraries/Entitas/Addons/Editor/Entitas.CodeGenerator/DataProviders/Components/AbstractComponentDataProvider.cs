using System;
using System.Collections.Generic;
using System.Linq;
using Entitas.CodeGenerator.Api;
using System.Reflection;

namespace Entitas.CodeGenerator {

    public abstract class AbstractComponentDataProvider : ICodeGeneratorDataProvider {

        public abstract string name { get; }
        public abstract bool isEnabledByDefault { get; }

        readonly Type[] _types;
        readonly IComponentDataProvider[] _dataProviders;

        protected AbstractComponentDataProvider(IComponentDataProvider[] dataProviders) : this(dataProviders, Assembly.GetAssembly(typeof(IEntity)).GetTypes()) {
        }

        protected AbstractComponentDataProvider(IComponentDataProvider[] dataProviders, Type[] types) {
            _types = types;
            _dataProviders = dataProviders;
        }

        public CodeGeneratorData[] GetData() {
            var dataFromComponents = _types
                .Where(type => type.ImplementsInterface<IComponent>())
                .Where(type => !type.IsAbstract)
                .Select(type => createDataForComponent(type));

            var dataFromNonComponents = _types
                .Where(type => !type.ImplementsInterface<IComponent>())
                .Where(type => !type.IsGenericType)
                .Where(type => hasContexts(type))
                .SelectMany(type => createDataForNonComponent(type));

            var generatedComponentsLookup = dataFromNonComponents.ToLookup(data => data.GetFullComponentName());
            return dataFromComponents
                .Where(data => !generatedComponentsLookup.Contains(data.GetFullComponentName()))
                .Concat(dataFromNonComponents)
                .ToArray();
        }

        ComponentData createDataForComponent(Type type) {
            var data = new ComponentData();
            foreach(var provider in _dataProviders) {
                provider.Provide(type, data);
            }

            return data;
        }

        ComponentData[] createDataForNonComponent(Type type) {
            return getComponentNames(type)
                .Select(componentName => {
                var data = createDataForComponent(type);
                data.SetFullTypeName(componentName.AddComponentSuffix());
                data.SetFullComponentName(componentName.AddComponentSuffix());
                data.SetComponentName(componentName.RemoveComponentSuffix());
                data.SetMemberInfos(new List<PublicMemberInfo> {
                    new PublicMemberInfo(type, "value")
                });

                return data;
            })
                .ToArray();
        }

        bool hasContexts(Type type) {
            return ContextsComponentDataProvider.GetContextNames(type).Length != 0;
        }

        string[] getComponentNames(Type type) {
            var attr = Attribute
                .GetCustomAttributes(type)
                .OfType<CustomComponentNameAttribute>()
                .SingleOrDefault();

            if(attr == null) {
                var nameSplit = type.ToCompilableString().Split('.');
                var componentName = nameSplit[nameSplit.Length - 1].AddComponentSuffix();
                return new[] { componentName };
            }

            return attr.componentNames;
        }
    }
}
