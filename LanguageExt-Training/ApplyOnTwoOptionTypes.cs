using System;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;

namespace LanguageExt_Training
{
    /**
     * Drawbacks:
     * We have to know about Apply and TryOption
     * We have to tranform Options to TryOption
     * We have to pack our function so we could invoke it with apply. Yes?
     * "TryOption<Guid> paymentId = apply(startPayment, appCode, apiKey)" looks neet
     * paymentId is lazy (really) so we don't have to invoke until we really have to
     * out of the box pattern match for Some/None/Fail
     */
    public class ApplyOnTwoOptionTypes
    {
        /**
         * Based on https://github.com/louthy/language-ext/blob/057ee5eb123148eeac37938c2b00c48571c51c7d/LanguageExt.Tests/TryOptionApply.cs
         */

        Option<string> GetAppCode() => Some("App Code");
        Option<string> GetApiKey() => Some("API Key");
        Guid StartPayment(string appCode, string apiKey) => PaymentId;
        Guid PaymentId = Guid.Parse("ab8084ca-ae51-4f59-a766-63a02309d016");

        [Fact]
        public void ElTesto()
        {
            var appCode = GetAppCode().ToTryOption();
            var apiKey = GetApiKey().ToTryOption();
            TryOption<Func<string, string, Guid>> startPayment = TryOption(() => fun((string ac, string ak) => StartPayment(ac, ak)));

            TryOption<Guid> paymentId = startPayment.Apply(appCode, apiKey);

            /* Note that paymentId is a function (!) and has not been yet invoked.
             * We can still call paymentId()
             * Welcome to FP world where everything is function.
             */

            var message = paymentId.Match( // TryOption<>.Match does some pattern matching and invokes appropriate function
                Some: id => $"Hey bro, there is your {id}",
                None: () => "Bro, something was missing, we couldn't even try to start your payment!",
                Fail: ex => $"Bro, we screwed up. Here is what went wrong: {ex}");

            message.Should().BeOfType<string>().And.Contain(PaymentId.ToString());
        }

        [Fact]
        public void ElTesto_static_applies()
        {
            TryOption<string> appCode = GetAppCode().ToTryOption();
            TryOption<string> apiKey = GetApiKey().ToTryOption();
            Func<string, string, Guid> startPayment = fun((string code, string key) => StartPayment(code, key));

            /* We seperated startPayment declaration and packing it into TryOption wrapper
             * This way our code is more readable
             */

            TryOption<Guid> paymentId = apply(startPayment, appCode, apiKey);

            var message = paymentId.Match(
                Some: id => $"Hey bro, there is your {id}",
                None: () => "Bro, something was missing, we couldn't even try to start your payment!",
                Fail: ex => $"Bro, we screwed up. Here is what went wrong: {ex}");

            message.Should().BeOfType<string>().And.Contain(PaymentId.ToString());
        }

        [Fact]
        public void ElTesto_no_func_decl()
        {
            TryOption<string> appCode = GetAppCode().ToTryOption();
            TryOption<string> apiKey = GetApiKey().ToTryOption();

            /* Here we deleted declaration of function. It appears it is not really needed :) */

            TryOption<Guid> paymentId = apply(StartPayment, appCode, apiKey);

            var message = paymentId.Match(
                Some: id => $"Hey bro, there is your {id}",
                None: () => "Bro, something was missing, we couldn't even try to start your payment!",
                Fail: ex => $"Bro, we screwed up. Here is what went wrong: {ex}");

            message.Should().BeOfType<string>().And.Contain(PaymentId.ToString());
        }


        [Fact]
        public void ElTesto_compact()
        {
            /* This is just a compact version of all above */

            TryOption<string> appCode = GetAppCode().ToTryOption();
            TryOption<string> apiKey = GetApiKey().ToTryOption();

            var message = apply(StartPayment, appCode, apiKey)
                            .Match(Some: id => $"Hey bro, there is your {id}",
                                   None: () => "Bro, something was missing, we couldn't even try to start your payment!",
                                   Fail: ex => $"Bro, we screwed up. Here is what went wrong: {ex}");

            message.Should().BeOfType<string>().And.Contain(PaymentId.ToString());
        }
    }
}