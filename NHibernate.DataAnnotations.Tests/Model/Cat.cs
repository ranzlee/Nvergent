using System.ComponentModel.DataAnnotations;
using Iesi.Collections.Generic;

namespace NHibernate.DataAnnotations.Tests.Model
{
    public class Cat
    {
        public const string NameLengthValidation = "Cats can't remember names longer than 10 characters";

        public virtual int Id { get; protected set; }

        [StringLength(10, ErrorMessage = NameLengthValidation)]
        public virtual string Name { get; set; }

        private ISet<Cat> _kittens = new HashedSet<Cat>();
        public virtual ISet<Cat> Kittens
        {
            get { return _kittens; }
            set { _kittens = value; }
        }

        public virtual Cat Parent { get; set; }
    }
}