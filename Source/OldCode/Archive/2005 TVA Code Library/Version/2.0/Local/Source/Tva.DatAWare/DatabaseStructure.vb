'*******************************************************************************************************
'  DatabaseStructure.vb - Standard DatAWare database structure element
'  Copyright � 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/03/2006 - J. Ritchie Carroll
'       Initial version of source imported from 1.1 code library
'
'*******************************************************************************************************

Imports System.Text

' Standard database structure element
Public Class DatabaseStructure

    Implements IComparable

    Public SourceID As Short                    ' 2
    Public Unit As Short                        ' 2
    Public SecurityLevel As Short               ' 2
    Public CompressionMinimumTime As Integer    ' 4
    Public CompressionMaximumTime As Integer    ' 4
    Public Spares As Byte()                     ' 64
    Public FlagWord As Integer                  ' 4
    Public TransitionFlag As Integer            ' 4
    Public ScanRate As Single                   ' 4
    Public BinaryInfo As Byte()                 ' 256
    Public TextEncoding As Encoding

    Private m_index As Integer
    Private m_description As String = ""        ' 40
    Private m_hardWareInfo As String = ""       ' 64
    Private m_pointID As String = ""            ' 20
    Private m_synonym1 As String = ""           ' 20
    Private m_synonym2 As String = ""           ' 20
    Private m_siteName As String = ""           ' 2
    Private m_system As String = ""             ' 4
    Private m_email As String = ""              ' 50
    Private m_pager As String = ""              ' 30
    Private m_phone As String = ""              ' 30
    Private m_remarks As String = ""            ' 128

    Public Const BinaryLength As Integer = 754

    Public Sub New(ByVal index As Integer)

        m_index = index
        TextEncoding = Encoding.Default ' By default we decode strings using encoding for the system's current ANSI code page
        Spares = Array.CreateInstance(GetType(Byte), 64)
        BinaryInfo = Array.CreateInstance(GetType(Byte), 256)

    End Sub

    Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer, Optional ByVal encoding As Encoding = Nothing)

        Me.New(index)

        If Not encoding Is Nothing Then TextEncoding = encoding

        If binaryImage Is Nothing Then
            Throw New ArgumentNullException("BinaryImage was null - could not create DatAWare.DatabaseStructure")
        ElseIf binaryImage.Length - startIndex < BinaryLength Then
            Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create DatAWare.DatabaseStructure")
        Else
            m_description = Trim(TextEncoding.GetString(binaryImage, startIndex, 40))
            Unit = BitConverter.ToInt16(binaryImage, startIndex + 40)
            SecurityLevel = BitConverter.ToInt16(binaryImage, startIndex + 42)
            m_hardWareInfo = Trim(TextEncoding.GetString(binaryImage, startIndex + 44, 64))
            Array.Copy(binaryImage, startIndex + 108, Spares, 0, 64)
            FlagWord = BitConverter.ToInt32(binaryImage, startIndex + 172)
            TransitionFlag = BitConverter.ToInt32(binaryImage, startIndex + 176)
            ScanRate = BitConverter.ToSingle(binaryImage, startIndex + 180)
            m_pointID = Trim(TextEncoding.GetString(binaryImage, startIndex + 184, 20))
            m_synonym1 = Trim(TextEncoding.GetString(binaryImage, startIndex + 204, 20))
            m_synonym2 = Trim(TextEncoding.GetString(binaryImage, startIndex + 224, 20))
            m_siteName = Trim(TextEncoding.GetString(binaryImage, startIndex + 244, 2))
            SourceID = BitConverter.ToInt16(binaryImage, startIndex + 246)
            CompressionMinimumTime = BitConverter.ToInt32(binaryImage, startIndex + 248)
            CompressionMaximumTime = BitConverter.ToInt32(binaryImage, startIndex + 252)
            m_system = Trim(TextEncoding.GetString(binaryImage, startIndex + 256, 4))
            m_email = Trim(TextEncoding.GetString(binaryImage, startIndex + 260, 50))
            m_pager = Trim(TextEncoding.GetString(binaryImage, startIndex + 310, 30))
            m_phone = Trim(TextEncoding.GetString(binaryImage, startIndex + 340, 30))
            m_remarks = Trim(TextEncoding.GetString(binaryImage, startIndex + 370, 128))
            Array.Copy(binaryImage, startIndex + 498, BinaryInfo, 0, 256)
        End If

    End Sub

    Public Shared Function Clone(ByVal pointDefinition As DatabaseStructure, ByVal newIndex As Integer) As DatabaseStructure

        Dim newPointDefinition As New DatabaseStructure(newIndex)

        With newPointDefinition
            .Description = pointDefinition.Description
            .Unit = pointDefinition.Unit
            .SecurityLevel = pointDefinition.SecurityLevel
            .HardwareInfo = pointDefinition.HardwareInfo
            Array.Copy(pointDefinition.Spares, .Spares, 64)
            .FlagWord = pointDefinition.FlagWord
            .TransitionFlag = pointDefinition.TransitionFlag
            .ScanRate = pointDefinition.ScanRate
            .PointID = pointDefinition.PointID
            .Synonym1 = pointDefinition.Synonym1
            .Synonym2 = pointDefinition.Synonym2
            .SiteName = pointDefinition.SiteName
            .SourceID = pointDefinition.SourceID
            .CompressionMinimumTime = pointDefinition.CompressionMinimumTime
            .CompressionMaximumTime = pointDefinition.CompressionMaximumTime
            .System = pointDefinition.System
            .Email = pointDefinition.Email
            .Pager = pointDefinition.Pager
            .Phone = pointDefinition.Phone
            .Remarks = pointDefinition.Remarks
            Array.Copy(pointDefinition.BinaryInfo, .BinaryInfo, 256)
            .TextEncoding = pointDefinition.TextEncoding
        End With

    End Function

    Public ReadOnly Property Index() As Integer
        Get
            Return m_index
        End Get
    End Property

    Public Property Description() As String
        Get
            Return m_description
        End Get
        Set(ByVal Value As String)
            m_description = Value.Substring(0, 40)
        End Set
    End Property

    Public Property HardwareInfo() As String
        Get
            Return m_hardWareInfo
        End Get
        Set(ByVal Value As String)
            m_hardWareInfo = Value.Substring(0, 64)
        End Set
    End Property

    Public Property PointID() As String
        Get
            Return m_pointID
        End Get
        Set(ByVal Value As String)
            m_pointID = Value.Substring(0, 20)
        End Set
    End Property

    Public Property Synonym1() As String
        Get
            Return m_synonym1
        End Get
        Set(ByVal Value As String)
            m_synonym1 = Value.Substring(0, 20)
        End Set
    End Property

    Public Property Synonym2() As String
        Get
            Return m_synonym2
        End Get
        Set(ByVal Value As String)
            m_synonym2 = Value.Substring(0, 20)
        End Set
    End Property

    Public Property SiteName() As String
        Get
            Return m_siteName
        End Get
        Set(ByVal Value As String)
            m_siteName = Value.Substring(0, 2)
        End Set
    End Property

    Public Property System() As String
        Get
            Return m_system
        End Get
        Set(ByVal Value As String)
            m_system = Value.Substring(0, 4)
        End Set
    End Property

    Public Property Email() As String
        Get
            Return m_email
        End Get
        Set(ByVal Value As String)
            m_email = Value.Substring(0, 50)
        End Set
    End Property

    Public Property Pager() As String
        Get
            Return m_pager
        End Get
        Set(ByVal Value As String)
            m_pager = Value.Substring(0, 30)
        End Set
    End Property

    Public Property Phone() As String
        Get
            Return m_phone
        End Get
        Set(ByVal Value As String)
            m_phone = Value.Substring(0, 30)
        End Set
    End Property

    Public Property Remarks() As String
        Get
            Return m_remarks
        End Get
        Set(ByVal Value As String)
            m_remarks = Value.Substring(0, 128)
        End Set
    End Property

    Public ReadOnly Property BinaryImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

            ' Construct the binary IP buffer for this event
            Array.Copy(TextEncoding.GetBytes(m_description.PadRight(40)), 0, buffer, 0, 40)
            Array.Copy(BitConverter.GetBytes(Unit), 0, buffer, 40, 2)
            Array.Copy(BitConverter.GetBytes(SecurityLevel), 0, buffer, 42, 2)
            Array.Copy(TextEncoding.GetBytes(m_hardWareInfo.PadRight(64)), 0, buffer, 44, 64)
            Array.Copy(Spares, 0, buffer, 108, 64)
            Array.Copy(BitConverter.GetBytes(FlagWord), 0, buffer, 172, 4)
            Array.Copy(BitConverter.GetBytes(TransitionFlag), 0, buffer, 176, 4)
            Array.Copy(BitConverter.GetBytes(ScanRate), 0, buffer, 180, 4)
            Array.Copy(TextEncoding.GetBytes(m_pointID.PadRight(20)), 0, buffer, 184, 20)
            Array.Copy(TextEncoding.GetBytes(m_synonym1.PadRight(20)), 0, buffer, 204, 20)
            Array.Copy(TextEncoding.GetBytes(m_synonym2.PadRight(20)), 0, buffer, 224, 20)
            Array.Copy(TextEncoding.GetBytes(m_siteName.PadRight(2)), 0, buffer, 244, 2)
            Array.Copy(BitConverter.GetBytes(SourceID), 0, buffer, 246, 2)
            Array.Copy(BitConverter.GetBytes(CompressionMinimumTime), 0, buffer, 248, 4)
            Array.Copy(BitConverter.GetBytes(CompressionMaximumTime), 0, buffer, 252, 4)
            Array.Copy(TextEncoding.GetBytes(m_system.PadRight(4)), 0, buffer, 256, 4)
            Array.Copy(TextEncoding.GetBytes(m_email.PadRight(50)), 0, buffer, 260, 50)
            Array.Copy(TextEncoding.GetBytes(m_pager.PadRight(30)), 0, buffer, 310, 30)
            Array.Copy(TextEncoding.GetBytes(m_phone.PadRight(30)), 0, buffer, 340, 30)
            Array.Copy(TextEncoding.GetBytes(m_remarks.PadRight(128)), 0, buffer, 370, 128)
            Array.Copy(BinaryInfo, 0, buffer, 498, 256)

            Return buffer
        End Get
    End Property

    ' We sort database structures in index order
    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is DatabaseStructure Then
            Return m_index.CompareTo(DirectCast(obj, DatabaseStructure).Index)
        Else
            Throw New ArgumentException("DatabaseStructure can only be compared with other DatabaseStructures")
        End If

    End Function

End Class
