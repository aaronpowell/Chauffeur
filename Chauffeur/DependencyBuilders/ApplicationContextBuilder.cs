using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class ApplicationContextBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register(CacheHelper.CreateDisabledCacheHelper);
            container.Register<ServiceContext>().WhenCreated(o =>
            {
                var ctx = (ServiceContext)o;

                try
                {
                    var ms = ctx.MemberService;
                }
                catch (NullReferenceException)
                {
                    var type = ctx.GetType();
                    var f = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                        .FirstOrDefault(x => x.FieldType == typeof(Lazy<IMemberService>));

                    if (f != null)
                    {
                        f.SetValue(ctx, new Lazy<IMemberService>(container.Resolve<IMemberService>));
                    }
                }

                try
                {
                    var us = ctx.UserService;
                }
                catch (NullReferenceException)
                {
                    var type = ctx.GetType();
                    var f = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                        .FirstOrDefault(x => x.FieldType == typeof(Lazy<IUserService>));

                    if (f != null)
                    {
                        f.SetValue(ctx, new Lazy<IUserService>(container.Resolve<IUserService>));
                    }
                }
            });

            container.Register<ApplicationContext>();

            ApplicationContext.EnsureContext(container.Resolve<ApplicationContext>(), true);
        }
    }
}
