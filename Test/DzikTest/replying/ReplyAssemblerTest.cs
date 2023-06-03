﻿using Dzik.domain;
using Dzik.replying;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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

        private List<MsgPart> WhenAssembleIsCalled(string input, int maxMsgLen = 100)
        {
            return ReplyAssembler.Assemble(input, maxMsgLen, new DoubleLenMockEncryptor(), new MockExchangeProvider());
        }

        private class DoubleLenMockEncryptor : Encryptor
        {
            public string Encrypt(string plainText)
            {
                return plainText.ToUpper() + plainText.ToUpper();
            }
        }

        private class MockExchangeProvider : KeyExchangeResponseProvider
        {
            public string GetKeyExchangeResponseOrNull()
            {
                return "ęĘę̣ƏĮŕĊį͑ŗ̀͘Ƒ̄Ğŧ̫Ǝ̹͖̦̐̏ŉŌ̥͡ĂƂę̘Ƃ͍ıƌŞėĊĜŀ͆ĸŰĦŔƒčŗďŷĬűşńāĜīŸ̢̘̀ŨąŇžśû͚̯͡Ź̭Ĩ̩Ţĵ̊Ďđœžĭ͏ûŎņŸŭ̜̥̥̓̑͠ĝ̡̘͘ž̾͂ŋŚĪ̙ŀįŝƄ̾ŸƄƁƂě̬Ĉ̍Ō̴ŋŦƐęě̯Ğŝ̰Ĩů͉̊şŻ́ť̈ſİŠďč̩ı̡̝źƋŝ̼üŧċĻ̴̦̌̈͏̶ŎŻŒƏı̛ͤ̅şŽĴėŹ̜͟ś̄͡ĝƁĞ̩͜ęďşŹ͔ŹŴ̤͞Ń̒ƓĠżŢ̻Œū̡ƌ̀ŷ̡̤ĠĳđŪ͘ƌ̗Ű̡͚ŃĕŉŌłĻƎ͈̜̅Ḙ̆̅̑́ſŮĉƂŬĸ̊ĳ̭Ɠś͉ČŰĒſĲ̖̾ċƂş͔͈̖ş̕İ̈Đ̦̕Ɛ̚ŀňüĘƆ͎Ɛňű̺ĕŢĨ̦͛ƋĔŭ̧Ş͖Ǝ̥œ̧̊͐Ɖ̟̅̓Ğħ̳ļŏĲģ̝ŇƏƌ̯͖͜čĪſś̙̓̈́͜ŮƑƍþ̂Ƌħ̬ƀŴń́͠ƃ̨̪͐̑Ŀ͙̙͂ŌăŖ̠̩ĸĊ̭őœşŭōĥŻ̦śŖ̵̵̌͗̊ƈ̫ĜŢĤ̖Ɠ̛̃ė͖Ě̪ŁŸƆĻ̿̂Š̟ŤúƁšş͜ſ̧̐͗ħ̌Š̯ďňͅŪĂũ̿ƃ̹üĽ̤͒Ğŀ̦̺ıĂ̑ĊŨ͕ƅĞ̱ü̹̭̋̑ƈĜùĸ̬űĬū̪ġĤč̓̋ĈŁƇĒƃ̈́ŵ̖ĝŲŒĶ̪̔ͣŬƑ͚̝ğ̓ũīğƉĹ͑̾͝ĖĩĻĝ̉Ÿ͒ŒĦ͔ģ̙Ĵŉ͚ĬęŽŹŏ͑Ƒ͌Ŏđ̘̮̏Ɖă̷̌ƅƂıŠ̍̀̕ůũŤĽþ̂đĩźĦďıŮłĶďƑ̓͟ăĲŮ̖ƍ̤żƇŨ͛̚Ęͤŭ͠ģƐŽŤ̑ąŕŬŷ̤ƎŷƆķş̛Ǝ̧͞ŧŹŦŅō͑Ă̻͌̔śƐ̭Ĩ̞ͅƓ̷̴Ī̘ƏŬńċ̠̀Ɓ͈͓́̿̏̉Ĕ̓ĸŴĘĺ͖͠ń̺Ę̜īͤũ͍̅ĎŢę̝űýűŭį̭̀Ũ̩̃Ŵ͍Ň̠̌ͅĖ̬Ɠ̝ėĊŋūĽČ͙͒̆ŢŚ̝͜ľ̕ġŐħńěƏŞ̸̡͌ķĶĠőź̞͏Ɗ̂Ĝ̈́żčƂŘ̍Ļ̠Ĭ̒̊Ɗńŗ̖̠ļ̶̰̭̐ĤĆ̈ŋęě̛̍ƒŵľė̄Ɔ̦ĞŲ͙ŉ͈̪̔üŐŉű̇ęį̱̏Ĵ̩źĬĄġ̑ĥ̈Ĳ̳̃̽Ũţŷ̱ļƍƌ̡̮̞̅ͅĿĆŕŵ̾ŌŤ̤ƁŉͅĘ̎ƈũ̔́Ąūŕ͚ıŒƈŕ͏ŷŬ̎̽̓̋Ĝ̞Ĺþ̅ģ́œŎ͙Ɔ̉̓şƐŗā͐ƎŬŴ͓ŅĀşŽ͋̎ůĎĕţùƌĹ͙ą̦ĘŵŠŶŶýú̺Ċĸ̟̬͐͆͡ŰıĽ̡Č͎ŤťįłżůĐœĪŝĝ̖̖Ĝ̟ŉŨƏ̱̦ͅĭĚěĻć̆͜ƎŶ̵̈́ƍı͗şƊŞ̝̠̝́ģƅ̺ľƉĩă͓͕ƈ̙́ĺċŌ̶ŢƆőć̷̉̏ĜĻĈ̉ƐŅńĎĝƇĿ͐͝ęŰŲļłĆŖƉ̐Ƌƌ̨þĭű̗Ụ̊ÿĮŽ̮Ƅħ͐͋şă̇īĈĐĳħœ̝̑ĿĩġĊŒŨ̧̽ŹħũŶĤĔ̸ŀį̦̌͢ľ͊́ų̈́İƁĳľĄş̞̃͂ļƅƒő͍ů̯̮ůƑŹĂͅŭ͑ő̦ĜŵĒŒŎŭĽěńƁŠĀ͇͋̍̚ŧįŁüĽƄ͈Ũƈ͐įėĕĹŻź̧͘ŕƌ̣ƅ̥ĖĽĂ̔ĜƑţČ̆͛͝Đ̷̟̕ͅŧąûĬƌĉ̫̇ź͉̝Ĥ̼͠ĺűķĞųƅ͑ŬėƄŧ̸̳̩ĉŤ̋ýł̭̼ŅĸġüĖğĝŖ̩͆ĺįƄ̔ýŽĉķńƄÿŨšşͅƈą̴͎̭͊̈͑͟ŝ͙̿ĽþĈ͔ƅć́ťĄ̥ŭ͊ŢŉŋüŮ̈́č̹̱ͤ̍͐ͅŐ͉Ň̔̍ă̗ĲĤĥĿķ̱͆ŏĴŢěĄ̡̰̿͠͞ͅŝƌė̃Ɔ̞͏͍̗ŧĿĒē͉ğŔ̡̘̻̖ƒė͔þď̟Ľ̸̄͢ŕŖĳĮṳ̆͊Č̭ě̈ż̤ÿŊųĚŷƆƎ͌ŵ̗͉ĉď͙̩̿Ż̿ěį̙ͣňĮę̭ĶŇĪ̔͟ŘŲŷĥ̌Ģŏ͓ĘĖŗĳŞīľ̓ĮĽ͓ͤù̟̆̂Ēű̧źƅŞ̸ū̖ĳŰĶ̻ƒŕ̤̉ĶŽͅŝ̪͐Ş̡ŰĚ͕ĴĬƉĲŮŰ̠ü̊ųťőƒ̌͊ƀƆ̚Ō̎̕İƃŦ͜ŧŜČ͐Ć̣ĝ̢̊ͅźğ̝̆úķŖ̝̻Ůū̚ĚĂ̯Ř̏̒ŻŸġ̪ƅİĒ̪Ř̉Į̨ÿŮŁĂ̵̫̓̊ŚŘů͛Į̓Ǝ̥̝̄ŝùÿƏŊ͑Ƒ̔ē͎͙ýĿ̥ŚĂ͏̅ƌ͕ŮþďīĎ̝Ĺ̓̓Ą͜ƈŷąƆ̭̜̦Ĝłľ̫̉ĶūƂă̭͖ƌÿ̰ņ̯œź̶̹Ĥĳ͗̽Ŋı̞ƊČ̓Ħ̘ţğ͓ūŇ̏ň̃ăƒĞĆƎ̺̯̻šš̫ͅŦūƏ̞ŘşĵƐ̯̀ĩƁħƆŰ̺̎̊ûěúýƉṴ̆̔͡͠łōįŲčĹŪ̧͆Ĕ̫ŗĩƇ̄͛̓ͣ͡ħͤŻ͌Ůł̬ğ̤ľûďŪ͈ŉü͟ŎƏį̡Ű̲̍Ż͛Ş̝͡ēĲĞŀ̬ŎŚůŰľŌ͙ķőŵ̥̟Ɛ͋ďħ̩ýŋŞ̒ļƂřû͈łüÿťķ̈ƁĽŶĴ̭̤͗ͤ͐Ś͕̒į̈́ŊĻũþſĉěṈ́Ŏ͓̂̚żĔţ͡ņŨĨ̝̽ŧƈŁŠ̟Ĉ̑ł̥̑Ůę̄͏̚ƈį͖ŰĝŮƁ̛̊̓Ā͓Ũ̈þ̌ďŐƎŲĈ́ıƊ̦ĆƓŞČļ̼̌̄͢čƃŁĂ͇̝̳Į͆ĪŞ̟̚Ŗ̰ƆŮŚĤ̹͇͑̄͘ͅėĻŐƋĸīşĵŪŪ̮̒ŭōű̟̈́̈ŖȨ̇ſŷ̰̚Ł͙ő̘̙̽īļŒ̔ńĽùƑũĔ̘̥ú̽ͤÿƆƀ̦͗̏̄̐̚ſͣą̇ĵ̰̀ͤŲ̰̉Ŏþ͊ûĪ͖Ēƌ̳Đūą̳Ǝĉ̦Ĩ̧̌ŏĠƍāĮķƇ̶̀̚ƄŴĊͣ͒Ĝ̲̾šŢ͑ĪŃŊ̨̦ͤͅēͣĳĦĞ̹Ƈ͍̽ƃř̺͗ņŠ̙͇Œ͚ĉůĦœ̍čĦŏ͔̂͂ń̛ł̞Ɛ͎͊Ě̅ňėŁƓƄġő͏̼ũ̘̰͈̻Ĕ͎͖ͅűŝ̬̎ĵāƒƄş͖ĮĄļġĎƁŦŗ͇̈́ŒőŴřŕ̺̯̒Ƈ̍͘͢͡ƌĂŷĬ̚ņİęč͕̫̹́̓żƋŅ̮ŭ̀Ɖ̢͕Ƈ̡͂ģ̚ĜžŘłŻňƂĮņƑŞƒƎĞĚ͕͆̆ň̙Ũś̼͛Ĺ̄ž̸ĉćƒŶ̢͟ľŕŁ͇̺ŚƆ̀̃Ŋ̈śú̂þę͊̒Ş̝̏͂̚ĈĚĜ̼̄ĘƏĪĞ͉́̚ÿĢĴƎ̲ğĥ̄ūń̜İŔŬķſŝŷ̮ƑŗƑļ̫͏į̿ŮłİĜ̼ṳ̃̆źěƓ̭Ľ͚ĸķēĦ̖ŷ̓ų̲̥͐ĄĻŘ̨ă̡͕úħ̼ŊƂĀƍāą̄͐ĒĤ̐";
            }
        }
    }
}
