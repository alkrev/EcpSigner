using Xunit;
using Moq;
using FluentAssertions;
using System;
using WindowsTools;

namespace WindowsTools
{
    public class FlashWindowTests
    {
        private readonly IntPtr _dummyHwnd = new IntPtr(123456);

        [Fact]
        public void Start_Should_CallFlashWindowEx_WithCorrectParameters()
        {
            // Arrange
            var nativeMock = new Mock<IFlashWindowNative>();

            var flashWindow = new FlashWindow(_dummyHwnd, nativeMock.Object);

            FlashWindow.FLASHWINFO capturedInfo = default;

            nativeMock
                .Setup(n => n.Flash(ref It.Ref<FlashWindow.FLASHWINFO>.IsAny))
                .Returns(true)
                .Callback((ref FlashWindow.FLASHWINFO info) => { capturedInfo = info; });

            // Act
            flashWindow.Start();

            // Assert
            capturedInfo.hwnd.Should().Be(_dummyHwnd);
            capturedInfo.dwFlags.Should().Be(FlashWindow.FLASHW_ALL | FlashWindow.FLASHW_TIMERNOFG);
            capturedInfo.uCount.Should().Be(UInt32.MaxValue);
            capturedInfo.dwTimeout.Should().Be(0);
            capturedInfo.cbSize.Should().Be(Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf<FlashWindow.FLASHWINFO>()));

            nativeMock.Verify(n => n.Flash(ref It.Ref<FlashWindow.FLASHWINFO>.IsAny), Times.Once);
        }

        [Fact]
        public void Stop_Should_CallFlashWindowEx_WithCorrectParameters()
        {
            // Arrange
            var nativeMock = new Mock<IFlashWindowNative>();

            var flashWindow = new FlashWindow(_dummyHwnd, nativeMock.Object);

            FlashWindow.FLASHWINFO capturedInfo = default;

            nativeMock
                .Setup(n => n.Flash(ref It.Ref<FlashWindow.FLASHWINFO>.IsAny))
                .Returns(true)
                .Callback((ref FlashWindow.FLASHWINFO info) => { capturedInfo = info; });

            // Act
            flashWindow.Stop();

            // Assert
            capturedInfo.hwnd.Should().Be(_dummyHwnd);
            capturedInfo.dwFlags.Should().Be(FlashWindow.FLASHW_STOP);
            capturedInfo.uCount.Should().Be(UInt32.MaxValue);
            capturedInfo.dwTimeout.Should().Be(0);
            capturedInfo.cbSize.Should().Be(Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf<FlashWindow.FLASHWINFO>()));

            nativeMock.Verify(n => n.Flash(ref It.Ref<FlashWindow.FLASHWINFO>.IsAny), Times.Once);
        }
    }
}
