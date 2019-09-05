using HttpDo;
using System;
using System.Linq;

namespace ExampleApp
{
    class Program
    {
        public static HttpService Handler { get; private set; }

        static void Main(string[] args)
        {
            Handler = new HttpService("http://localhost:3333", "HttpDo");

            Console.WriteLine("Click anything to close the program.");

            Console.ReadKey(true);
        }

        [HttpGet("print")]
        public static void Print() => Console.WriteLine("This is an example message.");

        [HttpGet("margs")]
        public static void MethodWithArgs(int? a, int b, int c = 21) => Console.WriteLine("A is nullable, so isn't required\n" +
            "B is required and will pop errors if it's not set.\n" +
            "C has a default value of 21, if you don't set it - it'll remain 21\n" +
            "\n" +
            $"A: {a} B: {b} C: {c}");

        [HttpGet("unauthorized", true)]
        public static string SecureMethod() => "You are authorized, so you can view this.";

        [HttpGet("authorize")]
        public static string Authorize()
        {
            Handler.GetSession()["is_authorized"] = true;

            return "Consider yourself authorized.";
        }

        [HttpGet("unauthorize")]
        public static string Unauthorize()
        {
            Handler.GetSession()["is_authorized"] = false;

            return "You are now unauthorized.";
        }

        [HttpGet("redirect")]
        public static Response Redirector()
        {
            return new Response("");
        }

        [HttpGet("redirectwsession")]
        public static Response RedirectorWithSession()
        {
            Handler.GetSession()["example_value"] = true;

            return new Response("");
        }

        [HttpPost("formdefinedparams")]
        public static string PostMethodWithArgs(string text, int integer) => $"The values were: `{text}` and `{integer}`";

        [HttpPost("form")]
        public static string PostMethodWithFormData(FormData form)
        {
            var output = $"Form contains: {form.Count} params and they are:";

            form.ToList().ForEach(x => output += "\n" + x.Key + "=" + x.Value);

            return output;
        }

        [HttpPost("formmixparams")]
        public static string PostMethodWithFormDataAndArgs(FormData form, int integer)
        {
            var output = $"Form contains: {form.Count} params and they are:";

            form.ToList().ForEach(x => output += "\n" + x.Key + "=" + x.Value);

            output += $"\nTheres also value under `integer` {integer}, but when you use FormData it becomes obsolete.";

            return output;
        }
    }
}
