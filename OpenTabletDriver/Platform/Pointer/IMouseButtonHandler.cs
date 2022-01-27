namespace OpenTabletDriver.Platform.Pointer
{
    public interface IMouseButtonHandler
    {
        void MouseDown(MouseButton button);
        void MouseUp(MouseButton button);
    }
}
