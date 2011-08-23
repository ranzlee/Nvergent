using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using NHibernate.DataAnnotations.Core;

namespace NHibernate.DataAnnotations
{
    public class SessionAuditor : ISessionAuditor
    {
        private readonly SessionInterceptor _sessionInterceptor;

        internal SessionAuditor(SessionInterceptor sessionInterceptor)
        {
            _sessionInterceptor = sessionInterceptor;
        }

        public void Eval(ITransaction transaction, bool throwException = true)
        {
            if (IsValid())
            {
                transaction.Commit();
                return;
            }
            transaction.Rollback();
            if (throwException) ThrowValidationException();
        }

        public bool IsValid()
        {
            return _sessionInterceptor.GetValidationResults().Count == 0;
        }

        public string GetValidationErrorString()
        {
            return _sessionInterceptor.ValidationErrorString;
        }

        public void ThrowValidationException()
        {
            throw new ValidationException(GetValidationErrorString());
        }

        public IDictionary<object, ReadOnlyCollection<ValidationResult>> GetValidationResults()
        {
            return _sessionInterceptor.GetValidationResults();
        }

        public ReadOnlyCollection<ValidationResult> GetValidationResults(object o)
        {
            return _sessionInterceptor.GetValidationResults(o);
        }
    }
}