using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    internal class DynamicLimit
    {
        public bool Subtract { get; set; }
        public string MainValue { get; set; }
        public string Delay { get; set; }
        public static IEnumerable<DynamicLimit> Parse(string x)
        {
            if (String.IsNullOrEmpty(x)) return null;
            Queue<string> tokens = new Queue<string>();
            bool tokenOn = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < x.Length; i++)
            {
                if (tokenOn)
                {
                    if (char.IsWhiteSpace(x, i))
                    {
                        tokenOn = false;
                        tokens.Enqueue(sb.ToString());
                    }
                    else
                    {
                        char curr = x[i];
                        if (curr == '+' || curr == '-')
                        {
                            tokenOn = false;
                            tokens.Enqueue(sb.ToString());
                            tokens.Enqueue(curr.ToString());
                        }
                        else sb.Append(curr);
                    }

                }
                else
                {
                    if (char.IsWhiteSpace(x, i)) continue;
                    else
                    {
                        char curr = x[i];
                        if (curr == '+' || curr == '-')
                        {
                            tokens.Enqueue(curr.ToString());
                        }
                        else
                        {
                            sb.Clear();
                            sb.Append(curr);
                            tokenOn = true;
                        }
                    }
                }
            }
            if (tokenOn) tokens.Enqueue(sb.ToString());
            var result = new List<DynamicLimit>();

            while (tokens.Count > 0)
            {
                var curr = tokens.Dequeue();
                if (curr == "+" || curr == "-") continue;
                var limit = new DynamicLimit { MainValue = curr };
                result.Add(limit);
                if (tokens.Count > 0)
                {
                    curr = tokens.Peek();
                    if (curr == "+")
                    {

                        while (tokens.Count > 0 && (curr == "+" || curr == "-")) { curr = tokens.Dequeue(); curr = tokens.Peek(); }
                        curr = tokens.Dequeue();
                        if (curr != "+" && curr != "-")
                        {
                            limit.Delay = curr;
                            limit.Subtract = false;
                        }
                    }
                    else if (curr == "-")
                    {
                        while (tokens.Count > 0 && (curr == "+" || curr == "-")) { curr = tokens.Dequeue(); curr = tokens.Peek(); }
                        curr = tokens.Dequeue();
                        if (curr != "+" && curr != "-")
                        {
                            limit.Delay = curr;
                            limit.Subtract = true;
                        }
                    }

                }
            }
            return result;
        }
    }
}
