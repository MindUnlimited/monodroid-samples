using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Todo;
using Android.Util;
using Android.Graphics;

namespace Cheesesquare
{
    public static class PublicFields
    {
        public static Database Database;
        public static async void UpdateDatabase()
        {
            await PublicFields.Database.SyncAsync();
        }

        public static Tree<Item> ItemTree { get; set; }
        //public static Dictionary<string, TreeNode<Item>> ItemDictionary { get; set; }
        public static List<Item> ItemList { get; set; }
        public static List<TreeNode<Item>> domains { get; set; }
        public static List<Group> userGroups { get; set; }

        public static async void attachChildren(TreeNode<Item> node, IEnumerable<Item> children = null)
        {
            PublicFields.UpdateDatabase();

            var sharedChildren = await PublicFields.Database.GetItemLinks();
            var sharedChildrenLinks = sharedChildren.Where(it => it.Parent != null && it.Parent == node.Value.id);

            // retrieve the item from the ItemLink
            List<Item> sharedChildrenItems = new List<Item>();
            foreach (var l in sharedChildrenLinks)
            {
                var it = await PublicFields.Database.GetItem(l.ItemID);

                if (it == null)
                {
                    Log.Error("main", string.Format("could not retrieve item with item ID: {0}!", l.ItemID));
                }
                else
                {
                    it.Parent = node.Value.id;
                    it.SharedLink = l;

                    sharedChildrenItems.Add(it);
                }
            }

            if (children == null)
            {
                children = PublicFields.ItemList.Where(it => it.Parent != null && it.Parent == node.Value.id && it.OwnedBy == PublicFields.Database.defGroup.id).Concat(sharedChildrenItems);
            }

            foreach (var child in children)
            {
                var childNode = node.Children.Add(child);
                var childNodeLinks = sharedChildren.Where(it => it.Parent != null && it.Parent == child.id);

                // retrieve the item from the ItemLink
                List<Item> childNodeSharedChildrenItems = new List<Item>();
                foreach (var l in childNodeLinks)
                {
                    var it = await PublicFields.Database.GetItem(l.ItemID);
                    it.Parent = childNode.Value.id;
                    it.SharedLink = l;

                    childNodeSharedChildrenItems.Add(it);
                }


                var childrenOfChild = PublicFields.ItemList.Where(it => it.Parent != null && it.Parent == child.id && it.OwnedBy == PublicFields.Database.defGroup.id).Concat(childNodeSharedChildrenItems);

                if (childrenOfChild != null && childrenOfChild.Count() > 0)
                {
                    attachChildren(childNode, childrenOfChild);
                }
            }
        }

        public static async void MakeTree()
        {
            if (PublicFields.Database.userGroups == null)
                PublicFields.Database.userGroups = await PublicFields.Database.getGroups();

            PublicFields.ItemTree = new Tree<Item>(new Item { Name = "ROOT" });
            PublicFields.ItemList = (List<Todo.Item>)await PublicFields.Database.GetItems();

            PublicFields.domains = new List<TreeNode<Item>>();
            foreach (var domain in PublicFields.ItemList.Where(it => it.Type == 1).ToList())
            {
                var domainNode = PublicFields.ItemTree.Children.Add(domain);
                attachChildren(domainNode);
                PublicFields.domains.Add(domainNode);
            }
        }

        public static int CalculateInSampleSize(
        BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) > reqHeight
                        && (halfWidth / inSampleSize) > reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        public static Bitmap DecodeSampledBitmapFromFile(string pathName,
        int reqWidth, int reqHeight)
        {

            // First decode with inJustDecodeBounds=true to check dimensions
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeFile(pathName, options);

            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            return BitmapFactory.DecodeFile(pathName, options);
        }
    }
}