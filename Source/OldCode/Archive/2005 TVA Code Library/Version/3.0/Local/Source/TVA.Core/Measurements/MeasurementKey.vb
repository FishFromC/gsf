'*******************************************************************************************************
'  TVA.Measurements.MeasurementKey.vb - Defines primary key elements for a measurement
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
'  12/8/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    ''' <summary>Defines a primary key for a measurement</summary>
    Public Structure MeasurementKey

        Implements IEquatable(Of MeasurementKey), IComparable(Of MeasurementKey), IComparable

        Private m_id As Integer
        Private m_source As String
        Private m_hashCode As Integer

        Public Sub New(ByVal id As Integer, ByVal source As String)

            If String.IsNullOrEmpty(source) Then Throw New ArgumentNullException("source", "MeasurementKey source cannot be null")

            m_id = id
            m_source = source.ToUpper()
            GenHashCode()

        End Sub

        Public Property ID() As Integer
            Get
                Return m_id
            End Get
            Set(ByVal value As Integer)
                If m_id <> value Then
                    m_id = value
                    GenHashCode()
                End If
            End Set
        End Property

        Public Property Source() As String
            Get
                Return m_source
            End Get
            Set(ByVal value As String)
                If String.Compare(m_source, value, True) <> 0 Then
                    m_source = value.ToUpper()
                    GenHashCode()
                End If
            End Set
        End Property

        Public Overrides Function ToString() As String

            Return String.Format("{0}:{1}", m_source, m_id)

        End Function

        Public Overrides Function GetHashCode() As Integer

            Return m_hashCode

        End Function

        Public Overloads Function Equals(ByVal other As MeasurementKey) As Boolean Implements System.IEquatable(Of MeasurementKey).Equals

            Return (m_hashCode = other.GetHashCode())

        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            ' Can't use TryCast on a structure...
            If TypeOf obj Is MeasurementKey Then Return Equals(DirectCast(obj, MeasurementKey))
            Throw New ArgumentException("Object is not a MeasurementKey")

        End Function

        Public Function CompareTo(ByVal other As MeasurementKey) As Integer Implements System.IComparable(Of MeasurementKey).CompareTo

            Dim sourceCompare As Integer = String.Compare(m_source, other.Source)

            If sourceCompare = 0 Then
                If m_id < other.ID Then
                    Return -1
                ElseIf m_id > other.ID Then
                    Return 1
                Else
                    Return 0
                End If
            Else
                Return sourceCompare
            End If

        End Function

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            ' Can't use TryCast on a structure...
            If TypeOf obj Is MeasurementKey Then Return CompareTo(DirectCast(obj, MeasurementKey))
            Throw New ArgumentException("Object is not a MeasurementKey")

        End Function

        Private Sub GenHashCode()

            ' We cache hash code during construction or after element value change to speed structure usage
            m_hashCode = (m_source & m_id.ToString()).GetHashCode()

        End Sub

#Region " MeasurementKey Operators "

        Public Shared Operator =(ByVal key1 As MeasurementKey, ByVal key2 As MeasurementKey) As Boolean

            Return key1.Equals(key2)

        End Operator

        Public Shared Operator <>(ByVal key1 As MeasurementKey, ByVal key2 As MeasurementKey) As Boolean

            Return Not key1.Equals(key2)

        End Operator

        Public Shared Operator >(ByVal key1 As MeasurementKey, ByVal key2 As MeasurementKey) As Boolean

            Return key1.CompareTo(key2) > 0

        End Operator

        Public Shared Operator >=(ByVal key1 As MeasurementKey, ByVal key2 As MeasurementKey) As Boolean

            Return key1.CompareTo(key2) >= 0

        End Operator

        Public Shared Operator <(ByVal key1 As MeasurementKey, ByVal key2 As MeasurementKey) As Boolean

            Return key1.CompareTo(key2) < 0

        End Operator

        Public Shared Operator <=(ByVal key1 As MeasurementKey, ByVal key2 As MeasurementKey) As Boolean

            Return key1.CompareTo(key2) <= 0

        End Operator

#End Region

    End Structure

End Namespace