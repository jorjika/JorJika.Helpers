using FluentAssertions;
using JorJika.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace JorJika.Helpers.Tests
{
    public class ObjectHelperUnitTests
    {
        private Person _personBeforeChange = new Person()
        {
            Id = 1,
            FullName = "John Smith",
            BirthDate = new DateTime(1982, 12, 25),
            MonthlySalary = 1300.00m,
            RegistrationDate = new DateTime(2019, 01, 15, 15, 13, 00),
            Documents = new List<IdentityDocument>()
            {
                {
                    new IdentityDocument()
                    {
                        DocumentId = 110,
                        DocumentType = "IdCard",
                        DocumentNumber = "12AB12345",
                    }
                }
            }
        };

        private Person _personAfterChange = new Person()
        {
            Id = 1,
            FullName = "James Smith",
            BirthDate = new DateTime(1982, 11, 25),
            MonthlySalary = 1200.00m,
            RegistrationDate = new DateTime(2019, 01, 15, 15, 13, 00),
            Documents = new List<IdentityDocument>()
            {
                {
                    new IdentityDocument()
                    {
                        DocumentId = 110,
                        DocumentType = "Passport",
                        DocumentNumber = "12AB11111",
                    }
                }
            }
        };

        [Fact]
        public void Track_changes_between_two_objects_Test()
        {
            var changes = ObjectHelper.TrackChanges(_personBeforeChange, _personAfterChange);

            var fullNameChangeRow = changes.Where(c => c.FieldName == "FullName").FirstOrDefault();
            
            fullNameChangeRow.Should().NotBeNull();
            fullNameChangeRow.OldValue.Should().Be("John Smith");
            fullNameChangeRow.NewValue.Should().Be("James Smith");

            var documentChangeRow = changes.Where(c => c.Location== "Documents" && c.FieldName == "DocumentNumber").FirstOrDefault();
            documentChangeRow.Should().NotBeNull();
            documentChangeRow.OldValue.Should().Be("12AB12345");
            documentChangeRow.NewValue.Should().Be("12AB11111");
        }


        public class Person
        {
            [Id]
            public int Id { get; set; }

            [ChangeSensitive]
            public string FullName { get; set; }

            [ChangeSensitive]
            [Format(FormatType.Date)]
            public DateTime BirthDate { get; set; }

            [ChangeSensitive]
            [Format(FormatType.Decimal2)]
            public decimal MonthlySalary { get; set; }

            [ChangeSensitive]
            [Format(FormatType.DateTime)]
            public DateTime RegistrationDate { get; set; }

            [ChangeSensitive]
            public List<IdentityDocument> Documents { get; set; }
        }

        public class IdentityDocument
        {
            [Id]
            public int DocumentId { get; set; }

            [ChangeSensitive]
            public string DocumentType { get; set; }

            [ChangeSensitive]
            public string DocumentNumber { get; set; }
        }
    }
}
