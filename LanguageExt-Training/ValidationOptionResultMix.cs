using FluentValidation.Results;
using LanguageExt;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

using static LanguageExt.Prelude;
namespace LanguageExt_Training
{
    class SomeOperationRequest { }
    class SomeOperationResult
    {
        public int PaymentId { get; } = 42;
    }


    public class ValidationOptionResultMix
    {
        /**
         * Based on:
         * https://github.com/louthy/language-ext/issues/313
         */

        Validation<ValidationFailure, SomeOperationRequest> Validate(SomeOperationRequest request) => Validation<ValidationFailure, SomeOperationRequest>.Success(request);
        Validation<ValidationFailure, string> Validate(string apiKey) => Validation<ValidationFailure, string>.Success(apiKey);
        Option<string> GetApiKey() => Some("Some api key");
        Option<string> GetAppCode(string apiKey) => Some("Some app code");
        Result<SomeOperationResult> StartPayment(SomeOperationRequest request, string appCode) => new Result<SomeOperationResult>(new SomeOperationResult());
        Task<Result<SomeOperationResult>> StartPaymentAsync(SomeOperationRequest request, string appCode) => Task.FromResult(new Result<SomeOperationResult>(new SomeOperationResult() { }));

        [Fact]
        public void ElTesto()
        {
            var request = new SomeOperationRequest();

            var validateApiKey = GetApiKey().ToValidation(new ValidationFailure("ApiKey", "API Key is missing in headers"));

            /* This will create validation on Option<string>. ApiKey will be valid only when present. */

            Validation<ValidationFailure, string> v = from v1 in validateApiKey
                                                      from v2 in Validate(request)
                                                      select v1 + v2;

            /* Alternativly, we can do this way */
            //var v_alt = (apiKey, Validate(request)).Apply((_, req) => req);

            /* C# evaluates functions one-by-one. If one failes, the rest is omitted as we haven't got all required variables (v1 and v2) to complete the last statement */

            /* All of the above is the great use example of monad being aplicative structure */

            Option<string> appCode = from key in v.ToOption()
                                     from code in GetAppCode(key)
                                     select code;

            /* Here we go again. Monad being applicative.
             * Note we have to transform Validation to Option.
             * We might miss something so.
             * Maybe we should consider side-effects?
             */

            Option<Result<SomeOperationResult>> mPaymentResult = (from code in appCode.ToTryOption()
                                                                  from req in TryOption(request)
                                                                  select StartPayment(req, code)).ToOption();

            string result = match(mPaymentResult,
                Some: mResult => mResult.Match(
                   Succ: res => $"Success! Payment ID = {res.PaymentId}",
                   Fail: ex => ex.Message),
                None: () => "Invalid requqest.");

            /* I guess we could do some more work here to make it more elegant, but I have enough.
             * It kind of makes sense. We can get result only in if the validation passes and we have all required parameters.
             * So we have to match first if we got any result and then if the result is Ok or Exception.
             * We could still do it */
        }

        [Fact]
        public async Task ElTestoAsync()
        {
            var request = new SomeOperationRequest();

            var validateApiKey = GetApiKey().ToValidation(new ValidationFailure("ApiKey", "API Key is missing in headers"));

            var v = from v1 in validateApiKey
                    from v2 in Validate(request)
                    select v1 + v2;

            var appCode = from key in v.ToOption()
                          from code in GetAppCode(key)
                          select code;

            TryAsync<Result<SomeOperationResult>> bloo = from req in TryAsync(request)
                                                         from code in appCode.ToTryAsync()
                                                         select StartPaymentAsync(req, code);
            
            string message = await bloo.Match(
                Succ: tryRes => tryRes.Match(
                    Succ: payRes => $"Payment succeeded! PaymentID = {payRes.PaymentId}",
                    Fail: ex => ex.Message),
                Fail: ex => ex.Message);

            message.Should().Contain("42");
        }

        [Fact]
        public void NoneConvertedToValidationReturnFailure()
        {
            Option<string>.None
                .ToValidation(new ValidationFailure("ApiKey", "API Key is missing in headers"))
                .IfSuccess(_ => throw new Exception("Should never be here"));
        }

        [Fact]
        public void PassedValidationConvertedToTryOptionWillReturnString() // !!!!
        {
            Validation<ValidationFailure, string>.Success("value")
                .ToTryOption()
                .Match(Some: x => x,
                       Fail: () => throw new Exception("Should never be here"))
                .Should()
                .BeOfType<string>();
        }
    }
}
