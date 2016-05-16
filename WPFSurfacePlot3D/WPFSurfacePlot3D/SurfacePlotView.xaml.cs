using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace WPFSurfacePlot3D
{
    /// <summary>
    /// Interaction logic for SurfacePlotView.xaml
    /// </summary>
    public partial class SurfacePlotView : UserControl
    {
        public SurfacePlotView()
        {
            InitializeComponent();

            hViewport.ZoomExtentsGesture = new KeyGesture(Key.Space);
        }
    }
}
