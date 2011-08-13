using System;
using NHibernate.Bytecode;
using NHibernate.DependencyInjection.Core;
using NHibernate.Properties;

namespace NHibernate.DependencyInjection
{
    public class BytecodeProvider : AbstractBytecodeProvider
    {
        internal static IEntityProvider EntityInjector { get; private set; }

        public BytecodeProvider(IEntityProvider entityProvider)
        {
            if (entityProvider == null) throw new ArgumentNullException("entityProvider");
            EntityInjector = entityProvider;
        }

        public override IReflectionOptimizer GetReflectionOptimizer(System.Type clazz, IGetter[] getters, ISetter[] setters)
        {
            return new ReflectionOptimizer(clazz, getters, setters);
        }

        public override IProxyFactoryFactory ProxyFactoryFactory
        {
            get { return new ProxyFactoryFactory(); }
        }
    }
}