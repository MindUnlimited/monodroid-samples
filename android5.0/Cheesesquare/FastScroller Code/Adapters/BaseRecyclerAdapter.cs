using Android.Support.V7.Widget;

namespace MindSet
{
    public abstract class BaseRecyclerAdapter : RecyclerView.Adapter
    {
        public abstract string GetTextToShowInBubble(int pos);
    }
}