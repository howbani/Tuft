using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Tuft.Dataplane;
using System.Windows.Media.Animation;
using Tuft.ui;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Shapes;

namespace Tuft.Intilization
{
	public sealed class Arrow : Shape
	{
        
        public Sensor To { get; set; }
        public Sensor From { get; set; }

        //  public Storyboard PacketAnimator { get; set; } 

        private static Stack<Border> MovedObjectStack = new Stack<Border>(); 
        #region Dependency Properties

        public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)); 
		public static readonly DependencyProperty HeadWidthProperty = DependencyProperty.Register("HeadWidth", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty HeadHeightProperty = DependencyProperty.Register("HeadHeight", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)); 

		#endregion

		#region CLR Properties

		[TypeConverter(typeof(LengthConverter))]
		public double X1
		{
			get { return (double)base.GetValue(X1Property); }
			set { base.SetValue(X1Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double Y1
		{
			get { return (double)base.GetValue(Y1Property); }
			set { base.SetValue(Y1Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double X2
		{
			get { return (double)base.GetValue(X2Property); }
			set { base.SetValue(X2Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double Y2
		{
			get { return (double)base.GetValue(Y2Property); }
			set { base.SetValue(Y2Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double HeadWidth
		{
			get { return (double)base.GetValue(HeadWidthProperty); }
			set { base.SetValue(HeadWidthProperty, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double HeadHeight
		{
			get { return (double)base.GetValue(HeadHeightProperty); }
			set { base.SetValue(HeadHeightProperty, value); }
		}

		#endregion

		#region Overrides

		protected override Geometry DefiningGeometry
		{
			get
			{
				// Create a StreamGeometry for describing the shape
				StreamGeometry geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using (StreamGeometryContext context = geometry.Open())
				{
					InternalDrawArrowGeometry(context);
				}

				// Freeze the geometry for performance benefits
				geometry.Freeze();

				return geometry;
			}
		}		

		#endregion

		#region Privates

		private void InternalDrawArrowGeometry(StreamGeometryContext context)
		{
			double theta = Math.Atan2(Y1 - Y2, X1 - X2);
			double sint = Math.Sin(theta);
			double cost = Math.Cos(theta);

			Point pt1 = new Point(X1, this.Y1);
			Point pt2 = new Point(X2, this.Y2);

			Point pt3 = new Point(
				X2 + (HeadWidth * cost - HeadHeight * sint),
				Y2 + (HeadWidth * sint + HeadHeight * cost));

			Point pt4 = new Point(
				X2 + (HeadWidth * cost + HeadHeight * sint),
				Y2 - (HeadHeight * cost - HeadWidth * sint));

			context.BeginFigure(pt1, true, false);
			context.LineTo(pt2, true, true);
			context.LineTo(pt3, true, true);
			context.LineTo(pt2, true, true);
			context.LineTo(pt4, true, true);
		}

        #endregion


        // animation:
        private Storyboard IntilaizeStory(Point start, Point end, long colorID)
        {
            // Create a NameScope for the page so that
            // we can use Storyboards.
            NameScope.SetNameScope(PublicParameters.MainWindow.Canvas_SensingFeild, new NameScope());

            // Create a rectangle.
            Border aRectangle = new Border();
            aRectangle.CornerRadius = new CornerRadius(2);
            aRectangle.Width = 7;
            aRectangle.Height = 7;
            int cid = Convert.ToInt16(colorID % PublicParameters.RandomColors.Count);
            aRectangle.Background = new SolidColorBrush(PublicParameters.RandomColors[cid]);
            aRectangle.BorderThickness = new Thickness(1);
            aRectangle.BorderBrush = new SolidColorBrush(Colors.Black);
            
            MovedObjectStack.Push(aRectangle);

            // Create a transform. This transform
            // will be used to move the rectangle.
            TranslateTransform animatedTranslateTransform = new TranslateTransform();

            // Register the transform's name with the page
            // so that they it be targeted by a Storyboard.
            PublicParameters.MainWindow.Canvas_SensingFeild.RegisterName("AnimatedTranslateTransform", animatedTranslateTransform);
            aRectangle.RenderTransform = animatedTranslateTransform;
            PublicParameters.MainWindow.Canvas_SensingFeild.Children.Add(aRectangle);

           

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure(); // add the figure.
            pFigure.StartPoint = start; // define the start point of the Figure.
            LineSegment lineSegment = new LineSegment(); // move according to line.
            lineSegment.Point = end; // set the end point.
            pFigure.Segments.Add(lineSegment); // add the line to the figure.
            animationPath.Figures.Add(pFigure); // add the figure to the path.

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a DoubleAnimationUsingPath to move the
            // rectangle horizontally along the path by animating 
            // its TranslateTransform.
            DoubleAnimationUsingPath translateXAnimation =
                new DoubleAnimationUsingPath();
            translateXAnimation.PathGeometry = animationPath;
            translateXAnimation.Duration = TimeSpan.FromSeconds(Properties.Settings.Default.AnimationSpeed);

            // Set the Source property to X. This makes
            // the animation generate horizontal offset values from
            // the path information. 
            translateXAnimation.Source = PathAnimationSource.X;

            // Set the animation to target the X property
            // of the TranslateTransform named "AnimatedTranslateTransform".
            Storyboard.SetTargetName(translateXAnimation, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(translateXAnimation,
                new PropertyPath(TranslateTransform.XProperty));

            // Create a DoubleAnimationUsingPath to move the
            // rectangle vertically along the path by animating 
            // its TranslateTransform.
            DoubleAnimationUsingPath translateYAnimation =
                new DoubleAnimationUsingPath();
            translateYAnimation.PathGeometry = animationPath;
            translateYAnimation.Duration = TimeSpan.FromSeconds(Properties.Settings.Default.AnimationSpeed);

            // Set the Source property to Y. This makes
            // the animation generate vertical offset values from
            // the path information. 
            translateYAnimation.Source = PathAnimationSource.Y;

            // Set the animation to target the Y property
            // of the TranslateTransform named "AnimatedTranslateTransform".
            Storyboard.SetTargetName(translateYAnimation, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(translateYAnimation,
                new PropertyPath(TranslateTransform.YProperty));

            // Create a Storyboard to contain and apply the animations.
            Storyboard pathAnimationStoryboard = new Storyboard();
            //pathAnimationStoryboard.RepeatBehavior = RepeatBehavior.Forever;
            pathAnimationStoryboard.Children.Add(translateXAnimation);
            pathAnimationStoryboard.Children.Add(translateYAnimation);

          



            return pathAnimationStoryboard;
        }



        private void PathAnimationStoryboard_Completed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => DefualtValues(), System.Windows.Threading.DispatcherPriority.Send);
        }

        public void DefualtValues()
        {

            while (MovedObjectStack.Count > 0)
            {
                Border rq = MovedObjectStack.Pop();
                PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => PublicParameters.MainWindow.Canvas_SensingFeild.Children.Remove(rq), System.Windows.Threading.DispatcherPriority.Send);
            }
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => Stroke = Brushes.Gray);
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => StrokeThickness = 0.2);
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => HeadHeight = 0);
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => HeadWidth = 0);
          //  PublicParamerters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => From.Ellipse_Communication_range.Visibility = Visibility.Hidden, System.Windows.Threading.DispatcherPriority.Send);

        }

        public void  AnimateValues(long PID) 
        {
            Storyboard PacketAnimator = IntilaizeStory(new Point(X1, Y1), new Point(X2, Y2), PID);
            PacketAnimator.Completed += PathAnimationStoryboard_Completed;
            PacketAnimator.Begin(PublicParameters.MainWindow.Canvas_SensingFeild);
           // PacketAnimator.Remove(PublicParamerters.MainWindow.Canvas_SensingFeild);

            int cid = Convert.ToInt16(PID % PublicParameters.RandomColors.Count);
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => Stroke = new SolidColorBrush(PublicParameters.RandomColors[cid]));
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => StrokeThickness = 1);
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => HeadHeight = 4);
            PublicParameters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => HeadWidth = 4);
         //   PublicParamerters.MainWindow.Canvas_SensingFeild.Dispatcher.Invoke(() => From.Ellipse_Communication_range.Visibility = Visibility.Visible);
        }


        public void BeginAnimation(long PID)  
        {
            Dispatcher.Invoke(() => AnimateValues(PID), System.Windows.Threading.DispatcherPriority.Send);
        }
    }
}
