using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

[assembly: Dependency(typeof(Thesis.Droid.AssetService))]

namespace Thesis.Droid
{
    public class AssetService:IAssetService
    {
        public string LoadFile(string fileName)
        {
            if (fileName == null)
            {
                return null;
            }

            // Read the contents of our asset
            string content;
            AssetManager assets = Android.App.Application.Context.Assets;
            using (StreamReader sr = new StreamReader(assets.Open(fileName)))
            {
                content = sr.ReadToEnd();
            }
            return content;
        }

    }
}