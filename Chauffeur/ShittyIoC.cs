using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;

namespace Chauffeur
{
    class ShittyIoC
    {
        private readonly Dictionary<Type, Func<object>> instanceDependencyMap = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<string, Type> deliverablesByName = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> deliverablesByAlias = new Dictionary<string, Type>();

        public ShittyIoC()
        {
            var deliverables = TypeFinder.FindClassesOfType<Deliverable>();

            foreach (var deliverable in deliverables)
                RegisterDeliverable(deliverable);
        }

        public void Register<T>(Func<object> factory)
        {
            instanceDependencyMap.Add(typeof(T), factory);
        }

        private void RegisterDeliverable(Type deliverable)
        {
            var name = deliverable.GetCustomAttribute<DeliverableNameAttribute>();
            deliverablesByName.Add(name.Name, deliverable);

            var aliases = deliverable.GetCustomAttributes<DeliverableAliasAttribute>();
            foreach (var alias in aliases)
                deliverablesByAlias.Add(alias.Alias, deliverable);
        }

        public Deliverable ResolveDeliverableByName(string command)
        {
            var deliverableType = deliverablesByName.ContainsKey(command) ?
                deliverablesByName[command] :
                deliverablesByAlias.ContainsKey(command) ?
                    deliverablesByAlias[command] :
                    deliverablesByName["unknown"];

            return (Deliverable)Resolve(deliverableType);
        }

        private T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type type)
        {
            var resolvedTypeFactory = LookUpDependency(type);
            if (resolvedTypeFactory != null)
                return resolvedTypeFactory();
            
            var resolvedType = type;
            var constructor = resolvedType.GetConstructors().First();
            var parameters = constructor.GetParameters();
 
            if (!parameters.Any())
            {
                return Activator.CreateInstance(resolvedType);
            }
            else
            {
                return constructor.Invoke(
                    ResolveParameters(parameters).ToArray()
                );
            }
        }
 
        private Func<object> LookUpDependency(Type type)
        {
            if (instanceDependencyMap.ContainsKey(type))
                return instanceDependencyMap[type];
            return null;
        }
 
        private IEnumerable<object> ResolveParameters(IEnumerable<ParameterInfo> parameters)
        {
            return parameters
                .Select(p => Resolve(p.ParameterType))
                .ToList();
        }
    }
}
