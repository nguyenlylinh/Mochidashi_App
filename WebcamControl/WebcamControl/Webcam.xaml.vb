Imports Microsoft.Expression.Encoder
Imports Microsoft.Expression.Encoder.Devices
Imports Microsoft.Expression.Encoder.Live
Imports Microsoft.Expression.Encoder.Profiles
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging

Public Class Webcam
    Implements IDisposable

#Region "VideoFileFormat dependency property"
    ''' <summary>
    ''' Gets the video format in which the webcam video will be saved.
    ''' </summary>
    Public ReadOnly Property VideoFileFormat As String
        Get
            Return GetValue(Webcam.VideoFileFormatProperty)
        End Get
    End Property

    Private Shared ReadOnly VideoFileFormatPropertyKey As DependencyPropertyKey =
        DependencyProperty.RegisterReadOnly("VideoFileFormat", GetType(String), GetType(Webcam), New PropertyMetadata(".wmv"))

    Public Shared ReadOnly VideoFileFormatProperty As DependencyProperty = VideoFileFormatPropertyKey.DependencyProperty
#End Region

#Region "SnapshotFormat dependency property"
    ''' <summary>
    ''' Gets or Sets the format used when saving video snapshot.
    ''' </summary>    
    Public Property SnapshotFormat() As ImageFormat
        Get
            Return CType(GetValue(SnapshotFormatProperty), ImageFormat)
        End Get
        Set(ByVal value As ImageFormat)
            SetValue(SnapshotFormatProperty, value)
        End Set
    End Property

    Public Shared ReadOnly SnapshotFormatProperty As DependencyProperty =
        DependencyProperty.Register("SnapshotFormat", GetType(ImageFormat), GetType(Webcam), New PropertyMetadata(ImageFormat.Jpeg))
#End Region

