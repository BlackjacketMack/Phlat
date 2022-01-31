using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Phlatware.Tests
{
    [TestClass]
    public class PhlatSamples
    {
        private Phlat _target;
        private PhlatConfiguration _config;

        public class Base
        {
            public int Id { get; set; }
            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;

                return this.Id == ((Base)obj).Id;
            }

            public override int GetHashCode() =>  Id.GetHashCode();

            public override string ToString() => $"{GetType().Name}:{Id}";
        }

        //Person will be our aggregate root
        public class Person : Base
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public IList<Address> Addresses { get; set; } = new List<Address>();
        }

        //A person can have multiple addresses
        public class Address : Base
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public bool IsShipping { get; set; }
        }

        private Person _sourcePerson;
        private Person _targetPerson;

        [TestInitialize]
        public void Initting()
        {
            _sourcePerson = new Person
            {
                Id = 1,
                Name = "FLAT Stanley",
                Age = 30,
                Addresses = new List<Address>
                {
                    new Address
                    {
                        Id = 1,
                        Street = "Lombard Street",
                        City = "San Francisco",
                        State = "California",
                        IsShipping = false
                    },

                    new Address
                    {
                        Id = 2,
                        Street = "Hollywood Boulevard",
                        City = "Hollywood",
                        State = "California",
                        IsShipping = true
                    },
                }
            };

            _targetPerson = new Person
            {
                Id = 1,
                Name = "Stanley",
                Age = 29,
                Addresses = new List<Address>
                {
                    new Address
                    {
                        Id = 1,
                        Street = "Lombard Street",
                        City = "San Francisco",
                        State = "California",
                        IsShipping = false
                    },

                    new Address
                    {
                        Id = 2,
                        Street = "Hollywood Boulevard",
                        City = "Hollywood",
                        State = "California",
                        IsShipping = true
                    },
                }
            };

            _config = new PhlatConfiguration();

            _config.Configure<Person>((s,t)=>
            {
                t.Name = s.Name;
                t.Age = s.Age;
            }).HasMany(m => m.Addresses);

            _config.Configure<Address>();

            _target = new Phlat(_config);
        }

        [TestMethod]
        public void TestFlatten()
        {
            var result = _target.Flatten(_targetPerson);

            Console.WriteLine(result.ToString());
        }

        [TestMethod]
        public void TestMerge()
        {
            var result = _target.Merge(_sourcePerson, _targetPerson);

            Console.WriteLine(result.ToString());
        }
    }
}
