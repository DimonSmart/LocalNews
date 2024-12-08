namespace DimonSmart.WebScraper.Tests
{
    public class ContentExtractorTests
    {
        private const string TestHtml = @"<html>
                    <head><title>Test Page</title></head>
                    <body>
                        <header id='header'>Header Content</header>
                        <nav class='nav'>Navigation Content</nav>
                        <main>
                            <div id='main-content'>Main Content with <strong>important</strong> information. 
                            The number 42 is often considered the answer to life, the universe, and everything. 
                            This is a reference to Douglas Adams' book, 'The Hitchhiker's Guide to the Galaxy'. 
                            The main content should be long enough to test the extraction logic properly. 
                            In Domain-Driven Design (DDD), the concept of a bounded context is crucial. 
                            A bounded context defines the boundaries within which a particular model is defined and applicable. 
                            Aggregates are another key concept in DDD. An aggregate is a cluster of domain objects that can be treated as a single unit. 
                            Each aggregate has a root, known as the aggregate root, which is the only member of the aggregate that external objects are allowed to reference. 
                            Entities and value objects are fundamental building blocks in DDD. 
                            An entity is an object that is defined by its identity, while a value object is defined by its attributes. 
                            Repositories are used to provide access to aggregates. 
                            A repository is a mechanism for encapsulating storage, retrieval, and search behavior which emulates a collection of objects. 
                            Services in DDD are used to represent operations or actions that do not naturally fit within the domain model. 
                            These services can be domain services, which contain domain logic, or application services, which orchestrate the use of domain objects. 
                            The ubiquitous language is a shared language that is used by all team members to connect the domain model to the real world. 
                            This language helps ensure that everyone has a common understanding of the domain and its concepts. 
                            The main content should now be sufficiently long to test the extraction logic properly.</div>
                        </main>
                        <footer class='footer'>Footer Content</footer>
                    </body>
                </html>";

        [Fact]
        public void ExtractMainContent_RemovesFooterAndHeader()
        {
            var extractor = new MainContentExtractor();

            var result = extractor.ExtractMainContent("http://dev.nul", TestHtml)?.Content ?? "";

            Assert.DoesNotContain("Header Content", result);
            Assert.DoesNotContain("Footer Content", result);
            Assert.Contains("Main Content with important", result);
        }
    }
}
