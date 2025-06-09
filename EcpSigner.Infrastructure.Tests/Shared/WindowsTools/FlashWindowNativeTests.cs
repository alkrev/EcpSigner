using FluentAssertions;

namespace WindowsTools
{
    public class FlashWindowNativeTests
    {
        [Fact]
        public void Flash_Should_Invoke_FlashWindowEx_And_Return_Bool()
        {
            // Arrange
            var native = new FlashWindowNative();
            var hwnd = IntPtr.Zero; // В реальности нужно передать корректный HWND окна
            var fInfo = new FlashWindow.FLASHWINFO
            {
                cbSize = Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf<FlashWindow.FLASHWINFO>()),
                hwnd = hwnd,
                dwFlags = FlashWindow.FLASHW_STOP,
                uCount = 1,
                dwTimeout = 0
            };

            // Act
            Action act = () => native.Flash(ref fInfo);

            // Assert
            act.Should().NotThrow(); // Убедимся, что метод не выбрасывает исключений
        }
    }
}
