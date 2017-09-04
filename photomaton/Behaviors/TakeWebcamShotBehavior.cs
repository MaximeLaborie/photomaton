using photomaton.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace photomaton.Behaviors
{
    public class TakeWebcamShotBehavior
    {
        public static readonly DependencyProperty IsGettingImageProperty =
            DependencyProperty.RegisterAttached("IsGettingImage", typeof(bool), typeof(TakeWebcamShotBehavior),
                new PropertyMetadata(false, OnIsGettingImagePropertyChanged));

        public static void OnIsGettingImagePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs evt)
        {
            MainWindow win = source as MainWindow;
            if ((bool)evt.OldValue == false && (bool)evt.NewValue == true)
                win.SetCameraShotToViewModel();
        }

        public static void SetIsGettingImage(DependencyObject target, bool value)
        {
            target.SetValue(IsGettingImageProperty, value);
        }

        public static bool GetTest(DependencyObject target)
        {
            return (bool)target.GetValue(IsGettingImageProperty);
        }
    }
}
