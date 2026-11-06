using System;
using System.Linq;
using System.Net;

namespace HIA.Core.HTML
{
    public class DOMElement
    {
        public DOMElement ParentNode { get; set; }

        public DOMCollection Children { get; }

        public string ElementId { get; set; }

        public string NodeValue { get; set; }

        public string TagName { get; set; }

        public string[] ClassName { get; set; }

        public bool IsTextNode => Children == null;

        public DOMElement(bool isText = false)
        {
            ParentNode = null;
            Children = isText ? null : new();
            ElementId = string.Empty;
            NodeValue = string.Empty;
            TagName = string.Empty;
            ClassName = Array.Empty<string>();
        }

        public bool MatchQuery(string query)
        {
            if (query.StartsWith("#") && ElementId == query) return true;
            else if (query.StartsWith(".") && ClassName.Contains(query)) return true;
            else if (TagName.Equals($"{query.ToUpper()}") || TagName.StartsWith($"{query.ToUpper()} ")) return true;
            return false;
        }

        public DOMCollection QuerySelector(string query)
        {
            if (IsTextNode) return null;

            DOMCollection cols = new();
            if (MatchQuery(query)) cols.Add(this);

            if (Children?.Count > 0)
            {
                var lst = Children?.QuerySelector(query);
                if (lst?.Count > 0)
                {
                    cols.AddRange(lst);
                }
            }
            return cols.Count > 0 ? cols : null;
        }

        public string InnerText()
        {
            if (IsTextNode) return NodeValue ?? "";
            if (Children == null || Children.Count < 1) return "";

            string s = NodeValue;
            foreach (var c in Children)
            {
                var cc = c.InnerText();
                if (!string.IsNullOrEmpty(cc)) s += $" {cc} ";
            }
            return WebUtility.HtmlDecode(s);
        }
    }
}