#Region "VideoDevice dependency property"
    ''' <summary>
    ''' Gets or Sets the name of the video device.
    ''' </summary>    
    Public Property VideoDevice() As EncoderDevice
        Get
            Return CType(GetValue(VideoDeviceProperty), EncoderDevice)
        End Get
        Set(ByVal value As EncoderDevice)
            SetValue(VideoDeviceProperty, value)
        End Set
    End Property

    Public Shared ReadOnly VideoDeviceProperty As DependencyProperty =
        DependencyProperty.Register("VideoDevice", GetType(EncoderDevice), GetType(Webcam),
                                    New PropertyMetadata(New PropertyChangedCallback(AddressOf VideoDeviceChange)))

    Private _videoDevice As EncoderDevice
    Private Shared Sub VideoDeviceChange(ByVal source As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim device As EncoderDevice = CType(e.NewValue, EncoderDevice)
        Dim devices As IEnumerable(Of EncoderDevice) =
            EncoderDevices.FindDevices(EncoderDeviceType.Video).Where(Function(dv) dv.Name = device.Name)

        If devices.Count <> 0 Then
            Dim src As Webcam = CType(source, Webcam)
            src._videoDevice = devices.First
            If src.isPreviewing Then
                src.StartPreview()
            End If
        End If
    End Sub
#End Region

#Region "AudioDevice dependency property"
    ''' <summary>
    ''' Gets or Sets the name of the audio device.
    ''' </summary>    
    Public Property AudioDevice() As EncoderDevice
        Get
            Return CType(GetValue(AudioDeviceProperty), EncoderDevice)
        End Get
        Set(ByVal value As EncoderDevice)
            SetValue(AudioDeviceProperty, value)
        End Set
    End Property

    Public Shared ReadOnly AudioDeviceProperty As DependencyProperty =
        DependencyProperty.Register("AudioDevice", GetType(EncoderDevice), GetType(Webcam),
                                    New PropertyMetadata(New PropertyChangedCallback(AddressOf AudioDeviceChange)))

    Private _audioDevice As EncoderDevice
    Private Shared Sub AudioDeviceChange(ByVal source As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim device As EncoderDevice = CType(e.NewValue, EncoderDevice)
        Dim devices As IEnumerable(Of EncoderDevice) =
            EncoderDevices.FindDevices(EncoderDeviceType.Audio).Where(Function(dv) dv.Name = device.Name)

        If devices.Count <> 0 Then
            Dim src As Webcam = CType(source, Webcam)
            src._audioDevice = devices.First
            If src.isPreviewing Then
                src.StartPreview()
            End If
        End If
    End Sub
#End Region

#Region "VideoName dependency property"
    ''' <summary>
    ''' Gets or Sets the name of the video file. (The name should not include the file extension).
    ''' </summary>    
    Public Property VideoName() As String
        Get
            Return CType(GetValue(VideoNameProperty), String)
        End Get
        Set(ByVal value As String)
            SetValue(VideoNameProperty, value)
        End Set
    End Property

    Public Shared ReadOnly VideoNameProperty As DependencyProperty =
        DependencyProperty.Register("VideoName", GetType(String), GetType(Webcam), New PropertyMetadata(Nothing))
#End Region

#Region "VideoDirectory dependency property"
    ''' <summary>
    ''' Gets or Sets the folder where the recorded video will be saved.
    ''' </summary>    
    Public Property VideoDirectory() As String
        Get
            Return CType(GetValue(VideoDirectoryProperty), String)
        End Get
        Set(ByVal value As String)
            SetValue(VideoDirectoryProperty, value)
        End Set
    End Property

    Public Shared ReadOnly VideoDirectoryProperty As DependencyProperty =
        DependencyProperty.Register("VideoDirectory", GetType(String), GetType(Webcam), New PropertyMetadata(Nothing))
#End Region

#Region "ImageDirectory dependency property"
    ''' <summary>
    ''' Gets or Sets the folder where video snapshot will be saved.
    ''' </summary>    
    Public Property ImageDirectory() As String
        Get
            Return CType(GetValue(ImageDirectoryProperty), String)
        End Get
        Set(ByVal value As String)
            SetValue(ImageDirectoryProperty, value)
        End Set
    End Property

    Public Shared ImageDirectoryProperty As DependencyProperty =
        DependencyProperty.Register("ImageDirectory", GetType(String), GetType(Webcam), New PropertyMetadata(Nothing))
#End Region

#Region "FrameRate dependency property"
    ''' <summary>
    ''' Gets or sets the frame rate, in frames per second. Default value is 15.
    ''' </summary>    
    Public Property FrameRate As Integer
        Get
            Return CType(GetValue(FrameRateProperty), Integer)
        End Get
        Set(value As Integer)
            SetValue(FrameRateProperty, value)
        End Set
    End Property

    Public Shared ReadOnly FrameRateProperty As DependencyProperty =
        DependencyProperty.Register("FrameRate", GetType(Integer), GetType(Webcam),
                                    New PropertyMetadata(15, New PropertyChangedCallback(AddressOf FrameRateChange)))

    Private Shared Sub FrameRateChange(ByVal source As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim src As Webcam = CType(source, Webcam)
        If src.isPreviewing Then
            src.StartPreview()
        End If
    End Sub
#End Region

#Region "Bitrate dependency property"
    ''' <summary>
    ''' Gets or sets the bitrate. Default value is 2000.
    ''' </summary>    
    Public Property Bitrate As Integer
        Get
            Return CType(GetValue(BitrateProperty), Integer)
        End Get
        Set(value As Integer)
            SetValue(BitrateProperty, value)
        End Set
    End Property

    Public Shared ReadOnly BitrateProperty As DependencyProperty =
        DependencyProperty.Register("Bitrate", GetType(Integer), GetType(Webcam),
                                    New PropertyMetadata(2000, New PropertyChangedCallback(AddressOf BitrateChange)))

    Private Shared Sub BitrateChange(ByVal source As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim src As Webcam = CType(source, Webcam)
        If src.isPreviewing Then
            src.StartPreview()
        End If
    End Sub
#End Region

#Region "FrameSize dependency property"
    ''' <summary>
    ''' Gets or sets the size of the video profile. Default is 320x240.
    ''' </summary>    
    Public Property FrameSize As Size
        Get
            Return CType(GetValue(FrameSizeProperty), Size)
        End Get
        Set(value As Size)
            SetValue(FrameSizeProperty, value)
        End Set
    End Property

    Public Shared ReadOnly FrameSizeProperty As DependencyProperty =
        DependencyProperty.Register("FrameSize", GetType(Size), GetType(Webcam),
                                    New PropertyMetadata(New Size(320, 240), New PropertyChangedCallback(AddressOf FrameSizeChange)))

    Private Shared Sub FrameSizeChange(ByVal source As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim src As Webcam = CType(source, Webcam)
        If src.isPreviewing Then
            src.StartPreview()
        End If
    End Sub
#End Region

#Region "IsRecording dependency property"
    ''' <summary>
    ''' Gets a value indicating whether video recording is taking place.
    ''' </summary>
    Public ReadOnly Property IsRecording As Boolean
        Get
            Return GetValue(Webcam.IsRecordingProperty)
        End Get
    End Property

    Private Shared ReadOnly IsRecordingPropertyKey As DependencyPropertyKey =
        DependencyProperty.RegisterReadOnly("IsRecording", GetType(Boolean), GetType(Webcam), New PropertyMetadata(False))

    Public Shared ReadOnly IsRecordingProperty As DependencyProperty = IsRecordingPropertyKey.DependencyProperty
#End Region

    Private deviceSource As LiveDeviceSource
    Private job As LiveJob
    Private isPreviewing As Boolean

    ''' <summary>
    ''' Displays webcam video.
    ''' </summary>
    Public Sub StartPreview()
        Try
            If isPreviewing Then StopPreview()

            job = New LiveJob
            Dim frameDuration As Long = CLng(FrameRate * Math.Pow(10, 7))

            deviceSource = job.AddDeviceSource(_videoDevice, _audioDevice)
            deviceSource.PickBestVideoFormat(FrameSize, frameDuration)
            deviceSource.PreviewWindow = New PreviewWindow(New HandleRef(WebcamPanel, WebcamPanel.Handle))

            job.OutputFormat.VideoProfile = New AdvancedVC1VideoProfile With {.Size = FrameSize,
                .FrameRate = FrameRate, .Bitrate = New ConstantBitrate(Bitrate)}

            job.ActivateSource(deviceSource)

            isPreviewing = True
        Catch ex As SystemErrorException
            Throw New SystemErrorException
        End Try
    End Sub

    ''' <summary>
    ''' Stops the display of webcam video and also stops any ongoing recording.
    ''' </summary>
    Public Sub StopPreview()
        If isPreviewing Then
            If IsRecording Then StopRecording()
            deviceSource = Nothing
            job.Dispose()
            isPreviewing = False
        End If
    End Sub

    Private Function TimeStamp() As String
        Dim ts As String = DateTime.Now.ToString
        ts = ts.Replace("/", "-")
        ts = ts.Replace(":", ".")
        Return ts
    End Function

    ''' <summary>
    ''' Starts the recording of webcam images to a video file.
    ''' </summary>
    ''' <returns>Location where recoding is being saved.</returns>
    Public Function StartRecording() As String
        If Not isPreviewing Then Throw New PreviewNotStartedException("Recording can't be done without previewing.")
        If String.IsNullOrEmpty(VideoDirectory) Then Throw New DirectoryNotSpecifiedException("Video directory has not been specified.")
        If Not Directory.Exists(VideoDirectory) Then Directory.CreateDirectory(VideoDirectory)
        If IsRecording Then StopRecording()

        Dim filePath As String
        If String.IsNullOrEmpty(VideoName) Then
            filePath = Path.Combine(VideoDirectory, "Webcam " & TimeStamp() & VideoFileFormat)
        Else
            filePath = Path.Combine(VideoDirectory, VideoName & VideoFileFormat)
        End If

        Dim archiveFormat As New FileArchivePublishFormat(filePath)

        If job.PublishFormats.Count > 0 Then job.PublishFormats.Clear()

        job.PublishFormats.Add(archiveFormat)
        job.StartEncoding()

        SetValue(IsRecordingPropertyKey, True)
        Return filePath
    End Function

    ''' <summary>
    ''' Stops the recording of webcam video.
    ''' </summary>
    Public Sub StopRecording()
        If IsRecording Then
            job.StopEncoding()
            SetValue(IsRecordingPropertyKey, False)
        End If
    End Sub

    ''' <summary>
    ''' Takes a snapshot of an webcam image.
    ''' The size of the image will be equal to the size of the control.
    ''' </summary>
    ''' <returns>Location where snapshot is saved.</returns>
    Public Function TakeSnapshot() As String
        If Not isPreviewing Then Throw New PreviewNotStartedException("Recording can't be done before previewing.")
        If String.IsNullOrEmpty(ImageDirectory) Then Throw New DirectoryNotSpecifiedException("Image directory has not been specified")
        If Not Directory.Exists(ImageDirectory) Then Directory.CreateDirectory(ImageDirectory)

        Dim panelWidth As Integer = WebcamPanel.Width
        Dim panelHeight As Integer = WebcamPanel.Height
        Dim filename As String = TimeStamp().ToString()
        Dim filename1 As String = filename.Replace(" ", "")
        Dim filePath As String = Path.Combine(ImageDirectory, "Snapshot" & filename1 & "." & SnapshotFormat.ToString())
        Dim pnt As Point = WebcamPanel.PointToScreen(New Point(WebcamPanel.ClientRectangle.X, WebcamPanel.ClientRectangle.Y))

        Using bmp As New Bitmap(panelWidth, panelHeight)
            Using grx As Graphics = Graphics.FromImage(bmp)
                grx.CopyFromScreen(pnt, Point.Empty, New Size(panelWidth, panelHeight))
            End Using
            bmp.Save(filePath, SnapshotFormat)
        End Using
        Return filePath
    End Function

    ''' <summary>
    ''' Takes a snapshot of an webcam image.
    ''' The size of the image will be equal to the size of the control.
    ''' </summary>
    ''' <param name="name">Name of the file.</param>
    ''' <returns></returns>
    Public Function TakeSnapshot(ByVal name As String) As String
        If String.IsNullOrEmpty(name) Then Throw New ArgumentNullException()
        If Not isPreviewing Then Throw New PreviewNotStartedException("Recording can't be done before previewing.")
        If String.IsNullOrEmpty(ImageDirectory) Then Throw New DirectoryNotSpecifiedException("Image directory has not been specified")
        If Not Directory.Exists(ImageDirectory) Then Directory.CreateDirectory(ImageDirectory)

        Dim panelWidth As Integer = WebcamPanel.Width
        Dim panelHeight As Integer = WebcamPanel.Height
        Dim filePath As String = Path.Combine(ImageDirectory, name & "." & SnapshotFormat.ToString())
        Dim pnt As Point = WebcamPanel.PointToScreen(New Point(WebcamPanel.ClientRectangle.X, WebcamPanel.ClientRectangle.Y))

        Using bmp As New Bitmap(panelWidth, panelHeight)
            Using grx As Graphics = Graphics.FromImage(bmp)
                grx.CopyFromScreen(pnt, Point.Empty, New Size(panelWidth, panelHeight))
            End Using
            bmp.Save(filePath, SnapshotFormat)
        End Using

        Return filePath
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If deviceSource IsNot Nothing Then
                    deviceSource.PreviewWindow.Dispose()
                    deviceSource.PreviewWindow = Nothing
                    deviceSource.Dispose()
                    deviceSource = Nothing
                End If

                If job IsNot Nothing Then
                    job.Dispose()
                    job = Nothing
                End If

                WinFormsHost.Dispose()
                WinFormsHost = Nothing

                WebcamPanel.Dispose()
                WebcamPanel = Nothing
            End If
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Private Sub Webcam_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        If deviceSource IsNot Nothing Then
            deviceSource.PreviewWindow.SetSize(New Size(CInt(Me.ActualWidth), CInt(Me.ActualHeight)))
        End If
    End Sub

    Private Sub Webcam_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        Dispose()
    End Sub
End Class