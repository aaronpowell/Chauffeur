using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;

namespace Chauffeur
{
    internal class ShittyRegistration
    {
        public ResolutionType ResolutionType { get; set; }
        public Func<object> Factory { get; set; }
        public Type Type { get; set; }

        public Action<object> AfterCreation { get; set; }
    }

    internal enum ResolutionType
    {
        Factory,
        Reflection
    }

    internal class ShittyRegistrationBuilder : IRegistrationBuilder
    {
        private readonly ShittyRegistration registration;
        private readonly ShittyIoC container;
        public ShittyRegistrationBuilder(ShittyRegistration registration, ShittyIoC container)
        {
            this.registration = registration;
            this.container = container;
        }

        public IRegistrationBuilder WhenCreated(Action<object> action)
        {
            registration.AfterCreation = action;

            return this;
        }

        public IRegistrationBuilder As<T>() where T : class
        {
            container.Register<T>(registration);
            return this;
        }
    }

    internal class ShittyIoC : IContainer
    {
        private readonly Dictionary<object, ShittyRegistration> instanceDependencyMap = new Dictionary<object, ShittyRegistration>();

        public ShittyIoC()
        {
            var deliverables = TypeFinder.FindClassesOfType<Deliverable>();

            foreach (var deliverable in deliverables)
                RegisterDeliverable(deliverable);
        }

        public IRegistrationBuilder Register<T, TAs>()
        {
            var reg = new ShittyRegistration
            {
                Type = typeof(T),
                ResolutionType = ResolutionType.Reflection
            };

            instanceDependencyMap.Add(typeof(TAs), reg);

            return new ShittyRegistrationBuilder(reg, this);
        }

        public IRegistrationBuilder Register<T>() where T : class
        {
            return Register<T, T>();
        }

        public void RegisterFrom<T>()
            where T : IBuildDependencies, new()
        {
            new T().Build(this);
        }

        internal void RegisterFrom(Type builder)
        {
            ((IBuildDependencies)Activator.CreateInstance(builder)).Build(this);
        }

        public IRegistrationBuilder Register<T>(Func<T> factory)
            where T : class
        {
            var reg = new ShittyRegistration
            {
                Type = typeof(T),
                ResolutionType = ResolutionType.Factory,
                Factory = (Func<object>)factory
            };

            instanceDependencyMap.Add(typeof(T), reg);

            return new ShittyRegistrationBuilder(reg, this);
        }

        private void RegisterDeliverable(Type deliverable)
        {
            var registration = new ShittyRegistration
            {
                Type = deliverable,
                ResolutionType = ResolutionType.Reflection
            };

            var name = deliverable.GetCustomAttribute<DeliverableNameAttribute>();
            instanceDependencyMap.Add(name.Name, registration);

            var aliases = deliverable.GetCustomAttributes<DeliverableAliasAttribute>();
            foreach (var alias in aliases)
                instanceDependencyMap.Add(alias.Alias, registration);
        }

        public Deliverable ResolveDeliverableByName(string command)
        {
            var deliverableType = instanceDependencyMap.ContainsKey(command) ?
                instanceDependencyMap[command] :
                instanceDependencyMap.ContainsKey(command) ?
                    instanceDependencyMap[command] :
                    instanceDependencyMap["unknown"];

            return (Deliverable)Resolve(deliverableType);
        }

        public IEnumerable<Deliverable> ResolveAllDeliverables()
        {
            return instanceDependencyMap.Select(x => x.Value).Distinct().Select(Resolve).OfType<Deliverable>();
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var registration = LookUpDependency(type);
            return Resolve(registration);
        }

        private object Resolve(ShittyRegistration registration)
        {
            if (registration == null)
                return null;

            if (registration.ResolutionType == ResolutionType.Factory)
                return registration.Factory();

            if (registration.Type.IsInterface)
                return null;

            var resolvedType = registration.Type;
            var constructor = resolvedType
                .GetConstructors()
                .OrderBy(x => x.GetParameters().Count())
                .Where(ctor => !ctor.GetParameters().Any(pt => pt.ParameterType == typeof(string)))
                .Where(ctor => !ctor.GetParameters().Any(pt => pt.ParameterType == typeof(bool)))
                .LastOrDefault();

            if (constructor == null)
                throw new EntryPointNotFoundException($"Unable to create an instance of {resolvedType.Name} as we cannot find a single public constructor for it. Make sure you have at least 1 public constructor (we'll pick the one with most arguments though)");

            var parameters = constructor.GetParameters();

            object instance;

            if (!parameters.Any())
            {
                instance = Activator.CreateInstance(resolvedType);
            }
            else
            {
                instance = constructor.Invoke(
                    ResolveParameters(parameters).ToArray()
                );
            }

            registration.AfterCreation?.Invoke(instance);

            return instance;
        }

        private ShittyRegistration LookUpDependency(Type type)
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

        internal void Register<T>(ShittyRegistration registration)
        {
            instanceDependencyMap.Add(typeof(T), registration);
        }
    }

    public interface IBuildDependencies
    {
        void Build(IContainer container);
    }
}
