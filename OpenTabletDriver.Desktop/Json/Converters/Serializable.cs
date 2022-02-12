using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Json.Converters
{
    [JsonObject]
    internal abstract class Serializable
    {
        protected static NotSupportedException NotSupported([CallerMemberName] string memberName = null)
        {
            return new NotSupportedException($"Method {memberName} cannot be invoked on a serialized proxy.");
        }

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
