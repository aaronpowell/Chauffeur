using System;
using System.Collections.Generic;

namespace Chauffeur
{
    public interface IContainer
    {
        IRegistrationBuilder Register<T>() where T : class;
        IRegistrationBuilder Register<T>(Func<T> factory) where T : class;
        IRegistrationBuilder Register<T, TAs>();
        void RegisterFrom<T>() where T : IBuildDependencies, new();
        T Resolve<T>();
        object Resolve(Type type);
        Deliverable ResolveDeliverableByName(string command);
        IEnumerable<Deliverable> ResolveAllDeliverables();
    }
}