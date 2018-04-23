' Developer Express Code Central Example:
' How to add a DragAndDrop capability to NavBarControl
' 
' This example demonstrates how to add a DragAndDrop capability to
' NavBarControl.
' Our NavBarControl doesn't support drag and drop out of the box.
' To provide this capability in this sample, we subscribe to the NavBarControl's
' DragOver, Drop, PreviewMouseMove and PreviewMouseLeftButtonDown events. When
' PreviewMouseMove is raised, we check e.OriginalSource and the mouse left button
' state. If e.OriginalSource is a NavBarItemControl and the left button was
' pressed, we call DragDrop's DoDragDrop method. When DragOver is raised, we draw
' an adorner on a position to which we can move a selected NavBarItem. When Drop
' is raised, we calculate a position to which we must insert a selected
' NavBarItem.
' 
' You can find sample updates and versions for different programming languages here:
' http://www.devexpress.com/example=T164276


Imports Microsoft.VisualBasic
Imports System.Reflection
Imports System.Resources
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Windows

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.
<Assembly: AssemblyTitle("NavBarDragDropExample")>
<Assembly: AssemblyDescription("")>
<Assembly: AssemblyConfiguration("")>
<Assembly: AssemblyCompany("")>
<Assembly: AssemblyProduct("NavBarDragDropExample")>
<Assembly: AssemblyCopyright("Copyright ©  2014")>
<Assembly: AssemblyTrademark("")>
<Assembly: AssemblyCulture("")>

' Setting ComVisible to false makes the types in this assembly not visible 
' to COM components.  If you need to access a type in this assembly from 
' COM, set the ComVisible attribute to true on that type.
<Assembly: ComVisible(False)>

'In order to begin building localizable applications, set 
'<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
'inside a <PropertyGroup>.  For example, if you are using US english
'in your source files, set the <UICulture> to en-US.  Then uncomment
'the NeutralResourceLanguage attribute below.  Update the "en-US" in
'the line below to match the UICulture setting in the project file.

'[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


	'(used if a resource is not found in the page, 
	' or application resource dictionaries)
	'(used if a resource is not found in the page, 
	' app, or any theme specific resource dictionaries)
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>


' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:
' [assembly: AssemblyVersion("1.0.*")]
<Assembly: AssemblyVersion("1.0.0.0")>
<Assembly: AssemblyFileVersion("1.0.0.0")>
