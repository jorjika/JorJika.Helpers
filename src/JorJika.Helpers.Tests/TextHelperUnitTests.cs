using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace JorJika.Helpers.Tests
{
    public class TextHelperUnitTests
    {
        [Fact]
        public void Sylfaen_to_latin_direct_translation_Test()
        {
            TextHelper.SylfaenToLatin("გიორგი ჟორჟოლიანი").Should().Be("giorgi JorJoliani");
        }

        [Fact]
        public void Sylfaen_to_latin_correct_translation_Test()
        {
            TextHelper.SylfaenToLatin("გიორგი ჟორჟოლიანი", correctVersion: true).Should().Be("giorgi zhorzholiani");
        }

        [Fact]
        public void Sylfaen_to_latin_correct_translation_fullname_Test()
        {
            TextHelper.SylfaenToLatin("გიორგი ჟორჟოლიანი", correctVersion: true, fullName: true).Should().Be("Giorgi Zhorzholiani");
        }
    }
}
