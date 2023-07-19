using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Autofac;
using AutoMapper;
using Smooth.IoC.Repository.UnitOfWork;
using Smooth.IoC.UnitOfWork.Interfaces;
using VietGIS.Infrastructure;
using VietGIS.Infrastructure.CustomAttributes;
using VietGIS.Infrastructure.Helpers;
using VietGIS.Infrastructure.Repositories.Session;

namespace HP_Learning.Web
{
    public static class AutofacRegistrar
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.Register(c => new AutofacDbFactory(c.Resolve<IComponentContext>())).As<IDbFactory>().SingleInstance();
            builder.RegisterType<Smooth.IoC.UnitOfWork.UnitOfWork>().As<IUnitOfWork>();
            builder.RegisterType<AppSession>().As<IAppSession>();
            builder.RegisterGeneric(typeof(Repository<,>)).AsSelf();

            var dataAccess = Assembly.GetExecutingAssembly();

            // builder.RegisterAssemblyTypes(AssemblyHelper.GetReferencingAssemblies("VietGIS.Infrastructure").ToArray())
            //        .Where(t => t.IsSubclassOf(typeof(NullRepository)))
            //        .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(AssemblyHelper.GetReferencingAssemblies("VietGIS.Infrastructure").ToArray()).AsClosedTypesOf(typeof(IRepository<,>));

            foreach (var module in GlobalConfiguration.Modules)
            {
                builder.RegisterAssemblyTypes(module.Assembly).AsClosedTypesOf(typeof(IRepository<,>));
            }

            //            builder.Register(
            //                    ctx =>
            //                    {
            //                        var scope = ctx.Resolve<ILifetimeScope>();
            //                        return new Mapper(
            //                            ctx.Resolve<IConfigurationProvider>(),
            //                            scope.Resolve);
            //                    })
            //                .As<IMapper>()
            //                .InstancePerLifetimeScope();
        }

        [NoIoCFluentRegistration]
        sealed class AutofacDbFactory : IDbFactory
        {
            private readonly IComponentContext _container;

            public AutofacDbFactory(IComponentContext container)
            {
                _container = container;
            }

            public T Create<T>() where T : class, ISession
            {
                return _container.Resolve<T>();
            }

            public TUnitOfWork Create<TUnitOfWork, TSession>(IsolationLevel isolationLevel = IsolationLevel.Serializable)
                where TUnitOfWork : class, IUnitOfWork where TSession : class, ISession
            {
                return _container.Resolve<TUnitOfWork>(new NamedParameter("factory", _container.Resolve<IDbFactory>()),
                    new NamedParameter("session", Create<TSession>()), new NamedParameter("isolationLevel", isolationLevel)
                    , new NamedParameter("sessionOnlyForThisUnitOfWork", true));
            }

            public T Create<T>(IDbFactory factory, ISession session, IsolationLevel isolationLevel = IsolationLevel.Serializable) where T : class, IUnitOfWork
            {
                return _container.Resolve<T>(new NamedParameter("factory", factory),
                    new NamedParameter("session", session), new NamedParameter("isolationLevel", isolationLevel));
            }

            public void Release(IDisposable instance)
            {
                instance.Dispose();
            }
        }
    }
}