namespace NHibernate.DependencyInjection
{
    public static class Initializer
    {
        public static void RegisterEntityProvider(IEntityProvider provider)
        {
            Cfg.Environment.BytecodeProvider = new BytecodeProvider(provider);
        }

        public static void RegisterEntityProvider()
        {
            Cfg.Environment.BytecodeProvider = new BytecodeProvider();
        }
    }
}