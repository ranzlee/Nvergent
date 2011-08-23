using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using NHibernate.DataAnnotations.Core;
using NHibernate.Type;

namespace NHibernate.DataAnnotations
{
    public class SessionInterceptor : EmptyInterceptor
    {
        private ISession _session;

        private Queue<EntityStateFrame> _entityStateQueue;

        private readonly IDictionary<object, IEnumerable<ValidationResult>> _validationResults = new Dictionary<object, IEnumerable<ValidationResult>>();

        public string ValidationErrorString
        {
            get
            {
                var validationErrorsStringBuilder = new StringBuilder();
                foreach (var r in _validationResults)
                {
                    foreach(var ve in r.Value)
                    {
                        var memberString = ve.MemberNames
                        .Aggregate(string.Empty, (current, member) => current + string.Format("{0}, ", member));
                        var errorString = string.Format("{0}{1};",
                                                (string.IsNullOrEmpty(memberString))
                                                    ? string.Empty
                                                    : string.Format("[{0}] ", memberString.TrimEnd(new[] { ' ', ',' })),
                                                ve.ErrorMessage);
                        if (validationErrorsStringBuilder.ToString().Contains(errorString)) continue;
                        validationErrorsStringBuilder.Append(errorString);    
                    }   
                }
                return validationErrorsStringBuilder.ToString();
            }
        }

        public IDictionary<object, ReadOnlyCollection<ValidationResult>> GetValidationResults()
        {
            return _validationResults
                .Keys
                .ToDictionary(o => o, o => new ReadOnlyCollection<ValidationResult>(new List<ValidationResult>(_validationResults[o])));
        }

        public ReadOnlyCollection<ValidationResult> GetValidationResults(object o)
        {
            return !_validationResults.ContainsKey(o) 
                ? new ReadOnlyCollection<ValidationResult>(new List<ValidationResult>()) 
                : new ReadOnlyCollection<ValidationResult>(new List<ValidationResult>(_validationResults[o]));
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
        {
            InitializeEntityStateQueue();
            _entityStateQueue.Enqueue(new EntityStateFrame(entity, id, PersistenceOperationEnum.Updating, propertyNames, previousState));
            return base.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types);
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            InitializeEntityStateQueue();
            _entityStateQueue.Enqueue(new EntityStateFrame(entity, id, PersistenceOperationEnum.Adding, null, null));
            return base.OnSave(entity, id, state, propertyNames, types);
        }

        public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            InitializeEntityStateQueue();
            _entityStateQueue.Enqueue(new EntityStateFrame(entity, id, PersistenceOperationEnum.Removing, null, null));
            base.OnDelete(entity, id, state, propertyNames, types);
        }

        private void InitializeEntityStateQueue()
        {
            if (_entityStateQueue == null) _entityStateQueue = new Queue<EntityStateFrame>();
        }

        public override void PreFlush(ICollection entitites)
        {
            if (!ReferenceEquals(_session, null) 
                && !ReferenceEquals(_session.Transaction, null)
                && _session.Transaction.WasRolledBack) _session.Clear();
            base.PreFlush(entitites);
        }

        public override void SetSession(ISession session)
        {
            _session = session;
            base.SetSession(session);
        }

        public override void PostFlush(ICollection entities)
        {
            if (_entityStateQueue != null)
            {
                while (_entityStateQueue.Count > 0)
                {
                    var entityStateFrame = _entityStateQueue.Dequeue();
                    ValidateEntity(entityStateFrame.Entity, entityStateFrame.Id, entityStateFrame.PersistenceOperation, entityStateFrame.PropertyNames, entityStateFrame.PreviousState);
                }
                _entityStateQueue = null;
            }
            base.PostFlush(entities);
        }

        public override void AfterTransactionCompletion(ITransaction tx)
        {
            _entityStateQueue = null;
            base.AfterTransactionCompletion(tx);
        }

        private void ValidateEntity(object o, object id, PersistenceOperationEnum persistenceOperation, IList<string> properties, IList<object> previousState)
        {
            if (ReferenceEquals(_session, null)) return;
            if (!ReferenceEquals(_session.Transaction, null) && _session.Transaction.WasRolledBack) return;
            var factoryName = _session.GetSessionImplementation().Factory.Settings.SessionFactoryName;
            var epc = new EntityPersistenceContext {FactoryName = factoryName, Id = id};
            switch (persistenceOperation)
            {
                case PersistenceOperationEnum.Adding:
                    epc.IsBeingAdded = true;
                    break;
                case PersistenceOperationEnum.Updating:
                    epc.IsBeingModified = true;
                    if (properties == null || previousState == null) break;
                    for (var i = 0; i < properties.Count; i++ )
                    {
                        epc.PreviousState.Add(properties[i], previousState[i]);    
                    }
                    break;
                case PersistenceOperationEnum.Removing:
                    epc.IsBeingRemoved = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(persistenceOperation.ToString());
            }
            var validationResults = EntityValidator.DoMemberValidation(o, epc).ToList();
            if (validationResults.Count() == 0)
            {
                if (_validationResults.ContainsKey(o))
                {
                    //remove the validation errors from a previous flush
                    _validationResults.Remove(o);
                }
                return;
            }
            if (!_validationResults.ContainsKey(o))
            {
                //add the validation results for the entity
                _validationResults.Add(o, validationResults);
            }
            else
            {
                //replace the validation results for the entity
                _validationResults[o] = validationResults;
            }
        }
    }
}