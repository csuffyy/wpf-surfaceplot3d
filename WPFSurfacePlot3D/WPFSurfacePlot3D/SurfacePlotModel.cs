using HelixToolkit.Wpf;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFSurfacePlot3D
{
    public enum ColorCoding
    {
        /// <summary>
        /// No color coding, use coloured lights
        /// </summary>
        ByLights,

        /// <summary>
        /// Color code by gradient in y-direction using a gradient brush with white ambient light
        /// </summary>
        ByGradientY
    }

    class SurfacePlotModel : INotifyPropertyChanged
    {
        private int defaultFunctionSampleSize = 100;

        // So the overall goal of this section is to output the appropriate values to SurfacePlotVisual3D - namely,
        // - DataPoints as Point3D, plus xAxisTicks (and y, z) as double[]
        // - plus all the appropriate properties, which can be directly edited/bindable by the user

        public SurfacePlotModel()
        {
            Title = "New Surface Plot";

            ColorCoding = ColorCoding.ByLights;

            // Initialize the DataPoints collection
            Func<double, double, double> sampleFunction = (x, y) => 10 * Math.Sin(Math.Sqrt(x * x + y * y)) / Math.Sqrt(x * x + y * y);
            PlotFunction(sampleFunction, -10, 10);
        }

        #region === Public Methods ===

        #region PlotFunction

        public void PlotData(double[,] zData2DArray)
        {
            int n = zData2DArray.GetLength(0);
            int m = zData2DArray.GetLength(1);
            Point3D[,] newDataArray = new Point3D[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Point3D point = new Point3D(i, j, zData2DArray[i, j]);
                    newDataArray[i, j] = point;
                }
            }
            dataPoints = newDataArray;
            RaisePropertyChanged("DataPoints");
        }

        public void PlotData(double[,] zData2DArray, double xMinimum, double xMaximum, double yMinimum, double yMaximum)
        {

        }

        public void PlotData(double[,] zData2DArray, double[] xArray, double[] yArray)
        {
            // Note - check that dimensions match!!
        }

        public void PlotData(Point3D[,] point3DArray)
        {
            // Directly plot from a Point3D array
        }

        public void PlotFunction(Func<double, double, double> function)
        {
            PlotFunction(function, -1, 1, -1, 1, defaultFunctionSampleSize, defaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double minimumXY, double maximumXY)
        {
            PlotFunction(function, minimumXY, maximumXY, minimumXY, maximumXY, defaultFunctionSampleSize,
                defaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double minimumXY, double maximumXY,
            int sampleSize)
        {
            PlotFunction(function, minimumXY, maximumXY, minimumXY, maximumXY, sampleSize, sampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double xMinimum, double xMaximum,
            double yMinimum, double yMaximum)
        {
            PlotFunction(function, xMinimum, xMaximum, yMinimum, yMaximum, defaultFunctionSampleSize,
                defaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double xMinimum, double xMaximum,
            double yMinimum, double yMaximum, int sampleSize)
        {
            PlotFunction(function, xMinimum, xMaximum, yMinimum, yMaximum, sampleSize, sampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double xMinimum, double xMaximum,
            double yMinimum, double yMaximum, int xSampleSize, int ySampleSize)
        {
            double[] xArray = CreateLinearlySpacedArray(xMinimum, xMaximum, xSampleSize);
            double[] yArray = CreateLinearlySpacedArray(yMinimum, yMaximum, ySampleSize);

            DataPoints = CreateDataArrayFromFunction(function, xArray, yArray);

            RaisePropertyChanged("DataPoints");
            RaisePropertyChanged("ColorValues");
            RaisePropertyChanged("SurfaceBrush");
        }

        #endregion

        #endregion

        #region === Private Methods ===

        private static Point3D[,] CreateDataArrayFromFunction(Func<double, double, double> f, double[] xArray, double[] yArray)
        {
            Point3D[,] newDataArray = new Point3D[xArray.Length, yArray.Length];
            for (int i = 0; i < xArray.Length; i++)
            {
                double x = xArray[i];
                for (int j = 0; j < yArray.Length; j++)
                {
                    double y = yArray[j];
                    newDataArray[i, j] = new Point3D(x, y, f(x, y));
                }
            }
            return newDataArray;
        }

        private static double[] CreateLinearlySpacedArray(double minValue, double maxValue, int numberOfPoints)
        {
            double[] array = new double[numberOfPoints];
            double intervalSize = (maxValue - minValue) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; i++)
            {
                array[i] = minValue + i * intervalSize;
            }
            return array;
        }

        #endregion

        #region === Exposed Properties ===

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string property = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        private Point3D[,] dataPoints;
        public Point3D[,] DataPoints
        {
            get { return dataPoints; }
            set
            {
                dataPoints = value;
                //RaisePropertyChanged("DataPoints");
            }
        }

        private double[] xAxisTicks;
        public double[] XAxisTicks
        {
            get { return xAxisTicks; }
            set
            {
                xAxisTicks = value;
                //RaisePropertyChanged("DataPoints");
            }
        }

        private double[] yAxisTicks;
        public double[] YAxisTicks
        {
            get { return yAxisTicks; }
            set
            {
                yAxisTicks = value;
                //RaisePropertyChanged("DataPoints");
            }
        }

        private double[] zAxisTicks;
        public double[] ZAxisTicks
        {
            get { return zAxisTicks; }
            set
            {
                zAxisTicks = value;
                //RaisePropertyChanged("DataPoints");
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged("Title");
            }
        }

        private bool showSurfaceMesh;
        public bool ShowSurfaceMesh
        {
            get { return showSurfaceMesh; }
            set
            {
                showSurfaceMesh = value;
                RaisePropertyChanged("ShowSurfaceMesh");
            }
        }

        private bool showContourLines;
        public bool ShowContourLines
        {
            get { return showContourLines; }
            set
            {
                showContourLines = value;
                RaisePropertyChanged("ShowContourLines");
            }
        }

        private bool showMiniCoordinates;
        public bool ShowMiniCoordinates
        {
            get { return showMiniCoordinates; }
            set
            {
                showMiniCoordinates = value;
                RaisePropertyChanged("ShowMiniCoordinates");
            }
        }

        #endregion

        public ColorCoding ColorCoding { get; set; }

        public Model3DGroup Lights
        {
            get
            {
                var group = new Model3DGroup();
                switch (ColorCoding)
                {
                    case ColorCoding.ByGradientY:
                        group.Children.Add(new AmbientLight(Colors.White));
                        break;
                    case ColorCoding.ByLights:
                        group.Children.Add(new AmbientLight(Colors.Gray));
                        group.Children.Add(new PointLight(Colors.Red, new Point3D(0, -1000, 0)));
                        group.Children.Add(new PointLight(Colors.Blue, new Point3D(0, 0, 1000)));
                        group.Children.Add(new PointLight(Colors.Green, new Point3D(1000, 1000, 0)));
                        break;
                }
                return group;
            }
        }

        public Brush SurfaceBrush
        {
            get
            {
                switch (ColorCoding)
                {
                    case ColorCoding.ByGradientY:
                        return BrushHelper.CreateGradientBrush(Colors.Red, Colors.White, Colors.Blue);
                    case ColorCoding.ByLights:
                        return Brushes.White;
                }
                return null;
            }
        }
    }
}
