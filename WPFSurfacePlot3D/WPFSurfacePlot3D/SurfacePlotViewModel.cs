using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFSurfacePlot3D
{
    /// <summary>
    /// 曲面图ViewModel
    /// </summary>
    public class SurfacePlotViewModel : INotifyPropertyChanged
    {
        private const int DefaultFunctionSampleSize = 100;

        public SurfacePlotViewModel()
        {
            Title = "New Surface Plot";
        }

        #region 公共方法

        #region 绘图

        public void PlotData(double[,] zData2DArray)
        {
            int x = zData2DArray.GetLength(0);
            int y = zData2DArray.GetLength(1);
            var newDataArray = new Point3D[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    var point = new Point3D(i, j, zData2DArray[i, j]);
                    newDataArray[i, j] = point;
                }
            }

            DataPoints = newDataArray;
        }

        public void PlotData(double[,] zData2DArray, double xMinimum, double xMaximum, double yMinimum, double yMaximum)
        {
            //TODO:
        }

        public void PlotData(double[,] zData2DArray, double[] xArray, double[] yArray)
        {
            // Note - check that dimensions match!!
            //TODO:
        }

        public void PlotData(Point3D[,] point3DArray)
        {
            DataPoints = point3DArray;
        }

        public void PlotFunction(Func<double, double, double> function)
        {
            PlotFunction(function, -1, 1, -1, 1, DefaultFunctionSampleSize, DefaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double minimumXY, double maximumXY)
        {
            PlotFunction(function, minimumXY, maximumXY, minimumXY, maximumXY, DefaultFunctionSampleSize,
                DefaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double minimumXY, double maximumXY,
            int sampleSize)
        {
            PlotFunction(function, minimumXY, maximumXY, minimumXY, maximumXY, sampleSize, sampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double xMinimum, double xMaximum,
            double yMinimum, double yMaximum)
        {
            PlotFunction(function, xMinimum, xMaximum, yMinimum, yMaximum, DefaultFunctionSampleSize,
                DefaultFunctionSampleSize);
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
            XAxisTicks = xArray;

            double[] yArray = CreateLinearlySpacedArray(yMinimum, yMaximum, ySampleSize);
            YAxisTicks = yArray;

            DataPoints = CreateDataArrayFromFunction(function, xArray, yArray);

            RaisePropertyChanged("SurfaceBrush");
        }

        #endregion

        #endregion

        #region 私有方法

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

        /// <summary>
        /// 获取色阶表
        /// { 255, 255, 255 }  ->
        /// { 255, 255, 0 }    ->
        /// { 255, 0, 0 }      ->
        /// { 255, 0, 255 }    ->
        /// { 0, 0, 255 }      ->
        /// { 0, 0, 0 }
        /// </summary>
        private static List<Color> GetColorList()
        {
            const byte g = 255;
            const byte b = 255;
            const byte r = 255;

            var colors = new List<Color>();

            for (int i = b; i >= 0; i = i - 5)
            {
                colors.Add(Color.FromRgb(255, 255, (byte)i));
            }

            for (int i = g - 1; i >= 0; i = i - 5)
            {
                colors.Add(Color.FromRgb(255, (byte)i, 0));
            }

            for (int i = 1; i < b; i = i + 5)
            {
                colors.Add(Color.FromRgb(255, 0, (byte)i));
            }

            for (int i = r - 1; i >= 0; i = i - 5)
            {
                colors.Add(Color.FromRgb((byte)i, 0, 255));
            }

            for (int i = b - 1; i >= 0; i = i - 5)
            {
                colors.Add(Color.FromRgb(0, 0, (byte)i));
            }

            return colors;
        }

        #endregion

        #region 属性

        private Point3D[,] dataPoints;
        public Point3D[,] DataPoints
        {
            get { return dataPoints; }
            set
            {
                dataPoints = value;
                RaisePropertyChanged();
            }
        }

        private double[] xAxisTicks;
        public double[] XAxisTicks
        {
            get { return xAxisTicks; }
            set
            {
                xAxisTicks = value;
                RaisePropertyChanged("DataPoints");
            }
        }

        private double[] yAxisTicks;
        public double[] YAxisTicks
        {
            get { return yAxisTicks; }
            set
            {
                yAxisTicks = value;
                RaisePropertyChanged("DataPoints");
            }
        }

        private double[] zAxisTicks;
        public double[] ZAxisTicks
        {
            get { return zAxisTicks; }
            set
            {
                zAxisTicks = value;
                RaisePropertyChanged("DataPoints");
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged();
            }
        }

        private bool showMiniCoordinates;
        public bool ShowMiniCoordinates
        {
            get { return showMiniCoordinates; }
            set
            {
                showMiniCoordinates = value;
                RaisePropertyChanged();
            }
        }

        public Model3DGroup Lights
        {
            get
            {
                var group = new Model3DGroup();
                group.Children.Add(new AmbientLight(Colors.White));
                return group;
            }
        }

        public Brush SurfaceBrush
        {
            get { return BrushHelper.CreateGradientBrush(GetColorList()); }
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string property = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
