using System;
using System.ComponentModel.DataAnnotations;

namespace NHibernate.Glimpse.Test.Models
{
    public class Cat 
    {
        public virtual int Id { get; protected set; }

        [Required]
        [RegularExpression("^[A-Za-z]*")]
        public virtual string Name { get; set; }

        [Required]
        public virtual DateTime BirthDate { get; set; }

        [Required]
        public virtual string Gender { get; set; }   
    }
}
