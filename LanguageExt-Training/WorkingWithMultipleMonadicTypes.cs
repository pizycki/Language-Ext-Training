using System;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;

namespace LanguageExt_Training
{
    /**
     * Drawbacks:
     * We don't know what went wrong (was it app code or api key missing)
     * We have only two level monadic type (which is good for us).
     */

    public class WorkingWithMultipleMonadicTypes
    {
        Guid PaymentId = Guid.Parse("ab8084ca-ae51-4f59-a766-63a02309d016");
        Result<Guid> StartPayment(string appCode, string apiKey) => new Result<Guid>(PaymentId);

        [Fact]
        public void ElTesto()
        {
            Option<string> appCode = Some("App Code");
            Option<string> apiKey = Some("API Key");

            Option<Result<Guid>> result = from ac in appCode
                                          from ak in apiKey
                                          select StartPayment(ac, ak);

            var paymentId = result.Match(
                None: () => "Bad request.",
                Some: res => res.Match(
                    Succ: id => id.ToString(),
                    Fail: ex => ex.Message));

            paymentId.Should()
                .BeOfType<string>()
                .And
                .BeEquivalentTo(PaymentId.ToString());
        }
    }
}
