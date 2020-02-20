﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Thesis.iOS.PathService))]
namespace Thesis.iOS
{
    internal class PathService: IPathService
    {
        public string InternalFolder
        {
            get
            {
                return null;
            }
        }

        public string PublicExternalFolder
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        public string PrivateExternalFolder
        {
            get
            {
                return null;
            }
        }
    }
}


