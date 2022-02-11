using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace OpenTabletDriver.Attributes
{
    public class MemberValidatedAttribute : Attribute
    {
        public MemberValidatedAttribute(string memberName, bool requiresInstance = false)
        {
            MemberName = memberName;
            RequiresInstance = requiresInstance;
        }

        public string MemberName { get; }
        public bool RequiresInstance { get; }

        public T? GetValue<T>(IServiceProvider serviceProvider, PropertyInfo property)
        {
            var sourceType = property.ReflectedType!;
            var member = sourceType.GetMember(MemberName).First();
            var instance = RequiresInstance ? serviceProvider.GetRequiredService(sourceType) : null;

            try
            {
                var obj = member.MemberType switch
                {
                    MemberTypes.Property => sourceType.GetProperty(MemberName)!.GetValue(instance),
                    MemberTypes.Field => sourceType.GetField(MemberName)!.GetValue(instance),
                    MemberTypes.Method => sourceType.GetMethod(MemberName)!.Invoke(instance, new [] { serviceProvider }),
                    _ => default
                };
                return (T?)obj;
            }
            catch (Exception e)
            {
                Log.Write("Plugin", $"Failed to get valid binding values for '{MemberName}'", LogLevel.Error);

                var match = Regex.Match(e.Message, "Non-static (.*) requires a target\\.");

                if (e is TargetException && match.Success)
                {
                    Log.Debug("Plugin", $"Validation {match.Groups[1].Value} must be static");
                }
                else
                {
                    Log.Exception(e);
                }
            }

            return default;
        }
    }
}
