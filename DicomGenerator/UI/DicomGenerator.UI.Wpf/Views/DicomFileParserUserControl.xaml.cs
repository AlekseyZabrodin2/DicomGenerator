using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DicomGenerator.UI.Wpf.ViewModels;

namespace DicomGenerator.UI.Wpf.Views
{
    /// <summary>
    /// Interaction logic for DicomFileParserUserControl.xaml
    /// </summary>
    public partial class DicomFileParserUserControl : UserControl
    {

        private Storyboard _paperAnimation;

        public DicomFileParserViewModel ViewModel = new();

        public DicomFileParserUserControl()
        {
            InitializeComponent();
            DataContext = ViewModel;

            ViewModel.BusyAnimationStarted += StartPaperAnimation;
            ViewModel.BusyAnimationStopped += StopPaperAnimation;
        }

        private void StartPaperAnimation()
        {
            StopPaperAnimation();

            Canvas.SetLeft(Paper1, 30);
            Canvas.SetTop(Paper1, 25);

            Canvas.SetLeft(Paper2, 30);
            Canvas.SetTop(Paper2, 25);

            Canvas.SetLeft(Paper3, 30);
            Canvas.SetTop(Paper3, 25);

            StartPaper(
                Paper1,
                Paper1Translate,
                Paper1Rotate,
                TimeSpan.Zero);

            StartPaper(
                Paper2,
                Paper2Translate,
                Paper2Rotate,
                TimeSpan.FromMilliseconds(600));

            StartPaper(
                Paper3,
                Paper3Translate,
                Paper3Rotate,
                TimeSpan.FromMilliseconds(1200));
        }

        private void StartPaper(FrameworkElement paper, TranslateTransform translate, RotateTransform rotate, TimeSpan delay)
        {
            paper.Opacity = 0;
            translate.X = 0;
            translate.Y = 0;
            rotate.Angle = -10;

            var moveX = new DoubleAnimation
            {
                From = 0,
                To = 175,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = delay,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var moveY = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = delay,
                RepeatBehavior = RepeatBehavior.Forever
            };

            moveY.KeyFrames.Add(
                new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));

            moveY.KeyFrames.Add(
                new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1500))));

            moveY.KeyFrames.Add(
                new EasingDoubleKeyFrame
                {
                    Value = 4,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000)),
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseIn
                    }
                });

            var rotation = new DoubleAnimation
            {
                From = -30,
                To = -5,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = delay,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var opacity = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = delay,
                RepeatBehavior = RepeatBehavior.Forever
            };

            opacity.KeyFrames.Add(
                new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));

            opacity.KeyFrames.Add(
                new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));

            opacity.KeyFrames.Add(
                new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1600))));

            opacity.KeyFrames.Add(
                new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000))));

            translate.BeginAnimation(TranslateTransform.XProperty, moveX);
            translate.BeginAnimation(TranslateTransform.YProperty, moveY);
            rotate.BeginAnimation(RotateTransform.AngleProperty, rotation);
            paper.BeginAnimation(UIElement.OpacityProperty, opacity);
        }

        private void StopPaperAnimation()
        {
            StopPaper(Paper1, Paper1Translate, Paper1Rotate);
            StopPaper(Paper2, Paper2Translate, Paper2Rotate);
            StopPaper(Paper3, Paper3Translate, Paper3Rotate);
        }

        private void StopPaper(FrameworkElement paper, TranslateTransform translate, RotateTransform rotate)
        {
            translate.BeginAnimation(TranslateTransform.XProperty, null);

            rotate.BeginAnimation(RotateTransform.AngleProperty, null);

            paper.BeginAnimation(UIElement.OpacityProperty, null);

            translate.X = 0;
            rotate.Angle = 0;
            paper.Opacity = 0;
        }
    }
}
