Imports Microsoft.Expression.Encoder.Devices
Imports System.Collections.ObjectModel

Public Class MainWindow

    Public Property VideoDevices As Collection(Of EncoderDevice)


    Public Sub New()
        InitializeComponent()

        DataContext = Me

        VideoDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video)

    End Sub

    Private Sub StartCaptureButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Display webcam video
        Try
            WebcamViewer.StartPreview()
        Catch ex As Microsoft.Expression.Encoder.SystemErrorException
            MessageBox.Show("Device is in use by another application")
        End Try
    End Sub

    Private Sub StopCaptureButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Stop the display of webcam video
        WebcamViewer.StopPreview()
    End Sub


    Private Sub TakeSnapshotButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Take snapshot of webcam video
        WebcamViewer.TakeSnapshot()
    End Sub
End Class
