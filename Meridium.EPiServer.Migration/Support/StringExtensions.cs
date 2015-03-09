using System;
using System.Linq;
using System.Text.RegularExpressions;
using EPiServer.Core;
using HtmlAgilityPack;

namespace Meridium.EPiServer.Migration.Support {
    public static class StringExtensions {
        public static XhtmlString CleanupForIntroduction(this object obj) {
            if (obj is string) {
                // Sometimes the intro is a string, wrap string in xhtml to benfit from 
                // all the goodness in CleanUpForMainBody
                var xhtml = new XhtmlString((string)obj);
                return xhtml.CleanupForMainBody();
            }

            return obj.CleanupForMainBody();
        }

        public static XhtmlString CleanupForMainBody(this object obj) {
            var xhtml = obj as XhtmlString;
            if (xhtml == null || string.IsNullOrEmpty(xhtml.ToString())) return xhtml;
            return new XhtmlString(CleanupForMainBody((string) xhtml.ToString()));
        }

        public static string CleanupForMainBody(this string mainbody) {
            if (string.IsNullOrEmpty(mainbody)) return mainbody;
            var doc = new HtmlDocument();
            doc.OptionWriteEmptyNodes = true; //otherwise <p></p> will be converted to <p>
            doc.LoadHtml(mainbody);

            //replace some tags
            var headers = doc.DocumentNode.SelectNodes("//h1 | //b | //i");
            if (headers != null) {
                foreach (HtmlNode item in headers) {
                    if (item.Name.Equals("h1", StringComparison.InvariantCultureIgnoreCase)) item.Name = "h2";
                    if (item.Name.Equals("b", StringComparison.InvariantCultureIgnoreCase)) item.Name = "strong";
                    if (item.Name.Equals("i", StringComparison.InvariantCultureIgnoreCase)) item.Name = "em";
                    item.Attributes.RemoveAll();
                }
            }

            //remove DC
            var dcs = doc.DocumentNode.SelectNodes("//span[@state]");
            if (dcs != null) {
                foreach (var dc in dcs) {
                    dc.Remove();
                }
            }
            //clear attributes from p element
            var attributes = doc.DocumentNode.SelectNodes("//p[@*]");
            if (attributes != null) {
                foreach (HtmlNode element in attributes) {
                    element.Attributes.RemoveAll();
                }
            }
            //remove all comments
            doc.DocumentNode.Descendants()
                .Where(n => n.NodeType == HtmlNodeType.Comment)
                .ToList()
                .ForEach(n => n.Remove());

            //remove illegal tags
            var illegalTags = doc.DocumentNode.SelectNodes("//span | //div | //font");
            if (illegalTags != null) {
                foreach (var element in illegalTags) {
                    foreach (var child in element.ChildNodes) {
                        element.ParentNode.InsertBefore(child, element);
                    }
                    element.Remove();
                }
            }
            //remove multiple spaces and fix space followed by newline or newline followed by space
            return Regex.Replace(doc.DocumentNode.OuterHtml, @"[ ]{2,}", " ")
                .Replace(Environment.NewLine + " ", Environment.NewLine)
                .Replace(" " + Environment.NewLine, Environment.NewLine);
        }

        public static XhtmlString StripTags(this XhtmlString self, params string[] tags) {
            if (self == null) return null;

            var pattern = string.Format("</?(?:{0})( [^>]+)?>", string.Join("|", tags));
            return new XhtmlString(Regex.Replace(self.ToString(), pattern, "", RegexOptions.IgnoreCase));
        }
        
    }
}