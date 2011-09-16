namespace NHibernate.DataAnnotations
{
    /// <summary>
    /// Defines whether or not to cascade entity validation to this type. Typically, this interface is implemented
    /// on composite ID classes and value components
    /// </summary>
    public interface IEntityComponent
    {
        bool CascadeValidation { get; }
    }
}