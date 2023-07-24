using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Arke.SipEngine.FSM
{

    public class InjectableEnumValue
    {
        public bool IncrementOrdinal { get; set; } = true;
        public string Value { get; set; }
    }

    public abstract class ExtensibleEnum
    {
        public static IList<string> PreferredInterfaces = new List<string>() { "IStateMachineState", "IStateMachineTrigger" };
        private static readonly IDictionary<string, IDictionary<string, int>> Ordinals = new Dictionary<string, IDictionary<string, int>>();

        protected ExtensibleEnum(string value)
        {
            if (!Ordinals.ContainsKey(TypeName))
            {
                Ordinals.Add(TypeName, new Dictionary<string, int>());
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!Ordinals[TypeName].ContainsKey(value))
                {
                    var idx = Ordinals[TypeName].Keys.Count;
                    Ordinals[TypeName].Add(value, idx);
                    Value = $"{TypeName}.{value}";
                    Ordinal = idx;
                }
                else
                {
                    Value = $"{TypeName}.{value}";
                    Ordinal = Ordinals[TypeName][value];
                }

            }
        }
        //    {
        //        _ordinals[TypeName]++;
        //    }
        //}

        private string TypeName => GetType().GetInterfaces().Any(_ => PreferredInterfaces.Contains(_.Name))
            ? GetType().GetInterfaces().First(_ => PreferredInterfaces.Contains(_.Name)).Name
            : GetType().Name;
        protected string Value { get; }

        protected int Ordinal { get; }
        public static implicit operator int(ExtensibleEnum @enum) => @enum.Ordinal;
        public static implicit operator string(ExtensibleEnum @enum) => @enum.Value;
        public override string ToString() => Value;
        public override bool Equals(object obj) => Equals((ExtensibleEnum)obj);
        public bool Equals(ExtensibleEnum otherEnum) => otherEnum != null && otherEnum.Ordinal == Ordinal && otherEnum.Value == Value;
        public override int GetHashCode() => HashCode.Combine(Value, Ordinal);
    }
}
