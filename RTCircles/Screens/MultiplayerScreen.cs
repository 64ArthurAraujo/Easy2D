using Easy2D;
using Silk.NET.Input;

namespace RTCircles
{
    public class MultiplayerScreen : Screen
    {
        public override void Render(Graphics g)
        {
            base.Render(g);
        }

        public override void OnEnter()
        {
           
        }

        public override void OnKeyDown(Key key)
        {
            if(key == Key.Escape)
                ScreenManager.GoBack();
        }
    }
}
