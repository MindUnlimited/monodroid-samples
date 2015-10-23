using Android.Support.V7.Widget;

namespace Cheesesquare
{
    public abstract class BaseRecyclerAdapter : RecyclerView.Adapter
    {
        public abstract string GetTextToShowInBubble(int pos);
    }
}