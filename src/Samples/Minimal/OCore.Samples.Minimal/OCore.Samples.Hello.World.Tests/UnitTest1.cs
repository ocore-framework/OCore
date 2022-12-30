using Microsoft.Extensions.Options;
using OCore.Diagnostics.Options;
using System.Threading.Tasks;
using Xunit;

namespace OCore.Samples.Hello.World.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var capitalizationService = new NameCapitalizationService();

            var capitalized = await capitalizationService.Capitalize("Hello");

            Assert.Equal("HELLO", capitalized);            
        }

        [Fact]
        public async Task Test2()
        {
            var capitalizationService = new NameCapitalizationService();
            var helloWorldService = new HelloWorldService(capitalizationService);

            var shoutResponse = await helloWorldService.ShoutHelloTo("Orleans");

            Assert.Equal("Hello, ORLEANS!", shoutResponse);
        }

        [Fact]
        public async Task Test3()
        {
            var options = Options.Create(new DiagnosticsOptions());
            var correlationIdRecorder = new OCore.Diagnostics.Entities.CorrelationIdCallRecorder(options);
            await correlationIdRecorder.Create(new Diagnostics.Entities.CorrelationIdCallRecord { });
            ;
            return;
        }
    }
}