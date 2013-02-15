using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HtwKinect
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // override the Visual Studio variable DataDirectory. This will be used in App.config for DB connection string
            AppDomain.CurrentDomain.SetData("DataDirectory", GetProjectPath());
        }

        internal static string GetProjectPath()
        {
            // Gets debug or release folder
            string projectPath = Environment.CurrentDirectory;
            // go two times to the parent folder
            for (int i = 0; i < 2; i++)
                projectPath = System.IO.Path.GetDirectoryName(projectPath);
            return projectPath + @"\";
        }
    }
}
