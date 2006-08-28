' 04-24-06

Imports System.ComponentModel
Imports Tva.Collections

Namespace Ssam

    Partial Class SsamLogger
        Inherits System.ComponentModel.Component

        <System.Diagnostics.DebuggerNonUserCode(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Sub New(ByVal Container As System.ComponentModel.IContainer)
            MyClass.New()

            'Required for Windows.Forms Class Composition Designer support
            Container.Add(Me)
        End Sub

        <System.Diagnostics.DebuggerNonUserCode(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Sub New()
            ' During the this default initialization, we will not initialize the SSAM API implicitly 
            ' since it is done explicitly after the component has initialized.
            MyClass.New(New SsamApi(SsamServer.Development, True, True), False)

            'This call is required by the Component Designer.
            InitializeComponent()
        End Sub

        'Component overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                m_ssamApi.Dispose()
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Component Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Component Designer
        'It can be modified using the Component Designer.
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            components = New System.ComponentModel.Container()
        End Sub

    End Class

End Namespace
