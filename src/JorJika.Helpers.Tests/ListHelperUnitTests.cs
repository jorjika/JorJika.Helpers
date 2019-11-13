using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace JorJika.Helpers.Tests
{
    public class ListHelperUnitTests
    {
        private List<Person> _personList = new List<Person>() {
            { new Person(1, "John Smith", 28) },
            { new Person(2, "Sam Black", 20) },
            { new Person(3, "Daniel White", 30) },
            { new Person(4, "Jimmy Oxford", 28) },
        };

        [Fact]
        public void Clone_Test()
        {
            var clonedPersonList = _personList.Clone();

            clonedPersonList.First().Id = 5;

            //If first person id is changed, it should not be changed in original list.
            clonedPersonList.First().Id.Should().NotBe(_personList.First().Id);
        }

        [Fact]
        public void In_Clause_Test()
        {
            _personList.Where(p => p.Age.In(28, 30)).Count().Should().Be(3);
        }

        [Fact]
        public void In_Clause_Test2()
        {
            _personList.Where(p => p.FullName.In("Sam Black", "Jimmy Oxford")).Count().Should().Be(2);
        }

        [Serializable()]
        public class Person
        {
            public Person()
            {

            }
            public Person(int id, string fullName, int age)
            {
                Id = id;
                FullName = fullName;
                Age = age;
            }

            public int Id { get; set; }
            public string FullName { get; set; }
            public int Age { get; set; }
        }
    }
}
