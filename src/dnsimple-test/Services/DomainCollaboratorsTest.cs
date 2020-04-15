using System.Linq;
using dnsimple.Services;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace dnsimple_test.Services
{
    [TestFixture]
    public class DomainCollaboratorsTest
    {
        private JToken _jToken;

        private const string ListCollaboratorsFixture =
            "listCollaborators/success.http";

        private const string AddCollaboratorFixture =
            "addCollaborator/success.http";

        private const string AddCollaboratorInviteFixture =
            "addCollaborator/invite-success.http";

        private const string RemoveCollaboratorFixture =
            "removeCollaborator/success.http";

        private const long AccountId = 1010;

        [SetUp]
        public void Initialize()
        {
            var loader = new FixtureLoader("v2", ListCollaboratorsFixture);
            _jToken = JObject.Parse(loader.ExtractJsonPayload());
        }

        [Test]
        public void CollaboratorsData()
        {
            var collaborators = new CollaboratorsData(_jToken);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(100, collaborators.Collaborators.First().Id);
                Assert.AreEqual(1,
                    collaborators.Collaborators.First().DomainId);
                Assert.AreEqual("example.com",
                    collaborators.Collaborators.First().DomainName);
                Assert.AreEqual(999,
                    collaborators.Collaborators.First().UserId);
                Assert.AreEqual("existing-user@example.com",
                    collaborators.Collaborators.First().UserEmail);
                Assert.IsFalse(collaborators.Collaborators.First().Invitation);
            });
        }

        [Test]
        public void CollaboratorsResponse()
        {
            var response = new CollaboratorsResponse(_jToken);

            Assert.AreEqual(2, response.Data.Collaborators.Count);
        }

        [Test]
        [TestCase(1010, "100")]
        [TestCase(1010, "example.com")]
        public void ListCollaborators(long accountId, string domainIdentifier)
        {
            var client = new MockDnsimpleClient(ListCollaboratorsFixture);
            var collaborators =
                client.Domains.ListCollaborators(accountId, domainIdentifier);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(101,
                    collaborators.Data.Collaborators.Last().Id);
            });
        }

        [Test]
        [TestCase("1")]
        [TestCase("example.com")]
        public void AddCollaborator(string domainIdentifier)
        {
            var client = new MockDnsimpleClient(AddCollaboratorFixture);
            var collaborator =
                client.Domains.AddCollaborator(AccountId, domainIdentifier,
                    "existing-user@example.com").Data;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(999, collaborator.UserId);
                Assert.AreEqual("existing-user@example.com",
                    collaborator.UserEmail);
                Assert.IsFalse(collaborator.Invitation);
            });
        }

        [Test]
        [TestCase("1")]
        [TestCase("example.com")]
        public void InviteCollaborator(string domainIdentifier)
        {
            var client = new MockDnsimpleClient(AddCollaboratorInviteFixture);
            var collaborator = client.Domains.AddCollaborator(AccountId,
                domainIdentifier, "invited-user@example.com");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("invited-user@example.com",
                    collaborator.Data.UserEmail);
                Assert.IsNull(collaborator.Data.UserId);
                Assert.IsTrue(collaborator.Data.Invitation);
            });
        }

        [Test]
        public void RemoveCollaborator()
        {
            var client = new MockDnsimpleClient(RemoveCollaboratorFixture);

            Assert.DoesNotThrow(delegate
            {
                client.Domains.RemoveCollaborator(AccountId, "example.com",
                    100);
            });
        }
    }
}