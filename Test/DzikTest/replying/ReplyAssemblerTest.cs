using Dzik.domain;
using Dzik.replying;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DzikTest.replying
{
    [TestClass]
    public class ReplyAssemblerTest
    {
        [TestMethod]
        public void TrivialTextIsAssembled()
        {
            var input = "some text";

            var result = WhenAssembleIsCalled(input);

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("some text\n", result[0].content);
        }

        [TestMethod]
        public void WhenMaxLenIsExceededMsgIsBrokenIntoTwoPieces()
        {
            var input = @"12345
67890";

            var result = WhenAssembleIsCalled(input, 7);

            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual("12345", result[0].content);
            Assert.AreEqual("67890\n", result[1].content);
        }

        [TestMethod]
        public void WhenEncryptionIncreasesLineLengthTheLineIsBrokenIntoPartsProperly()
        {
            var input = @"$12345
67890";

            var result = WhenAssembleIsCalled(input, 17);

            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual("ĘĘĘ############", result[0].content);
            Assert.AreEqual("67890\n", result[1].content);
        }

        [TestMethod]
        public void WhenKeyExchangeResponseInsertionTagIsFoundMessageisProperlyBrokenDownIntoParts()
        {
            var input = @"&INSERT_KEY_AGREEMENT_RESPONSE_HERE
67890";

            var result = WhenAssembleIsCalled(input, 39);

            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual(KEY_AGREEMENT_RESPONSE, result[0].content);
            Assert.AreEqual("67890\n", result[1].content);
        }


        private List<MsgPart> WhenAssembleIsCalled(string input, int maxMsgLen = 100)
        {
            return ReplyAssembler.Assemble(input, maxMsgLen, new DoubleLenMockEncryptor(), new MockExchangeProvider());
        }

        private class DoubleLenMockEncryptor : Encryptor
        {
            public string Encrypt(string plainText)
            {
                var builder = new StringBuilder();
                foreach (var c in plainText.ToCharArray())
                {
                    builder.Append("##");
                }
                return builder.ToString();
            }
        }

        private class MockExchangeProvider : KeyExchangeResponseProvider
        {
            public string GetKeyExchangeResponseOrNull()
            {
                return KEY_AGREEMENT_RESPONSE;
            }
        }

        private const string KEY_AGREEMENT_RESPONSE = "[38-character-long-response-inserted]";
    }
}
