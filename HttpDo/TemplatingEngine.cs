using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpDo
{
    /// <summary>
    /// Class responsible for replacing template tags and 
    /// contained code with the result of said code.
    /// </summary>
    public static class TemplatingEngine
    {
        private static Regex EchoPattern { get; set; } = new Regex(@"@{{(.*?)}}");

        /// <summary>
        /// Replace template tags and contained code with the result of said code.
        /// </summary>
        /// <param name="session">Session used as source of values.</param>
        /// <param name="data">Page data to parse.</param>
        /// <returns>Parsed page data.</returns>
        public static string Parse(Session session, string data)
        {
            foreach (Match match in EchoPattern.Matches(data))
            {
                var func = CSharpScript.Create(match.Groups[1].Value,
                    globalsType: typeof(TemplatingGlobals),
                    options: ScriptOptions.Default.WithReferences(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly)).CreateDelegate();

                var value = func(new TemplatingGlobals()
                {
                    Session = session
                }).Result;

                data = data.Remove(match.Index, match.Length);
                data = data.Insert(match.Index, value?.ToString() ?? "[NULL]");
            }

            return data;
        }
    }
}
