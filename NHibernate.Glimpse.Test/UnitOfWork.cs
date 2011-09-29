using NHibernate.Session;

namespace NHibernate.Glimpse.Test
{
    public static class UnitOfWork
    {
        private static Marshaler _marshaler;

        public static void Initialize(Marshaler marshaler)
        {
            _marshaler = marshaler;
        }

        public static ISession Session
        {
            get { return _marshaler.CurrentSession; }
        }

        public static bool HasSession
        {
            get { return _marshaler.HasSession; }
        }

        public static void Commit()
        {
            _marshaler.Commit();
        }

        public static void End()
        {
            _marshaler.End();
        }
    }
}