using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AisParser.test
{
    public class ParserTests
    {

        [Fact]
        public void Should_return_null_for_message_with_empty_payload1()
        {
            const string sentence = "!AIVDM,1,1,,B,,0*25";

            var parser = new Parser();

            var result = parser.Parse(sentence);
            result.Should().BeNull();
            //result.Should().BeNull();
        }

        [Fact]
        public void Should_return_null_for_message_with_empty_payload2()
        {
            const string sentence = "!AIVDM,1,1,,A,,0*26";

            var parser = new Parser();

            var result = parser.Parse(sentence);
            result.Should().BeNull();
            //result.Should().BeNull();
        }
    }
}
