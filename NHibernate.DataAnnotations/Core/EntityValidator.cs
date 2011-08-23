using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NHibernate.DataAnnotations.Core
{
    internal static class EntityValidator
    {
        internal static IEnumerable<ValidationResult> DoMemberValidation(object o, EntityPersistenceContext context)
        {
            //cascade validation through all entity components associated with the entity
            var validationResults = new List<ValidationResult>();
            //validate entity
            var validationContext = GetValidationContext(o, context);
            Validator.TryValidateObject(o,
                validationContext, 
                validationResults, 
                context.ValidateProperties);
            //validate entity components
            var m = o.GetType().GetPropertiesFromCache().ToList();
            var componentProperties = m
                .Where(i => typeof(IEntityComponent).IsAssignableFrom(i.PropertyType))
                .ToList();
            foreach (var vp in componentProperties)
            {
                var vo = vp.GetValue(o, null);
                if (ReferenceEquals(vo, null)) continue;
                if (!((IEntityComponent)vo).CascadeValidation) continue;
                validationContext = GetValidationContext(vo, context);
                Validator.TryValidateObject(vo, 
                                            validationContext, 
                                            validationResults, 
                                            context.ValidateProperties);
            }
            return validationResults;
        }

        private static ValidationContext GetValidationContext(object o, EntityPersistenceContext context)
        {
            var items = new Dictionary<object, object> {{"EntityPersistenceContext", context}};
            return new ValidationContext(o, null, items);
        }
    }
}