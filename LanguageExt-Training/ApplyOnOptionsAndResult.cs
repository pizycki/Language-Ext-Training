using System;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;

namespace LanguageExt_Training
{
    public class ApplyOnOptionsAndResult
    {
        Option<string> GetAppCode() => Some("App Code");
        Option<string> GetApiKey(string appCode) => Some("API Key");
        Result<Guid> StartPayment(string appCode, string apiKey) => PaymentId; // Looks like Apply is awesome and can work both with Result<T> and T :)
        Guid PaymentId = Guid.Parse("ab8084ca-ae51-4f59-a766-63a02309d016");

        [Fact]
        public void ElTesto_OptionsFirst()
        {
            Option<string> appCode = GetAppCode();
            Option<string> apiKey = from code in appCode
                                    from key in GetApiKey(code)
                                    select key;

            string message =
                fun((string code, string key) => StartPayment(code, key))
                    .Apply(appCode.ToTryOption(), apiKey.ToTryOption())
                    .Match(Some: id => $"Hey bro, there is your {id}",
                           None: () => "Bro, something was missing, we couldn't even try to start your payment!",
                           Fail: ex => $"Bro, we screwed up. Here is what went wrong: {ex}");

            message.Should().BeOfType<string>().And.Contain(PaymentId.ToString());
        }
    }
}