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
    [ASN1Sequence(Name = "AlterEventEnrollment_Request", IsSet = false)]
    public class AlterEventEnrollment_Request : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(AlterEventEnrollment_Request));
        private AlarmAckRule alarmAcknowledgmentRule_;

        private bool alarmAcknowledgmentRule_present;
        private Transitions eventConditionTransitions_;

        private bool eventConditionTransitions_present;
        private ObjectName eventEnrollmentName_;

        [ASN1Element(Name = "eventEnrollmentName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public ObjectName EventEnrollmentName
        {
            get
            {
                return eventEnrollmentName_;
            }
            set
            {
                eventEnrollmentName_ = value;
            }
        }

        [ASN1Element(Name = "eventConditionTransitions", IsOptional = true, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public Transitions EventConditionTransitions
        {
            get
            {
                return eventConditionTransitions_;
            }
            set
            {
                eventConditionTransitions_ = value;
                eventConditionTransitions_present = true;
            }
        }


        [ASN1Element(Name = "alarmAcknowledgmentRule", IsOptional = true, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public AlarmAckRule AlarmAcknowledgmentRule
        {
            get
            {
                return alarmAcknowledgmentRule_;
            }
            set
            {
                alarmAcknowledgmentRule_ = value;
                alarmAcknowledgmentRule_present = true;
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

        public bool isEventConditionTransitionsPresent()
        {
            return eventConditionTransitions_present;
        }

        public bool isAlarmAcknowledgmentRulePresent()
        {
            return alarmAcknowledgmentRule_present;
        }
    }
}