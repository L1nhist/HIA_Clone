using System.Collections.Generic;

namespace HIA.Core.HTML
{
    public class DOMCollection : List<DOMElement>
    {
        public DOMCollection() : base()
        { }

        public DOMElement AddElement(bool isText = false)
        {
            DOMElement elem = new(isText);
            Add(elem);
            return elem;
        }

        public DOMCollection QuerySelector(string query)
        {
            if (Count < 1) return null;

            DOMCollection cols = new();
            foreach (var e in this)
            {
                var lst = e.QuerySelector(query);
                if (lst != null && lst.Count > 0)
                {
                    cols.AddRange(lst);
                }
            }

            return cols.Count > 0 ? cols : null;
        }
    }
}