using System.Xml;
using AngleSharp.Xml.Parser;
using AngleSharp.Html.Parser;
using NUnit.Framework;
using System.Threading.Tasks;
using AngleSharp.Dom;
using System.Xml.XPath;
using System.Xml.Linq;

namespace AngleSharp.XPath.Tests
{
    [TestFixture]
    public class HtmlDocumentNavigatorTests
    {
        [Test, Retry(5)]
        public async Task SelectSingleNodeTest()
        {
            // Arrange
            const string address = "https://stackoverflow.com/questions/39471800/is-anglesharps-htmlparser-threadsafe";
            var config = Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(address);

            // Act
            var content = document.DocumentElement.SelectSingleNode("//div[@id='content']");

            // Assert
            Assert.That(content, Is.Not.Null);
        }

        [Test]
        public void SelectNodes_SelectList_ShouldReturnList()
        {
            // Arrange
            const string html =
            @"<ol>
				<li>First</li>
				<li>Second</li>
				<li>Third</li>
			</ol>";
            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);

            // Act
            var nodes = document.DocumentElement.SelectNodes("//li");

            // Assert
            Assert.That(nodes, Has.Count.EqualTo(3));
        }

        [Test]
        public void SelectPrecedingNodeInDocumentWithDoctype_ShouldReturnNode()
        {
            // Arrange
            const string html =
            @"<!DOCTYPE html>
			<body>
				<span></span>
				<div></div>
			</body>";
            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);

            // Act
            var node = document.DocumentElement.SelectSingleNode("//div/preceding::span");

            // Assert
            Assert.That(node, Is.Not.Null);
        }

        [Test]
        public void SelectSingleNode_IgnoreNamespaces_ShouldReturnNode()
        {
            // Arrange
            var xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\"><url><loc>https://www.test.com/de/accounts/profile</loc><xhtml:link rel=\"alternate\" hreflang=\"fr\" href=\"https://www.test.com/fr/accounts/profile\" /><xhtml:link rel=\"alternate\" hreflang=\"en\" href=\"https://www.test.com/en/accounts/profile\" /><xhtml:link rel=\"alternate\" hreflang=\"it\" href=\"https://www.test.com/it/accounts/profile\" /><changefreq>weekly</changefreq><priority>0.4</priority></url></urlset>";
            var parser = new XmlParser();
            var doc = parser.ParseDocument(xml);

            // Act
            var node = doc.DocumentElement.SelectSingleNode("/urlset/url/link");

            // Assert
            Assert.IsNotNull(node);
            Assert.That(node.NodeName, Is.EqualTo("xhtml:link"));
        }

        [Test]
        public void SelectSingleNode_DontIgnoreNamespaces_ShouldReturnNode()
        {
            // Arrange
            var xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\"><url><loc>https://www.test.com/de/accounts/profile</loc><xhtml:link rel=\"alternate\" hreflang=\"fr\" href=\"https://www.test.com/fr/accounts/profile\" /><xhtml:link rel=\"alternate\" hreflang=\"en\" href=\"https://www.test.com/en/accounts/profile\" /><xhtml:link rel=\"alternate\" hreflang=\"it\" href=\"https://www.test.com/it/accounts/profile\" /><changefreq>weekly</changefreq><priority>0.4</priority></url></urlset>";
            var parser = new XmlParser();
            var doc = parser.ParseDocument(xml);
            var namespaceManager = new XmlNamespaceManager(new NameTable());

            namespaceManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
            namespaceManager.AddNamespace("d", "http://www.sitemaps.org/schemas/sitemap/0.9");

            // Act
            var node = doc.DocumentElement.SelectSingleNode("/d:urlset/d:url/xhtml:link", namespaceManager, false);

            // Assert
            Assert.IsNotNull(node);
            Assert.That(node.NodeName, Is.EqualTo("xhtml:link"));
        }

        [Test]
        public void SelectNodes_CanReturnAttribute()
        {
            // Arrange
            var html = "<!DOCTYPE html><html><body><div class=\"one\"><span class=\"two\">hello world</span></div></body></html>";
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(html);

            // Act
            var nodes = doc.DocumentElement.SelectNodes("//@*");

            // Assert
            Assert.IsNotNull(nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));
            Assert.That(nodes, Is.All.InstanceOf<Dom.IAttr>());
        }

        [Test]
        public void TestNameXPathFunctionOnXMLDoc()
        {
            // Arrange
            var xml = @"<html><head><title>Test</title></head><body><h1>Test</h1></body></html>";
            var angleSharpXmlDoc = new XmlParser().ParseDocument(xml);

            // Act
            var xmlNav = angleSharpXmlDoc.CreateNavigator();

            // Assert
            Assert.AreEqual(TagNames.Html, xmlNav.Evaluate("name()"));
        }

        [Test]
        public void TestNameXPathFunctionOnHTMLDoc()
        {
            // Arrange
            var html = @"<html><head><title>Test</title></head><body><h1>Test</h1></body></html>";

            var angleSharpHtmlDoc = new HtmlParser().ParseDocument(html);

            // Act
            var htmlNav = angleSharpHtmlDoc.CreateNavigator();

            // Assert
            Assert.AreEqual(TagNames.Html, htmlNav.Evaluate("name()"));
        }

        [Test]
        public void TestAttributeNavigation()
        {
            var xml = @"<root att1='value 1' att2='value 2'><child>foo</child></root>";

            var parser = new XmlParser();

            var doc = parser.ParseDocument(xml);

            var nav = doc.CreateNavigator(false);

            Assert.AreEqual(nav.Name, "root");
            if (nav.MoveToFirstAttribute())
            {
                do
                {
                    Assert.AreEqual(nav.NodeType, XPathNodeType.Attribute);
                }
                while (nav.MoveToNextAttribute());
                nav.MoveToParent();
            }
            Assert.AreEqual(nav.Name, "root");

        }

        [Test]
        public void TestReadSubTree()
        {
            var xml = @"<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'/><title>Test</title></head><body><h1>Test</h1><p id='p-1'>This is paragraph 1 line 1.<br/>Line 2.</p><p>This is paragraph 2 line 1.<br/>Line 2.</p></body></html>";

            var xmlDoc = new XmlParser().ParseDocument(xml);

            var angleSharpNav = xmlDoc.CreateNavigator(false);

            var xDoc = XDocument.Parse(xml);

            var xNav = xDoc.Root.CreateNavigator();

            using (var angleSharpReader = angleSharpNav.ReadSubtree())
            {
                using (var xReader = xNav.ReadSubtree())
                {
                    while (angleSharpReader.Read() && xReader.Read())
                    {
                        Assert.AreEqual(angleSharpReader.NodeType, xReader.NodeType);
                        Assert.AreEqual(angleSharpReader.LocalName, xReader.LocalName);
                        Assert.AreEqual(angleSharpReader.NamespaceURI, xReader.NamespaceURI);
                        Assert.AreEqual(angleSharpReader.HasAttributes, xReader.HasAttributes);

                        if (angleSharpReader.NodeType == XmlNodeType.Element && angleSharpReader.HasAttributes && xReader.HasAttributes)
                        {
                            while (angleSharpReader.MoveToNextAttribute() && xReader.MoveToNextAttribute())
                            {
                                Assert.AreEqual(angleSharpReader.NodeType, xReader.NodeType);
                                Assert.AreEqual(angleSharpReader.LocalName, xReader.LocalName);
                                Assert.AreEqual(angleSharpReader.NamespaceURI, xReader.NamespaceURI);
                            }

                            angleSharpReader.MoveToElement();
                            xReader.MoveToElement();
                        }
                    }
                }
            }
        }
    }
}
