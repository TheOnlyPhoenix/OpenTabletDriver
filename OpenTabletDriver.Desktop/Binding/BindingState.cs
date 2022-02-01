using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingState
    {
        public IBinding Binding { set; get; }
        public bool State { protected set; get; }

        protected bool PreviousState { set; get; }

        public void Invoke(IDeviceReport report, bool newState)
        {
            if (Binding is IStateBinding stateBinding)
            {
                if (newState && !PreviousState)
                    stateBinding.Press(report);
                else if (!newState && PreviousState)
                    stateBinding.Release(report);
            }

            PreviousState = newState;
        }
    }
}
