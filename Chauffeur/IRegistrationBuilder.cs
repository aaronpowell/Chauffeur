using System;

namespace Chauffeur
{
    public interface IRegistrationBuilder
    {
        IRegistrationBuilder As<T>() where T : class;
        IRegistrationBuilder WhenCreated(Action<object> action);
    }
}