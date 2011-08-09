using NHibernate.Properties;

namespace NHibernate.DependencyInjection.Core
{
    internal class ReflectionOptimizer : Bytecode.Lightweight.ReflectionOptimizer
    {
        internal ReflectionOptimizer
            (System.Type mappedType, 
             IGetter[] getters, 
             ISetter[] setters) : base(mappedType, getters, setters) { }

    
        public override object CreateInstance()
        {
            if (ReferenceEquals(mappedType, null)) return base.CreateInstance();
            if (ReferenceEquals(mappedType.FullName, null)) return base.CreateInstance();
            var instance = BytecodeProvider.EntityInjector.CreateInstance(mappedType);
            return instance ?? base.CreateInstance();
        }

        protected override void ThrowExceptionForNoDefaultCtor(System.Type type)
        {
            if (ReferenceEquals(BytecodeProvider.EntityInjector.CreateInstance(type), null))
            {
                base.ThrowExceptionForNoDefaultCtor(type);
            }
        }
    }
}