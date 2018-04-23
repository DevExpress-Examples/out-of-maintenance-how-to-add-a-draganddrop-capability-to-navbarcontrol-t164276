Imports Microsoft.VisualBasic
Imports DevExpress.Mvvm.UI.Interactivity
Imports DevExpress.Xpf.Core.Native
Imports DevExpress.Xpf.NavBar
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Documents
Imports System.Windows.Media

Namespace NavBarDragDropExample

	Public Enum Position
		Top
		Bottom
	End Enum

	Public Class PositionAdorner
		Inherits Adorner
		Public currentPosition As Position

		Public Sub New(ByVal adornedElement As UIElement, ByVal position As Position)
			MyBase.New(adornedElement)
			currentPosition = position
		End Sub

		Protected Overrides Sub OnRender(ByVal drawingContext As DrawingContext)
			Dim adornedElementRect As New Rect(Me.AdornedElement.RenderSize)
			Dim renderPen As New Pen(New SolidColorBrush(Colors.DarkCyan), 3)

			If currentPosition = Position.Top Then
				drawingContext.DrawLine(renderPen, New Point(0, 0), New Point(adornedElementRect.Width, 0))
			ElseIf currentPosition = Position.Bottom Then
				drawingContext.DrawLine(renderPen, New Point(0, adornedElementRect.Height), New Point(adornedElementRect.Width, adornedElementRect.Height))
			End If
		End Sub
	End Class

	Public Class NavBarDragDropBehavior
		Inherits Behavior(Of UIElement)
		Public NavBar As NavBarControl
		Public MouseLeftButtonPoint As Point
		Public PreviousElement As FrameworkElement = Nothing
		Public AdornerPosition As Position

		Private F_FLAG As String = "NavBarDragDrop"

		Protected Overrides Sub OnAttached()
			MyBase.OnAttached()
			NavBar = TryCast(Me.AssociatedObject, NavBarControl)
			AddHandler NavBar.Loaded, AddressOf NavBar_Loaded
		End Sub

		Private Sub NavBar_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			NavBar.AllowDrop = True
			AddHandler NavBar.PreviewMouseMove, AddressOf NavBar_PreviewMouseMove
			AddHandler NavBar.PreviewMouseLeftButtonDown, AddressOf NavBar_PreviewMouseLeftButtonDown
			AddHandler NavBar.DragOver, AddressOf NavBar_DragOver
			AddHandler NavBar.Drop, AddressOf NavBar_Drop
		End Sub

		Private Sub NavBar_PreviewMouseLeftButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
			MouseLeftButtonPoint = e.GetPosition(Nothing)
		End Sub

		Private Sub NavBar_PreviewMouseMove(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
			If e.LeftButton = System.Windows.Input.MouseButtonState.Pressed AndAlso CheckPosition(MouseLeftButtonPoint - e.GetPosition(Nothing)) Then
				Dim source = TryCast(e.OriginalSource, NavBarItemControl)

				If source IsNot Nothing Then
					Dim item = TryCast(source.DataContext, NavBarItem)
					Dim data = New DataObject(F_FLAG, item)
					DragDrop.DoDragDrop(NavBar, data, DragDropEffects.Copy)
				End If
			End If
		End Sub

		Private Sub NavBar_Drop(ByVal sender As Object, ByVal e As DragEventArgs)
			DropItem(e)
		End Sub

		Private Sub NavBar_DragOver(ByVal sender As Object, ByVal e As DragEventArgs)
			DragItem(e)
		End Sub

		Private Sub DropItem(ByVal args As DragEventArgs)
			Dim dropSource = args.OriginalSource
			Dim itemControl = LayoutHelper.FindParentObject(Of NavBarItemControl)(TryCast(dropSource, DependencyObject))
			Dim groupHeader = LayoutHelper.FindParentObject(Of NavBarGroupHeader)(TryCast(dropSource, DependencyObject))
			Dim item = TryCast(args.Data.GetData(F_FLAG), NavBarItem)

			If item IsNot Nothing Then
				Dim postIndex As Integer
				Dim navBarItem = New NavBarItem()
				Dim navBarGroup = New NavBarGroup()
				Dim currentGroup = item.Group

				If itemControl IsNot Nothing Then
					navBarItem = TryCast(itemControl.DataContext, NavBarItem)
				Else
					navBarItem = Nothing
				End If

				If navBarItem IsNot Nothing Then
					navBarGroup = navBarItem.Group

				ElseIf groupHeader IsNot Nothing Then
					navBarGroup = TryCast(groupHeader.DataContext, NavBarGroup)
				Else
					Return
				End If

				If navBarItem IsNot Nothing Then
					postIndex = navBarGroup.Items.IndexOf(navBarItem)
				Else
					postIndex = navBarGroup.Items.Count
				End If

				Dim currentIndex = navBarGroup.Items.IndexOf(item)

				currentGroup.Items.Remove(item)

                If (postIndex > navBarGroup.Items.Count AndAlso currentGroup IsNot navBarGroup) OrElse (AdornerPosition = Position.Top AndAlso postIndex - 1 >= 0 AndAlso currentGroup Is navBarGroup AndAlso currentIndex < postIndex) Then
                    postIndex -= 1
                ElseIf currentGroup Is navBarGroup AndAlso postIndex > currentGroup.Items.Count Then
                    postIndex = currentIndex
                ElseIf (AdornerPosition = Position.Bottom AndAlso currentGroup IsNot navBarGroup) OrElse (AdornerPosition = Position.Bottom AndAlso postIndex + 1 < navBarGroup.Items.Count AndAlso currentGroup Is navBarGroup AndAlso currentIndex > postIndex) Then
                    postIndex += 1
                End If
				navBarGroup.Items.Insert(postIndex, item)
				DeleteAdorner()
			End If
		End Sub

		Private Sub DragItem(ByVal args As DragEventArgs)
			AdornerPosition = New Position()

			Dim mousePoint = args.GetPosition(NavBar)

			Dim item = TryCast(args.Data.GetData(F_FLAG), NavBarItem)

			Dim dropSource = args.OriginalSource
			Dim itemControl = LayoutHelper.FindParentObject(Of NavBarItemControl)(TryCast(dropSource, DependencyObject))
			Dim groupHeader = LayoutHelper.FindParentObject(Of NavBarGroupHeader)(TryCast(dropSource, DependencyObject))

			Dim navBarItem = New NavBarItem()

			If itemControl IsNot Nothing Then
				Dim frameworkElement = TryCast(itemControl, FrameworkElement)
				Dim generalTransform As GeneralTransform = frameworkElement.TransformToVisual(NavBar)
				Dim rectangle As Rect = generalTransform.TransformBounds(New Rect(New Point(frameworkElement.Margin.Left, frameworkElement.Margin.Top), frameworkElement.RenderSize))

				Dim center = rectangle.Y + rectangle.Height/2

				If mousePoint.Y > center Then
					AdornerPosition = Position.Bottom

				Else
					AdornerPosition = Position.Top
				End If

				SetAdorner(TryCast(itemControl, FrameworkElement), AdornerPosition)
			Else
				DeleteAdorner()
			End If

			If (Not args.Data.GetDataPresent(F_FLAG)) Then
				args.Effects = DragDropEffects.None
			End If
		End Sub

		Private Sub SetAdorner(ByVal currentElement As FrameworkElement, ByVal position As Position)
			Dim currentAdornerLayer = AdornerLayer.GetAdornerLayer(currentElement)
			Dim currentAdorner = New PositionAdorner(currentElement, position)
			currentAdorner.IsHitTestVisible = False
			Dim controlAdorners = currentAdornerLayer.GetAdorners(currentElement)
			DeleteAdorner()
			currentAdornerLayer.Add(currentAdorner)
			PreviousElement = currentElement
		End Sub

		Private Sub DeleteAdorner()
			If PreviousElement IsNot Nothing Then
				Dim previousAdornerLayer = AdornerLayer.GetAdornerLayer(PreviousElement)

				If previousAdornerLayer IsNot Nothing Then
					Dim previousAdorners = previousAdornerLayer.GetAdorners(PreviousElement)

					If previousAdorners IsNot Nothing Then
						previousAdornerLayer.Remove(previousAdorners(0))
					End If
				End If
			End If
		End Sub

		Private Function CheckPosition(ByVal position As Vector) As Boolean
			Return (Math.Abs(position.X) > SystemParameters.MinimumHorizontalDragDistance OrElse Math.Abs(position.Y) > SystemParameters.MinimumVerticalDragDistance)
		End Function

		Protected Overrides Sub OnDetaching()
			MyBase.OnDetaching()
		End Sub
	End Class
End Namespace
