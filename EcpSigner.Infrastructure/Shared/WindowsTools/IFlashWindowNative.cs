namespace WindowsTools
{
    public interface IFlashWindowNative
    {
        bool Flash(ref FlashWindow.FLASHWINFO fInfo);
    }
}
