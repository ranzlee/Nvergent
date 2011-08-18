using System;
using System.ComponentModel.DataAnnotations;
using Iesi.Collections.Generic;

namespace NHibernate.Glimpse.Test.Models
{
    [Serializable]
    public class Cat
    {
        private readonly string _meow;

        public Cat(string meow)
        {
            _meow = meow;
        }

        public virtual string Meow()
        {
            return _meow;
        }

        public virtual int Id { get; protected set; }

        [Required]
        [RegularExpression("^[A-Za-z]*")]
        public virtual string Name { get; set; }

        [Required]
        public virtual DateTime BirthDate { get; set; }

        [Required]
        public virtual string Gender { get; set; }

        private Iesi.Collections.Generic.ISet<Cat> _kittens = new HashedSet<Cat>();
        public virtual Iesi.Collections.Generic.ISet<Cat> Kittens
        {
            get { return _kittens; }
            set { _kittens = value; }
        }

        public virtual Cat Parent { get; set; }
    }
}
