using Dzik.domain;
using Dzik.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dzik.replying
{
    internal static class ReplyAssembler
    {

        internal static List<MsgPart> Assemble(string content, Encryptor encryptor, KeyExchangeResponseProvider keyExchangeResponseProvider)
        {
            return Assemble(content, Settings.Default.MsgPartMaxLen, encryptor, keyExchangeResponseProvider);
        }

        internal static List<MsgPart> Assemble(string content, int maxMsgLen, Encryptor encryptor, KeyExchangeResponseProvider keyExchangeResponseProvider)
        {
            var lines = content.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
            var partialLen = 0;
            var part = new StringBuilder();

            var finishedParts = new List<MsgPart>();

            for (int i = 0; i < lines.Count; i++)
            {
                // the line might undergo a transformation prior to being measured and added
                var effectiveLine = lines[i];

                if (effectiveLine.Length + 1 > maxMsgLen)
                {
                    throw new Exception("Line lenght surpasses the max message len limit. Unsupported case!");
                }

                // handle KEY_AGREEMENT_RESPONSE marker
                if (effectiveLine.StartsWith(Constants.MARKER_INSERT_KEY_EXCHANGE_RESPONSE_HERE))
                {
                    var response = keyExchangeResponseProvider.GetKeyExchangeResponseOrNull();
                    if (response != null)
                    {
                        effectiveLine = response;
                    }
                }

                // handle IMG marker
                if (effectiveLine.StartsWith(Constants.MARKER_IMAGE_TAG))
                {
                    // first add the part gathered so far, if any
                    if (part.Length > 0)
                    {
                        AddTrimmedPartOrSkipIfEmpty(part, finishedParts);
                        partialLen = 0;
                        part.Clear();
                    }

                    var imgPart = new MsgPart(effectiveLine, MsgPartType.IMG);
                    finishedParts.Add(imgPart);

                    continue;
                }

                // handle dollar sign marker
                if (effectiveLine.StartsWith(Constants.MARKER_TO_ENCRYPT_TAG))
                {
                    // once one encryption tak is encountered, more are sought after -
                    for (int ii = i + 1; ii < lines.Count; ii++)
                    {
                        if (!lines[ii].StartsWith(Constants.MARKER_TO_ENCRYPT_TAG)) break;
                        effectiveLine += '\n' + lines[ii];
                        // progress to the index of the last encryption marker in row, for loop will add 1 more on top of that later.
                        i = ii;
                    }
                    // just to indicate something is happening, replace with enryption once available.
                    var lineWithTrimmedMarker = effectiveLine.Substring(Constants.MARKER_TO_ENCRYPT_TAG.Length).TrimStart();
                        effectiveLine = Constants.MARKER_TO_DECRYPT_TAG + encryptor.Encrypt(lineWithTrimmedMarker);

                        if (effectiveLine.Length + 1 > maxMsgLen)
                        {
                            throw new Exception("When encrypted, line surpasses the max message len limit. Unsupported case!");
                        }
                    }              

                if (partialLen + effectiveLine.Length > maxMsgLen)
                {
                    AddTrimmedPartOrSkipIfEmpty(part, finishedParts);
                    partialLen = 0;
                    part.Clear();
                }

                // collect line as contribution to the next part
                partialLen += effectiveLine.Length + 1;
                part.Append(effectiveLine + '\n');

            }

            // add last part if anything is left there
            var lastPartString = part.ToString();
            if (lastPartString.TrimEnd().Length > 0)
            {
                finishedParts.Add(new MsgPart(part.ToString(), MsgPartType.TEXT));
            }

            return finishedParts;
        }

        private static void AddTrimmedPartOrSkipIfEmpty(StringBuilder part, List<MsgPart> finishedParts)
        {
            var candidatePart = new MsgPart(part.ToString().Trim(), MsgPartType.TEXT);
            if (candidatePart.content.Length > 0)
            {
                finishedParts.Add(candidatePart);
            }
        }
    }

    internal enum MsgPartType
    {
        TEXT,
        IMG
    }

    internal class MsgPart
    {
        public string content;
        public MsgPartType type;

        internal MsgPart(string content, MsgPartType type)
        {
            this.content = content;
            this.type = type;
        }

        public override string ToString()
        {
            return $"{type}:{content}";
        }
    }
}
