using System;
using System.IO;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;


namespace 抗浮计算书
{
    internal static class Program
    {



        public static object? cengshu { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new zhchuangkou());

        }






    }
}