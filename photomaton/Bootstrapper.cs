using Microsoft.Practices.Unity;
using photomaton.Views;
using Prism.Unity;
using System.Windows;

namespace photomaton
{
    internal class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }
    }
}