namespace Analyze6502;

public abstract class BaseView
{
    public abstract void Draw(int x, int y);
    public abstract void ClearRenderArea(int x, int y);
}