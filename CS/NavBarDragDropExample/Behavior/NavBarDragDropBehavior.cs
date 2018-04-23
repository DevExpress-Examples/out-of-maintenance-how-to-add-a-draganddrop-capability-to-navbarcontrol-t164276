using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.NavBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NavBarDragDropExample
{

    public enum Position
    {
        Top,
        Bottom
    }

    public class PositionAdorner : Adorner
    {
        public Position currentPosition;

        public PositionAdorner(UIElement adornedElement, Position position)
            : base(adornedElement)
        {
            currentPosition = position;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);
            Pen renderPen = new Pen(new SolidColorBrush(Colors.DarkCyan), 3);

            if (currentPosition == Position.Top)
            {
                drawingContext.DrawLine(renderPen, new Point(0, 0), new Point(adornedElementRect.Width, 0));
            }
            else if (currentPosition == Position.Bottom)
            {
                drawingContext.DrawLine(renderPen, new Point(0, adornedElementRect.Height), new Point(adornedElementRect.Width, adornedElementRect.Height));
            }
        }
    }

    public class NavBarDragDropBehavior : Behavior<UIElement>
    {
        public NavBarControl NavBar;
        public Point MouseLeftButtonPoint;
        public FrameworkElement PreviousElement = null;
        public Position AdornerPosition;

        private string F_FLAG = "NavBarDragDrop";

        protected override void OnAttached()
        {
            base.OnAttached();
            NavBar = this.AssociatedObject as NavBarControl;
            NavBar.Loaded += NavBar_Loaded;
        }

        void NavBar_Loaded(object sender, RoutedEventArgs e)
        {
            NavBar.AllowDrop = true;
            NavBar.PreviewMouseMove += NavBar_PreviewMouseMove;
            NavBar.PreviewMouseLeftButtonDown += NavBar_PreviewMouseLeftButtonDown;
            NavBar.DragOver += NavBar_DragOver;
            NavBar.Drop += NavBar_Drop;
        }

        void NavBar_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseLeftButtonPoint = e.GetPosition(null);
        }

        void NavBar_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && CheckPosition(MouseLeftButtonPoint - e.GetPosition(null)))
            {
                var source = e.OriginalSource as NavBarItemControl;

                if (source != null)
                {
                    var item = source.DataContext as NavBarItem;
                    var data = new DataObject(F_FLAG, item);
                    DragDrop.DoDragDrop(NavBar, data, DragDropEffects.Copy);
                }
            }
        }

        void NavBar_Drop(object sender, DragEventArgs e)
        {
            DropItem(e);
        }

        void NavBar_DragOver(object sender, DragEventArgs e)
        {
            DragItem(e);
        }

        void DropItem(DragEventArgs args)
        {
            var dropSource = args.OriginalSource;
            var itemControl = LayoutHelper.FindParentObject<NavBarItemControl>(dropSource as DependencyObject);
            var groupHeader = LayoutHelper.FindParentObject<NavBarGroupHeader>(dropSource as DependencyObject);
            var item = args.Data.GetData(F_FLAG) as NavBarItem;

            if (item != null)
            {
                int postIndex;
                var navBarItem = new NavBarItem();
                var navBarGroup = new NavBarGroup();
                var currentGroup = item.Group;

                if (itemControl != null)
                {
                    navBarItem = itemControl.DataContext as NavBarItem;
                }
                else navBarItem = null;

                if (navBarItem != null)
                {
                    navBarGroup = navBarItem.Group;
                }

                else if (groupHeader != null)
                {
                    navBarGroup = groupHeader.DataContext as NavBarGroup;
                }
                else return;

                if (navBarItem != null)
                {
                    postIndex = navBarGroup.Items.IndexOf(navBarItem);
                }
                else
                {
                    postIndex = navBarGroup.Items.Count;
                }

                var currentIndex = navBarGroup.Items.IndexOf(item);

                currentGroup.Items.Remove(item);

                if ((postIndex > navBarGroup.Items.Count && currentGroup != navBarGroup) || (AdornerPosition == Position.Top && postIndex - 1 >= 0 && currentGroup == navBarGroup && currentIndex < postIndex))
                {
                    postIndex--;
                }
                else if (currentGroup == navBarGroup && postIndex > currentGroup.Items.Count)
                {
                    postIndex = currentIndex;
                }
                else if ((AdornerPosition == Position.Bottom && currentGroup != navBarGroup) || (AdornerPosition == Position.Bottom && postIndex + 1 < navBarGroup.Items.Count && currentGroup == navBarGroup && currentIndex > postIndex))
                {
                    postIndex++;
                }
                navBarGroup.Items.Insert(postIndex, item);
                DeleteAdorner();
            }
        }

        void DragItem(DragEventArgs args)
        {
            AdornerPosition = new Position();

            var mousePoint = args.GetPosition(NavBar);

            var item = args.Data.GetData(F_FLAG) as NavBarItem;

            var dropSource = args.OriginalSource;
            var itemControl = LayoutHelper.FindParentObject<NavBarItemControl>(dropSource as DependencyObject);
            var groupHeader = LayoutHelper.FindParentObject<NavBarGroupHeader>(dropSource as DependencyObject);

            var navBarItem = new NavBarItem();

            if (itemControl != null)
            {
                var frameworkElement = itemControl as FrameworkElement;
                GeneralTransform generalTransform = frameworkElement.TransformToVisual(NavBar);
                Rect rectangle = generalTransform.TransformBounds(new Rect(new Point(frameworkElement.Margin.Left, frameworkElement.Margin.Top), frameworkElement.RenderSize));

                var center = rectangle.Y + rectangle.Height/2;

                if (mousePoint.Y > center)
                {
                    AdornerPosition = Position.Bottom;
                }

                else AdornerPosition = Position.Top;

                SetAdorner(itemControl as FrameworkElement, AdornerPosition);
            }
            else
            {
                DeleteAdorner();
            }

            if (!args.Data.GetDataPresent(F_FLAG))
            {
                args.Effects = DragDropEffects.None;
            }
        }

        void SetAdorner(FrameworkElement currentElement, Position position)
        {
            var currentAdornerLayer = AdornerLayer.GetAdornerLayer(currentElement);
            var currentAdorner = new PositionAdorner(currentElement, position);
            currentAdorner.IsHitTestVisible = false;
            var controlAdorners = currentAdornerLayer.GetAdorners(currentElement);
            DeleteAdorner();
            currentAdornerLayer.Add(currentAdorner);
            PreviousElement = currentElement;
        }

        void DeleteAdorner()
        {
            if (PreviousElement != null)
            {
                var previousAdornerLayer = AdornerLayer.GetAdornerLayer(PreviousElement);

                if (previousAdornerLayer != null)
                {
                    var previousAdorners = previousAdornerLayer.GetAdorners(PreviousElement);

                    if (previousAdorners != null)
                    {
                        previousAdornerLayer.Remove(previousAdorners[0]);
                    }
                }
            }
        }

        private bool CheckPosition(Vector position)
        {
            return (Math.Abs(position.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y) > SystemParameters.MinimumVerticalDragDistance);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
