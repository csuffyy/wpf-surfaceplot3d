using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFSurfacePlot3D
{
    public class SurfacePlotVisual3D : ModelVisual3D
    {
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
            double lineThickness = 0.05;

            int rows = DataPoints.GetLength(0);
            int columns = DataPoints.GetLength(1);

            // Determine the x, y, and z ranges of the DataPoints collection
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
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

            double zScale = maxX / maxZ * 0.2;

            int numberOfXAxisTicks = 5;
            int numberOfYAxisTicks = 5;
            int numberOfZAxisTicks = 5;

            double xAxisInterval = (maxX - minX) / numberOfXAxisTicks;
            double yAxisInterval = (maxY - minY) / numberOfYAxisTicks;
            double zAxisInterval = Math.Ceiling(maxZ) / numberOfZAxisTicks;

            double axesOffset = xAxisInterval / 5;

            // Set the texture coordinates by either z-value or ColorValue
            var textureCoordinates = new Point[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var tc = (DataPoints[i, j].Z - minZ) / (maxZ - minZ);
                    textureCoordinates[i, j] = new Point(tc, tc);
                }
            }

            // Build the surface model (i.e. the coloured surface model)
            MeshBuilder surfaceModelBuilder = new MeshBuilder();
            surfaceModelBuilder.AddRectangularMesh(DataPoints, textureCoordinates);

            GeometryModel3D surfaceModel = new GeometryModel3D(surfaceModelBuilder.ToMesh(), MaterialHelper.CreateMaterial(SurfaceBrush, null, null, 1));
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
                label.Position = new Point3D(x, maxY + axesOffset, 0);
                axesLabelsModel.Children.Add(label);
            }

            // Loop through y intervals - for the surface meshlines, the grid, and Y axes ticks
            //for (double y = minY; y <= maxY + 0.0001; y += YAxisInterval)
            for (double y = minY + yAxisInterval; y < maxY; y += yAxisInterval)
            {
                // Axes labels
                BillboardTextVisual3D label = new BillboardTextVisual3D();
                label.Text = string.Format("{0:F1}", y);
                label.Position = new Point3D(maxX + axesOffset, y, 0);
                axesLabelsModel.Children.Add(label);
            }

            //添加底层方框
            var gridPath = new List<Point3D>();
            gridPath.Add(new Point3D(minX, minY, 0));
            gridPath.Add(new Point3D(maxX, minY, 0));
            gridPath.Add(new Point3D(maxX, maxY, 0));
            gridPath.Add(new Point3D(minX, maxY, 0));
            gridBuilder.AddTube(gridPath, lineThickness, 9, true);

            //gridBuilder.AddArrow(new Point3D(0, 0, 0), new Point3D(maxX, 0, 0), 9);
            //gridBuilder.AddArrow(new Point3D(0, 0, 0), new Point3D(0, maxY, 0), 9);
            //gridBuilder.AddArrow(new Point3D(0, 0, 0), new Point3D(0, 0, maxZ), 9);

            // Loop through z intervals - for the grid, and Z axes ticks
            //for (double z = minZ; z <= maxZ + 0.0001; z += ZAxisInterval)
            for (double z = 0; z <= maxZ + zAxisInterval + 0.0001; z += zAxisInterval)
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
                label.Position = new Point3D(maxX + axesOffset, minY + axesOffset, z);
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

            this.Transform = new ScaleTransform3D(1, 1, zScale);

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
