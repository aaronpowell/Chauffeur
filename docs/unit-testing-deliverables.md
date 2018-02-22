---
id: unit-testing-deliverables
title: Unit Testing Deliverables
---

Previously we've seen how easy it is to make a simple [Awesome Deliverable](creating-deliverables.md). Now let's do what I think it's really important writing some unit tests.

For this I'm going to use [NUnit](https://www.nuget.org/packages/nunit) as the test runner:

```c#
using NUnit.Framework;

namespace Chauffeur.Samples.Tests
{
    [TestFixture]
    public class AwesomeDeliverableTests
    {
    }
}
```

Now we'll create a test that asserts that `Hello World` did get written out so let's make a test:

```c#
using System.Threading.Tasks;
using NUnit.Framework;

namespace Chauffeur.Samples.Tests
{
    [TestFixture]
    public class AwesomeDeliverableTests
    {
        [Test]
        public async Task WhenRun_ReceivedMessage()
        {
            var deliverable = new AwesomeDeliverable(null, null);

            await deliverable.Run(null, null);
        }
    }
}
```

Well that'll fail cuz the `Out` property isn't a `TextWriter`, it'll be `null`, so let's create a mock version. You could do this with a mocking library but I'll do it by hand

```c#
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Chauffeur.Samples.Tests
{
    [TestFixture]
    public class AwesomeDeliverableTests
    {
        [Test]
        public async Task WhenRun_ReceivedMessage()
        {
            var deliverable = new AwesomeDeliverable(null, null);

            await deliverable.Run(null, null);
        }
    }

    class MockTextWriter : TextWriter
    {
        private readonly List<string> messages;
        public MockTextWriter()
        {
            messages = new List<string>();
        }

        public IEnumerable<string> Messages { get { return messages; } }

        public override System.Text.Encoding Encoding { get { return System.Text.Encoding.Default; } }

        public override async Task WriteLineAsync(string value)
        {
            messages.Add(value);
            await Task.FromResult(value);
        }
    }
}
```

And now we can use it:

```c#
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Chauffeur.Samples.Tests
{
    [TestFixture]
    public class AwesomeDeliverableTests
    {
        [Test]
        public async Task WhenRun_ReceivedMessage()
        {
            var writer = new MockTextWriter();
            var deliverable = new AwesomeDeliverable(null, writer);

            await deliverable.Run(null, null);

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }
    }

    class MockTextWriter : TextWriter
    {
        private readonly List<string> messages;
        public MockTextWriter()
        {
            messages = new List<string>();
        }

        public IEnumerable<string> Messages { get { return messages; } }

        public override System.Text.Encoding Encoding { get { return System.Text.Encoding.Default; } }

        public override async Task WriteLineAsync(string value)
        {
            messages.Add(value);
            await Task.FromResult(value);
        }
    }
}
```

Done! We have a test and it's passing.