using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFFourier_Plotter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables

        //Time variables
        public DispatcherTimer Timer;
        Stopwatch stopWatch = new Stopwatch();

        //Elements
        Ellipse pointerEllipse;
        Ellipse[] ellipses = new Ellipse[100];
        Line[] lines = new Line[100];
        List<Point> ellipsePointer = new List<Point>();
        List<double> frequencies = new List<double>();
        Brush brushEllipse= Brushes.Black;
        Brush brushLine= Brushes.Black;
        List<Point> points = new List<Point>();
        Path path;
        
        //Others
        double counter = 0;
        bool reachedMaximum = false;

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new List<Circle>();
            CreateNewTimer();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreatePointerEllipse();
        }

        #region Supporter Functions
        private void CreateNewTimer()
        {
            Timer = new DispatcherTimer();
            Timer.Tick += new EventHandler(TimerOnTick);
            Timer.Interval = TimeSpan.FromMilliseconds(1);
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            List<Circle> datas = (List<Circle>)DataContext;
            if (!reachedMaximum)
            {
                counter+=0.6 ;

                for (int i = 0; i < datas.Count; i++)
                {
                    if (i == 0)
                    {
                        plotterCanvas.Children.Remove(lines[i]);
                        lines[i].X2 = ellipsePointer[i].X + ellipses[i].Width / 2 + datas[i].radius * Math.Sin((counter * frequencies[i] * Math.PI) / 180 + Math.PI / 2);
                        lines[i].Y2 = ellipsePointer[i].Y + ellipses[i].Height / 2 + datas[i].radius * Math.Cos((counter  * frequencies[i] * Math.PI) / 180 + Math.PI / 2);
                        plotterCanvas.Children.Add(lines[i]);
                    }
                    else
                    {
                        plotterCanvas.Children.Remove(ellipses[i]);
                        Canvas.SetLeft(ellipses[i], lines[i - 1].X2 - ellipses[i].Width / 2);
                        Canvas.SetTop(ellipses[i], lines[i - 1].Y2 - ellipses[i].Height / 2);
                        plotterCanvas.Children.Add(ellipses[i]);

                        plotterCanvas.Children.Remove(lines[i]);
                        lines[i].X1 = lines[i - 1].X2;
                        lines[i].Y1 = lines[i - 1].Y2;
                        lines[i].X2 = lines[i - 1].X2 + datas[i].radius * Math.Sin((counter * frequencies[i] * Math.PI) / 180 + Math.PI / 2);
                        lines[i].Y2 = lines[i - 1].Y2 + datas[i].radius * Math.Cos((counter * frequencies[i] * Math.PI) / 180 + Math.PI / 2);
                        plotterCanvas.Children.Add(lines[i]);
                    }

                }

                plotterCanvas.Children.Remove(pointerEllipse);
                Canvas.SetLeft(pointerEllipse, lines[datas.Count - 1].X2 - pointerEllipse.Width / 2);
                Canvas.SetTop(pointerEllipse, lines[datas.Count - 1].Y2 - pointerEllipse.Height / 2);
                plotterCanvas.Children.Add(pointerEllipse);

                points.Add(new Point(lines[datas.Count - 1].X2, lines[datas.Count - 1].Y2));
                plotterCanvas.Children.Remove(path);
                path = MakeBezierPath(points);
                path.Stroke = Brushes.Blue;
                path.StrokeThickness = 1;
                plotterCanvas.Children.Add(path);

                progressBar.Value += 1;
            }
            else
            {
                Timer.Stop();
                stopWatch.Stop();
            }

        }
        private Path MakeBezierPath(List<Point> points)
        { 
            Path path = new Path();
            PathGeometry path_geometry = new PathGeometry();
            path.Data = path_geometry;
            PathFigure path_figure = new PathFigure();
            path_geometry.Figures.Add(path_figure);
            path_figure.StartPoint = points[0];
            PathSegmentCollection path_segment_collection =new PathSegmentCollection();
            path_figure.Segments = path_segment_collection;
            PointCollection point_collection =new PointCollection(points.Count - 1);
            for (int i = 1; i < points.Count; i++)
                point_collection.Add(points[i]);
            PolyBezierSegment bezier_segment = new PolyBezierSegment();
            bezier_segment.Points = point_collection;
            path_segment_collection.Add(bezier_segment);
            return path;
        }
        private void CreateStructures()
        {
            List<Circle> datas = (List<Circle>)DataContext;

            //stopWatch.Stop();
            if(frequencies!=null)
            {
                plotterCanvas.Children.Clear();
                ellipsePointer.Clear();
                frequencies.Clear();
            }

            //Draw Ellipses
            for (int i = 0; i < datas.Count; i++)
            {
                frequencies.Add(datas[i].frequency);
                ellipses[i] = new Ellipse()
                {
                    Width = 2 * datas[i].radius,
                    Height = 2 * datas[i].radius,
                    Stroke = brushEllipse,
                    StrokeThickness = 1
                };
                plotterCanvas.Children.Add(ellipses[i]);
            }

            //Set position of ellipses
            for (int i = 0; i < datas.Count; i++)
            {
                Point point;
                if (i == 0) point = new Point((plotterCanvas.ActualWidth - ellipses[i].Width) / 2, (plotterCanvas.ActualHeight - ellipses[i].Height) / 2);
                else point = new Point(ellipsePointer[i - 1].X + ellipses[i - 1].Width - ellipses[i].Width / 2, ellipsePointer[i - 1].Y + ellipses[i - 1].Height / 2 - ellipses[i].Height / 2);
                ellipsePointer.Add(point);
                Canvas.SetLeft(ellipses[i], point.X);
                Canvas.SetTop(ellipses[i], point.Y);
            }

            //Draw lines
            for(int i=0;i<datas.Count;i++)
            {
                lines[i] = new Line()
                {
                    Stroke = brushLine,
                    StrokeThickness = 1,
                    X1 = ellipsePointer[i].X+ellipses[i].Width/2,
                    Y1 = ellipsePointer[i].Y+ ellipses[i].Height/2,
                    X2 = ellipsePointer[i].X + ellipses[i].Width,
                    Y2 = ellipsePointer[i].Y + ellipses[i].Height / 2,
                };
                plotterCanvas.Children.Add(lines[i]);
            }
            CreatePointerEllipse();
        }

        private void CreatePointerEllipse()
        {
            pointerEllipse = new Ellipse()
            {
                Width = 5,
                Height = 5,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                Fill = Brushes.Red
            };

            if (ellipsePointer.Count>0)
            {
                Canvas.SetLeft(pointerEllipse, ellipsePointer[ellipsePointer.Count - 1].X+ ellipses[ellipsePointer.Count - 1].Width- pointerEllipse.Width/2);
                Canvas.SetTop(pointerEllipse, ellipsePointer[ellipsePointer.Count - 1].Y + ellipses[ellipsePointer.Count - 1].Height/2 - pointerEllipse.Width / 2);
            }
            else
            {
                Canvas.SetLeft(pointerEllipse, (plotterCanvas.ActualWidth - pointerEllipse.Width) / 2);
                Canvas.SetTop(pointerEllipse, (plotterCanvas.ActualHeight - pointerEllipse.Height) / 2);
            }
            plotterCanvas.Children.Add(pointerEllipse);
        }

        private void ActionRestart()
        {
            counter = 0;
            stopWatch.Reset();
            Timer.Stop();
            CreateNewTimer();
            plotterCanvas.Children.Clear() ;
            ellipsePointer.Clear();
            frequencies.Clear();
            points.Clear();
            reachedMaximum = false;
            progressBar.Value = 0;
        }
        #endregion

        #region MenuItem Events
        private void New_Click(object sender, RoutedEventArgs e)
        {
            ActionRestart();
            DataContext = new List<Circle> { } ;
            CreatePointerEllipse();
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "XML Files (*.xml) | *.xml| All files (*.*) | *.*",
                Title = "Open"
            };
            bool? res = openFileDialog.ShowDialog();
            if (res==true)
            {
                try
                {
                    CircleList circleList = CircleList.Load(openFileDialog.FileName);
                    List<Circle> circles = new List<Circle>();
                    foreach (var c in circleList.circles)
                    {
                        circles.Add(new Circle() { radius = c.radius, frequency = c.frequency });
                    }
                    DataContext = circles;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Choose correct .xml file\n{ex.Message}", "Error during file opening", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            CreateStructures();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Files (*.xml) | *.xml| All files (*.*) | *.*";
            saveFileDialog.Title = "Save as";
            if (saveFileDialog.ShowDialog() == true)
            {
                if (saveFileDialog.FileName != "")
                {
                    CircleList circleList = new CircleList()
                    {
                        circles=(List<Circle>)DataContext
                    };
                    circleList.Save(saveFileDialog.FileName);
                }

            }
        }
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to leave?", "Exit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }
        private void Circle_Checked(object sender, RoutedEventArgs e)
        {
            brushEllipse = Brushes.Black;
        }
        private void Circle_Unchecked(object sender, RoutedEventArgs e)
        {
            brushEllipse = Brushes.White;
        } 
        private void Line_Checked(object sender, RoutedEventArgs e)
        {
            brushLine = Brushes.Black;
        }
        private void Line_Unchecked(object sender, RoutedEventArgs e)
        {
            brushLine = Brushes.White;
        }
        #endregion

        #region Action Events
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Start();
            Timer.Start();
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Stop();
            Timer.Stop();
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            ActionRestart();
            CreateStructures();
        }
        #endregion

        #region Control Events
        private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (progressBar.Maximum == e.NewValue)
            {
                reachedMaximum = true;
                CreateStructures();
                plotterCanvas.Children.Add(path);
            }
        }

        #endregion

        #region DataGrid
        private void dataGridInfo_CurrentCellChanged(object sender, EventArgs e)
        {
            CreateStructures();
        }

        #endregion
    }

}
