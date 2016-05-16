using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using DataProcessing.Parser;

namespace WPFSurfacePlot3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml - all of the application logic lives here
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string TouchFilePath = @"Datas/Chip{0}-Touch.csv";
        private const string UnTouchFilePath = @"Datas/Chip{0}-UnTouch.csv";

        private readonly SurfacePlotViewModel viewViewModel = new SurfacePlotViewModel();
        private Random random = new Random();

        /// <summary>
        /// Initialize the main window (hence, this function runs on application start).
        /// You should initialize your SurfacePlotViewModel here, and set it as the
        /// DataContext for your SurfacePlotView (which is defined in MainWindow.xaml).
        /// </summary>
        public MainWindow()
        {
            DataContext = viewViewModel;

            InitializeComponent();

            functionSelectorComboBox.ItemsSource = Enum.GetValues(typeof(FunctionOptions));
        }

        /// <summary>
        /// Used to control which demo function the user has chosen to display.
        /// </summary>
        enum FunctionOptions { Sinc, Ripple, Gaussian, Funnel, Origami, Simple, DataPlot };

        /// <summary>
        /// This function is called whenever the user selects a different demo function to plot.
        /// </summary>
        private void FunctionSelectionWasChanged(object sender, RoutedEventArgs e)
        {
            FunctionOptions currentOption = FunctionOptions.Simple;
            Func<double, double, double> function;

            if (functionSelectorComboBox.SelectedItem == null)
            {
                Console.WriteLine("No function selected");
            }
            else
            {
                currentOption = (FunctionOptions)functionSelectorComboBox.SelectedItem;
            }

            switch (currentOption)
            {
                case FunctionOptions.Gaussian:
                    function = (x, y) => 5 * Math.Exp(-1 * Math.Pow(x, 2) / 4 - Math.Pow(y, 2) / 4) / (Math.Sqrt(2 * Math.PI));
                    viewViewModel.PlotFunction(function, -5, 5, 200);
                    break;

                case FunctionOptions.Sinc:
                    function = (x, y) => 10 * Math.Sin(Math.Sqrt(x * x + y * y)) / Math.Sqrt(x * x + y * y);
                    viewViewModel.PlotFunction(function, -10, 10);
                    break;

                case FunctionOptions.Funnel:
                    function = (x, y) => -1 / (x * x + y * y);
                    viewViewModel.PlotFunction(function, -1, 1);
                    break;

                case FunctionOptions.Origami:
                    function = (x, y) => Math.Cos(Math.Abs(x) + Math.Abs(y)) * (Math.Abs(x) + Math.Abs(y));
                    viewViewModel.PlotFunction(function, -1, 1);
                    break;

                case FunctionOptions.Simple:
                    function = (x, y) => x * y;
                    viewViewModel.PlotFunction(function, -1, 1);
                    break;

                case FunctionOptions.Ripple:
                    function = (x, y) => 0.25 * Math.Sin(Math.PI * Math.PI * x * y);
                    viewViewModel.PlotFunction(function, 0, 2, 300);
                    break;

                case FunctionOptions.DataPlot:
                    PlotCustomData();
                    break;

                default:
                    function = (x, y) => 0;
                    viewViewModel.PlotFunction(function, -1, 1);
                    break;
            }
        }

        private void PlotCustomData()
        {
            var touchPath = string.Format(TouchFilePath, 1);
            var unTouchPath = string.Format(UnTouchFilePath, 1);
            var touchTestData = TestDataParser.Parser(touchPath);
            var unTouchTestData = TestDataParser.Parser(unTouchPath);
            var touchList = touchTestData.DaList[0];
            var unTouchList = unTouchTestData.DaList[0];
            var diffList = touchList.Zip(unTouchList, (x, y) => (double)Math.Abs(x - y)).ToArray();
            //viewViewModel.PlotData(touchList.Select<int, double>(x => x).ToArray(), 88);
            //viewViewModel.PlotData(unTouchList.Select<int, double>(x => x).ToArray(), 88);
            viewViewModel.PlotData(diffList, 88);

            //double[,] arrayOfPoints = new double[88, 108];
            //for (int i = 0; i < 88; i++)
            //{
            //    for (int j = 0; j < 108; j++)
            //    {
            //        //arrayOfPoints[i, j] = 10 * Math.Sin(Math.Sqrt(i * i + j * j)) / Math.Sqrt(i * i + j * j + 0.0001);
            //        arrayOfPoints[i, j] = random.Next(88, 90);
            //    }
            //}
            //viewViewModel.PlotData(arrayOfPoints);
        }
    }
}
