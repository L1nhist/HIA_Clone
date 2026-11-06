using System;
using System.Collections.Generic;
using System.Linq;

namespace HIA.Core.HTML
{
    public class HTMLDocument
    {
        public DOMCollection Head { get; }
        public DOMCollection Body { get; }

        static HTMLDocument()
        { }

        public HTMLDocument()
        {
            Head = new DOMCollection();
            Body = new DOMCollection();
        }

        public static HTMLDocument Parse(string html)
        {
            HTMLDocument doc = new HTMLDocument();
            doc.ParseContent(html);
            return doc;
        }

        public void ParseContent(string html)
        {
            var tokens = Tokenize(html);
            BuildTree(tokens, Body);
        }

        private TagType GetTagType(string token)
        {
            if (token[0] != '<' && token[^1] != '>') return TagType.Text;
            if (token[1] == '/') return TagType.Closing;
            if (token[^2] == '/') return TagType.SelfClosed;
            else return TagType.Opening;
        }

        private string GetTagName(string source)
        {
            return source.Replace("<", "").Replace("/", "").Replace(">", "").Trim();
        }

        private string GetElementID(string source)
        {
            int idx = source.IndexOf(" id=");
            if (idx > 0 && idx + 5 < source.Length)
            {
                char c = source[idx + 4];
                if (c != '\'' && c != '\"')
                {
                    c = ' ';
                }

                int idx1 = source.IndexOf(c, idx + 5);
                if (idx1 > 0) return $"#{source.Substring(idx + 5, idx1 - idx - 5).Trim()}";
            }
            return "";
        }

        private string[] GetClassName(string source)
        {
            int idx = source.IndexOf(" class=");
            if (idx > 0 && idx + 8 < source.Length)
            {
                char c = source[idx + 7];
                if (c != '\'' && c != '\"')
                {
                    c = ' ';
                }

                int idx1 = source.IndexOf(c, idx + 8);
                if (idx1 > 0)
                {
                    string s = source.Substring(idx + 8, idx1 - idx - 8);
                    return s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => $".{s.Trim()}").ToArray();
                }
            }
            return Array.Empty<string>();
        }

        private List<string> Tokenize(string html)
        {
            List<string> tokens = new();
            bool capturing = false;
            string capturedText = "";

            for (int i = 0; i < html.Length; i++)
            {
                char c = html[i];
                if (c == '<')
                {
                    if (capturing && capturedText.Trim() != "")
                    {
                        tokens.Add(capturedText.Trim());
                    }
                    else
                    {
                        capturing = true;
                    }

                    capturedText = "";
                }
                else if (c == '>' || i == html.Length - 1)
                {
                    capturing = false;
                    capturedText += c;
                    tokens.Add(capturedText);
                }
                else if (!capturing)
                {
                    capturedText = "";
                    capturing = true;
                }

                if (capturing)
                {
                    capturedText += c;
                }
            }

            return tokens;
        }

        private void BuildTree(List<string> tokens, DOMCollection elements)
        {
            if (elements == null || tokens == null || tokens.Count < 1) return;

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];
                if (token.Trim() == "") continue;

                string tagName = GetTagName(token).ToUpper();
                TagType tagType = GetTagType(token);

                if (tagType == TagType.SelfClosed || tagType == TagType.Opening || tagType == TagType.Text)
                {
                    DOMElement elem = elements.AddElement(tagType == TagType.Text);

                    if (tagType == TagType.Text)
                    {
                        elem.NodeValue = token;
                    }
                    else
                    {
                        elem.TagName = tagName;
                        elem.ElementId = GetElementID(token);
                        elem.ClassName = GetClassName(token);
                    }

                    if (tagType == TagType.Opening)
                    {
                        i = AppendChild(tokens, i + 1, elem);
                    }
                }
                else if (tagType == TagType.Closing)
                {
                    continue;
                }
            }
        }

        private int AppendChild(List<string> tokens, int idx, DOMElement parent)
        {
            if (parent.IsTextNode) return idx;

            for (int i = idx; i < tokens.Count; i++)
            {
                string token = tokens[i];
                if (token.Trim() == "") continue;

                string tagName = GetTagName(token).ToUpper();
                TagType tagType = GetTagType(token);

                if (tagType == TagType.SelfClosed || tagType == TagType.Opening || tagType == TagType.Text)
                {
                    DOMElement elem = parent.Children.AddElement(tagType == TagType.Text);
                    elem.ParentNode = parent;

                    if (tagType == TagType.Text)
                    {
                        elem.NodeValue = token;
                    }
                    else
                    {
                        elem.TagName = tagName;
                        elem.ElementId = GetElementID(token);
                        elem.ClassName = GetClassName(token);
                    }

                    if (tagType == TagType.Opening)
                    {
                        i = AppendChild(tokens, i + 1, elem);
                    }
                }
                else if (tagType == TagType.Closing)
                {
                    return i;
                }
            }

            return tokens.Count;
        }
    }
}
