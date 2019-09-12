using System;
using System.Threading.Tasks;
using Windows.System;

namespace SumoBotController
{
    public class KeyManager
    {
        public delegate Task KeyEvent(object sender, VirtualKey key);
        public event KeyEvent KeyPressed;
        public event KeyEvent KeyReleased;

        VirtualKey _currentKey = VirtualKey.None;

        public async Task OnKeyPressed(object sender, VirtualKey key)
        {
            _currentKey = key;
            await KeyPressed?.Invoke(sender, key);
        }

        public async Task OnKeyReleased(object sender, VirtualKey key)
        {
            if (key != _currentKey)
            {
                return;
            }

            _currentKey = VirtualKey.None;
            await KeyReleased?.Invoke(sender, key);
        }
    }
}
