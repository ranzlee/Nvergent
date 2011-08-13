using System;
using System.ComponentModel.DataAnnotations;
using Iesi.Collections.Generic;

namespace NHibernate.Glimpse.Test.Models
{
    public class Cat 
    {
        public Cat()
        {
            _kittens = new HashedSet<Cat>();
        }

        public virtual int Id { get; protected set; }

        [Required]
        [RegularExpression("^[A-Za-z]*")]
        public virtual string Name { get; set; }

        [Required]
        public virtual DateTime BirthDate { get; set; }

        [Required]
        public virtual string Gender { get; set; }

        private ISet<Cat> _kittens;
        public virtual ISet<Cat> Kittens
        {
            get { return _kittens; }
            set { _kittens = value; }
        }

        public virtual Cat Parent { get; set; }
    }
}
