//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;

namespace GSF.MMS.Model
{
    [CompilerGenerated]
    [ASN1PreparedElement]
    [ASN1Choice(Name = "UnconfirmedService")]
    public class UnconfirmedService : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(UnconfirmedService));
        private EventNotification eventNotification_;
        private bool eventNotification_selected;
        private InformationReport informationReport_;
        private bool informationReport_selected;


        private UnsolicitedStatus unsolicitedStatus_;
        private bool unsolicitedStatus_selected;

        [ASN1Element(Name = "informationReport", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public InformationReport InformationReport
        {
            get
            {
                return informationReport_;
            }
            set
            {
                selectInformationReport(value);
            }
        }


        [ASN1Element(Name = "unsolicitedStatus", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public UnsolicitedStatus UnsolicitedStatus
        {
            get
            {
                return unsolicitedStatus_;
            }
            set
            {
                selectUnsolicitedStatus(value);
            }
        }


        [ASN1Element(Name = "eventNotification", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public EventNotification EventNotification
        {
            get
            {
                return eventNotification_;
            }
            set
            {
                selectEventNotification(value);
            }
        }

        public void initWithDefaults()
        {
        }

        public IASN1PreparedElementData PreparedData
        {
            get
            {
                return preparedData;
            }
        }


        public bool isInformationReportSelected()
        {
            return informationReport_selected;
        }


        public void selectInformationReport(InformationReport val)
        {
            informationReport_ = val;
            informationReport_selected = true;


            unsolicitedStatus_selected = false;

            eventNotification_selected = false;
        }


        public bool isUnsolicitedStatusSelected()
        {
            return unsolicitedStatus_selected;
        }


        public void selectUnsolicitedStatus(UnsolicitedStatus val)
        {
            unsolicitedStatus_ = val;
            unsolicitedStatus_selected = true;


            informationReport_selected = false;

            eventNotification_selected = false;
        }


        public bool isEventNotificationSelected()
        {
            return eventNotification_selected;
        }


        public void selectEventNotification(EventNotification val)
        {
            eventNotification_ = val;
            eventNotification_selected = true;


            informationReport_selected = false;

            unsolicitedStatus_selected = false;
        }
    }
}