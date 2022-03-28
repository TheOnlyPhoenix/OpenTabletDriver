using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using StreamJsonRpc.Protocol;

namespace OpenTabletDriver.UX
{
    public static class Extensions
    {
        public static T CreateInstance<T>(this IServiceProvider serviceProvider, params object[] additionalDependencies)
        {
            var constructorQuery = from ctor in typeof(T).GetConstructors()
                let parameters = ctor.GetParameters()
                orderby parameters.Length
                select ctor;

            var constructor = constructorQuery.First();

            var argsQuery = from parameter in constructor.GetParameters()
                where parameter.IsIn
                let type = parameter.ParameterType
                select additionalDependencies.FirstOrDefault(d => d.GetType().IsAssignableTo(type)) ?? serviceProvider.GetService(type);

            return (T) constructor.Invoke(argsQuery.ToArray());
        }

        public static void AsyncInvoke(this Application application, Task task)
        {
            application.AsyncInvoke(task.Start);
        }

        public static void AsyncInvoke(this Application application, Func<Task> task)
        {
            application.AsyncInvoke(task());
        }

        public static DirectBinding<TValue> BindPluginSetting<TValue>(this PluginSettings settings, string settingName)
        {
            return new DelegateBinding<TValue>(
                () => settings[settingName].GetValue<TValue>(),
                v => settings[settingName].SetValue(v)
            );
        }

        public static DirectBinding<TValue?> BindSetting<TValue>(this DirectBinding<PluginSettings?> binding, string settingName)
        {
            return new DelegateBinding<TValue?>(
                () => binding.DataValue![settingName].GetValue<TValue>(),
                v => binding.DataValue![settingName].SetValue(v),
                handler => binding.DataValueChanged += handler,
                handler => binding.DataValueChanged -= handler
            );
        }

        public static DirectBinding<TValue> Setting<TValue>(this DirectBinding<PluginSetting> binding)
        {
            return binding.Convert(c => c.GetValue<TValue>,
                v =>
                {
                    binding.DataValue.SetValue(v);
                    return binding.DataValue;
                }
            );
        }

        public static void ShowMessageBox(this Exception exception)
        {
            Log.Exception(exception);
            MessageBox.Show(
                exception.Message + Environment.NewLine + exception.StackTrace,
                $"Error: {exception.GetType().Name}",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        public static void ShowMessageBox(this CommonErrorData errorData)
        {
            string message = errorData.Message + Environment.NewLine + errorData.StackTrace;
            Log.Write(
                errorData.TypeName!,
                message,
                LogLevel.Error
            );
            MessageBox.Show(
                message,
                errorData.TypeName,
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        public static BindableBinding<TControl, bool> GetEnabledBinding<TControl>(this TControl control) where TControl : Control
        {
            return new BindableBinding<TControl, bool>(
                control,
                (c) => c.Enabled,
                (c, v) => c.Enabled = v,
                (c, e) => c.EnabledChanged += e,
                (c, e) => c.EnabledChanged -= e
            );
        }

        public static void Deconstruct<T1, T2>(this ValueTuple<T1, T2> tuple, out T1 item1, out T2 item2)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
        }

        public static void Deconstruct<T1, T2, T3>(this ValueTuple<T1, T2, T3> tuple, out T1 item1, out T2 item2, out T3 item3)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
        }
    }
}
