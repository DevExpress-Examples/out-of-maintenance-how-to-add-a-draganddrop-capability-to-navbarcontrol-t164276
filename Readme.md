# How to add a DragAndDrop capability to NavBarControl


This example demonstrates how to add a DragAndDrop capability to NavBarControl.<br />Our NavBarControl doesn't support drag and drop out of the box. To provide this capability in this sample, we subscribe to the NavBarControl's DragOver, Drop, PreviewMouseMove and PreviewMouseLeftButtonDown events. When PreviewMouseMove is raised, we check e.OriginalSource and the mouse left button state. If e.OriginalSource is a NavBarItemControl and the left button was pressed, we call DragDrop's DoDragDrop method. When DragOver is raised, we draw an adorner on a position to which we can move a selected NavBarItem. When Drop is raised, we calculate a position to which we must insert a selected NavBarItem.

<br/>


