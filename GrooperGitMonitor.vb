
'For more information, see Custom Timer Services in Grooper x Change:  https://xchange.grooper.com/discussion/779/custom-timer-service#latest

''' <summary>Template for a custom [Timer Service].</summary>
''' <remarks></remarks>
<DisplayName("Git Monitoring Service")>
Public Class GitMonitor : Inherits TimerService

    Public Sub New(gdb As GrooperDb)
        MyBase.New(gdb)
    End Sub

    Protected Overrides Sub Tick()

    End Sub

End Class