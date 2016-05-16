using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFSurfacePlot3D
{
    public class SurfacePlotVisual3D : ModelVisual3D
    {
        /// <summary>
        /// The constructor for a new SurfacePlotVisual3D object.
        /// </summary>
        public SurfacePlotVisual3D()
        {
            IntervalX = 1;
            IntervalY = 1;
            IntervalZ = 0.25;
        }

        /// <summary>
        /// Gets or sets the points defining the 3D surface plot, as a 2D-array of Point3D objects.
        /// </summary>
        public Point3D[,] DataPoints
        {
            get { return (Point3D[,])GetValue(DataPointsProperty); }
            set { SetValue(DataPointsProperty, value); }
        }

        public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register("DataPoints", typeof(Point3D[,]), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(null, ModelWasChanged));

        /// <summary>
        /// Gets or sets the brush used for the surface.
        /// </summary>
        public Brush SurfaceBrush
        {
            get { return (Brush)GetValue(SurfaceBrushProperty); }
            set { SetValue(SurfaceBrushProperty, value); }
        }

        public static readonly DependencyProperty SurfaceBrushProperty = DependencyProperty.Register("SurfaceBrush", typeof(Brush), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(null, ModelWasChanged));

        // todo: make Dependency properties
        public double IntervalX { get; set; }
        public double IntervalY { get; set; }
        public double IntervalZ { get; set; }

        /// <summary>
        /// This is called whenever a property of the SurfacePlotVisual3D is changed; it updates the 3D model.
        /// </summary>
        private static void ModelWasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfacePlotVisual3D)d).UpdateModel();
        }

        /// <summary>
        /// This function updates the 3D visual model. It is called whenever a DependencyProperty of the SurfacePlotVisual3D object is called.
        /// </summary>
        private void UpdateModel()
        {
            this.Children.Clear(); // Necessary to remove BillboardTextVisual3D objects (?)
            this.Content = CreateModel();
        }

        /// <summary>
        /// This function contains all the "business logic" for constructing a SurfacePlot 3D. 
        /// </summary>
        /// <returns>A Model3DGroup containing all the component models (mesh, surface definition, grid objects, etc).</returns>
        private Model3DGroup CreateModel()
        {
            var newModelGroup = new Model3DGroup();
            double lineThickness = 0.01;
            double axesOffset = 0.05;

            // Get relevant constaints from the DataPoints object
            int numberOfRows = DataPoints.GetUpperBound(0) + 1;
            int numberOfColumns = DataPoints.GetUpperBound(1) + 1;

            // Determine the x, y, and z ranges of the DataPoints collection
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

            double minColorValue = double.MaxValue;
            double maxColorValue = double.MinValue;

            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfColumns; j++)
                {
                    double x = DataPoints[i, j].X;
                    double y = DataPoints[i, j].Y;
                    double z = DataPoints[i, j].Z;
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                    maxZ = Math.Max(maxZ, z);
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    minZ = Math.Min(minZ, z);
                }
            }

            int numberOfXAxisTicks = 5;
            int numberOfYAxisTicks = 5;
            int numberOfZAxisTicks = 5;
            double xAxisInterval = (maxX - minX) / numberOfXAxisTicks;
            double yAxisInterval = (maxY - minY) / numberOfYAxisTicks;
            double zAxisInterval = Math.Ceiling(maxZ) / numberOfZAxisTicks;

            // Set color value to 0 at texture coordinate 0.5, with an even spread in either direction
            if (Math.Abs(minColorValue) < Math.Abs(maxColorValue))
            {
                minColorValue = -maxColorValue;
            }
            else
            {
                maxColorValue = -minColorValue;
            }

            // Set the texture coordinates by either z-value or ColorValue
            var textureCoordinates = new Point[numberOfRows, numberOfColumns];
            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfColumns; j++)
                {
                    var tc = (DataPoints[i, j].Z - minZ) / (maxZ - minZ) * 80;
                    textureCoordinates[i, j] = new Point(tc, tc);
                }
            }

            // Build the surface model (i.e. the coloured surface model)
            MeshBuilder surfaceModelBuilder = new MeshBuilder();
            surfaceModelBuilder.AddRectangularMesh(DataPoints, textureCoordinates);

            GeometryModel3D surfaceModel = new GeometryModel3D(surfaceModelBuilder.ToMesh(), MaterialHelper.CreateMaterial(SurfaceBrush, null, null, 1, 0));
            surfaceModel.BackMaterial = surfaceModel.Material;

            // Instantiate MeshBuilder objects for the Grid meshes
            MeshBuilder gridBuilder = new MeshBuilder();

            // Build the axes labels model (i.e. the object that holds the axes labels and ticks)
            ModelVisual3D axesLabelsModel = new ModelVisual3D();

            // Loop through x intervals - for the surface meshlines, the grid, and X axes ticks
            for (double x = minX; x <= maxX + 0.0001; x += xAxisInterval)
            {
                // Axes labels
                BillboardTextVisual3D label = new BillboardTextVisual3D();
                label.Text = string.Format("{0:F1}", x);
                //label.Position = new Point3D(x, minY - axesOffset, minZ - axesOffset);
                label.Position = new Point3D(x, maxY - axesOffset, minZ - axesOffset);
                axesLabelsModel.Children.Add(label);

                // Grid lines
                //var gridPath = new List<Point3D>();
                //gridPath.Add(new Point3D(x, minY, minZ));
                //gridPath.Add(new Point3D(x, maxY, minZ));
                //gridPath.Add(new Point3D(x, maxY, maxZ));
                //gridBuilder.AddTube(gridPath, lineThickness, 9, false);
            }

            // Loop through y intervals - for the surface meshlines, the grid, and Y axes ticks
            //for (double y = minY; y <= maxY + 0.0001; y += YAxisInterval)
            for (double y = minY + yAxisInterval; y < maxY; y += yAxisInterval)
            {
                // Axes labels
                BillboardTextVisual3D label = new BillboardTextVisual3D();
                label.Text = string.Format("{0:F1}", y);
                //label.Position = new Point3D(minX - axesOffset, y, minZ - axesOffset);
                label.Position = new Point3D(maxY - axesOffset, y, minZ - axesOffset);
                axesLabelsModel.Children.Add(label);

                // Grid lines
                //var gridPath = new List<Point3D>();
                //gridPath.Add(new Point3D(minX, y, minZ));
                //gridPath.Add(new Point3D(maxX, y, minZ));
                //gridPath.Add(new Point3D(maxX, y, maxZ));
                //gridBuilder.AddTube(gridPath, lineThickness, 9, false);
            }

            //添加底层方框
            var gridPath = new List<Point3D>();
            gridPath.Add(new Point3D(minX, minY, minZ));
            gridPath.Add(new Point3D(maxX, minY, minZ));
            gridPath.Add(new Point3D(maxX, maxY, minZ));
            gridPath.Add(new Point3D(minX, maxY, minZ));
            gridBuilder.AddTube(gridPath, lineThickness, 9, true);

            // Loop through z intervals - for the grid, and Z axes ticks
            //for (double z = minZ; z <= maxZ + 0.0001; z += ZAxisInterval)
            for (double z = 0; z <= maxZ + 0.0001; z += zAxisInterval)
            {
                // Grid lines
                var path = new List<Point3D>();
                path.Add(new Point3D(maxX, minY, z));
                path.Add(new Point3D(minX, minY, z));
                path.Add(new Point3D(minX, maxY, z));
                gridBuilder.AddTube(path, lineThickness, 9, false);

                // Axes labels
                BillboardTextVisual3D label = new BillboardTextVisual3D();
                label.Text = string.Format("{0:F1}", z);
                label.Position = new Point3D(maxX - axesOffset, minY + axesOffset, z);
                axesLabelsModel.Children.Add(label);
            }

            // Add axes labels
            //BillboardTextVisual3D xLabel = new BillboardTextVisual3D();
            //xLabel.Text = "X Axis";
            //xLabel.Position = new Point3D((maxX - minX) / 2, minY - 3 * axesOffset, minZ - 5 * axesOffset);
            //axesLabelsModel.Children.Add(xLabel);
            //BillboardTextVisual3D yLabel = new BillboardTextVisual3D();
            //yLabel.Text = "Y Axis";
            //yLabel.Position = new Point3D(minX - 3 * axesOffset, (maxY - minY) / 2, minZ - 5 * axesOffset);
            //axesLabelsModel.Children.Add(yLabel);
            //BillboardTextVisual3D zLabel = new BillboardTextVisual3D();
            //zLabel.Text = "0";
            //zLabel.Position = new Point3D(maxX - axesOffset, minY + axesOffset, 0);
            //axesLabelsModel.Children.Add(zLabel);

            // Create models from MeshBuilders
            GeometryModel3D gridModel = new GeometryModel3D(gridBuilder.ToMesh(), Materials.Black);

            // Update model group
            this.Children.Add(axesLabelsModel);
            newModelGroup.Children.Add(surfaceModel);
            newModelGroup.Children.Add(gridModel);

            return newModelGroup;
        }

        private static Color GetFakeColor(double k)
        {
            if (k < 0)
            {
                k = 0;
            }
            if (k > 1)
            {
                k = 1;
            }

            double r, g, b;
            if (k < 0.25)
            {
                r = 0;
                g = 4 * k;
                b = 1;
            }
            else if (k < 0.5)
            {
                r = 0;
                g = 1;
                b = 1 - 4 * (k - 0.25);
            }
            else if (k < 0.75)
            {
                r = 4 * (k - 0.5);
                g = 1;
                b = 0;
            }
            else
            {
                r = 1;
                g = 1 - 4 * (k - 0.75);
                b = 0;
            }

            byte R = (byte)(r * 255);
            byte G = (byte)(g * 255);
            byte B = (byte)(b * 255);

            return Color.FromRgb(R, G, B);
        }
    }
}
