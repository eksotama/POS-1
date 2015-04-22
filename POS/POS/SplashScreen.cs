﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.ClearScript;
using Microsoft.ClearScript.Windows;
using POS.Internals;
using POS.Internals.ScriptEngine;
using POS.Internals.ScriptEngine.ModuleSystem;
using POS.Internals.ScriptEngine.ModuleSystem;
using POS.Models;
using POS.Properties;
using Pos.Internals.Extensions;
using Telerik.WinControls;
using POS.Internals.Database;

namespace POS
{
    public partial class SplashScreen : Form
    {
        //Use timer class
        private Timer tmr;

        public SplashScreen()
        {
            this.InitializeComponent();
        }

        private void SplashScreen_Shown(object sender, EventArgs e)
        {
            this.tmr = new Timer();
            //set time interval 3 sec
            this.tmr.Interval = 1000;
            //starts the timer
            this.tmr.Start();
            this.tmr.Tick += this.tmr_Tick;
        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            this.tmr.Stop();

            var p = Application.StartupPath;

            ServiceLocator.LanguageCatalog.Load(p + "\\locale\\de_DE.mo");

            PluginLoader.AddObject("include", new Action<string>(fn => { PluginLoader.Eval(File.ReadAllText(fn)); }));
            PluginLoader.AddObject("eval", new Func<string, object>(fn => { return PluginLoader.Eval(fn); }));
            PluginLoader.AddObject("import", new Action<string>(fn => { ModuleLoader.Load(PluginLoader.GetEngine(), Assembly.LoadFile(fn)); }));

            if (!Directory.Exists(string.Format("{0}\\data", p)))
            {
                Directory.CreateDirectory(string.Format("{0}\\data", p));
            }
            if (!Directory.Exists(string.Format("{0}\\data\\invoices", p)))
            {
                Directory.CreateDirectory(string.Format("{0}\\data\\invoices", p));
            }
            if (!Directory.Exists(string.Format("{0}\\themes", p)))
            {
                Directory.CreateDirectory(string.Format("{0}\\themes", p));
            }

            foreach (var f in Directory.GetFiles(string.Format("{0}\\themes", p), "*.tssp", SearchOption.AllDirectories))
            {
                ThemeResolutionService.LoadPackageFile(f);
            }

            DbContext.Open(p + "\\data\\data");

            ServiceLocator.ProductCategories = DbContext.GetItems<ProductCategory>().ToList<ProductCategory>();
            ServiceLocator.Products = DbContext.GetItems<Product>().ToList<Product>();
            ServiceLocator.Invoices = DbContext.GetItems<Invoice>().ToList<Invoice>();

            ServiceLocator.Products.Add(new Product { Category = 0, ID = "Rose", Price = 0.81, Tax = 0.19, Image = Resources.box.ToBytes(ImageFormat.Png) });

            //var r = DbContext.GetItems<Product>();

            var frm = new MainForm();

            var ps = PluginLoader.Load(Application.StartupPath + "\\Plugins");
            var fs = PluginLoader.Call("init");

            frm.Show();

            //hide this form
            this.Hide();
        }
    }
}