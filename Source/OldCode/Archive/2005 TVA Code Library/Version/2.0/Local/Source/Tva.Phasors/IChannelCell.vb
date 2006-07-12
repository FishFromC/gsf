'*******************************************************************************************************
'  IChannelCell.vb - Channel data cell interface
'  Copyright � 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

' This interface represents a protocol independent representation of any kind of data cell.
<CLSCompliant(False)> _
Public Interface IChannelCell

    Inherits IChannel, ISerializable

    ' TODO: you must question validity of read-only state for non-serialized entities (either serialize value or allow value to be set after deserialization...)
    ReadOnly Property Parent() As IChannelFrame

    Property IDCode() As UInt16

    ReadOnly Property AlignOnDWordBoundry() As Boolean

End Interface

