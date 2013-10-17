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
    [ASN1Sequence(Name = "EventNotification", IsSet = false)]
    public class EventNotification : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(EventNotification));
        private ActionResultSequenceType actionResult_;

        private bool actionResult_present;
        private AlarmAckRule alarmAcknowledgmentRule_;

        private bool alarmAcknowledgmentRule_present;
        private EC_State currentState_;

        private bool currentState_present;
        private ObjectName eventConditionName_;
        private ObjectName eventEnrollmentName_;
        private bool notificationLost_;
        private Severity severity_;
        private EventTime transitionTime_;

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


        [ASN1Element(Name = "eventConditionName", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public ObjectName EventConditionName
        {
            get
            {
                return eventConditionName_;
            }
            set
            {
                eventConditionName_ = value;
            }
        }


        [ASN1Element(Name = "severity", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public Severity Severity
        {
            get
            {
                return severity_;
            }
            set
            {
                severity_ = value;
            }
        }


        [ASN1Element(Name = "currentState", IsOptional = true, HasTag = true, Tag = 3, HasDefaultValue = false)]
        public EC_State CurrentState
        {
            get
            {
                return currentState_;
            }
            set
            {
                currentState_ = value;
                currentState_present = true;
            }
        }


        [ASN1Element(Name = "transitionTime", IsOptional = false, HasTag = true, Tag = 4, HasDefaultValue = false)]
        public EventTime TransitionTime
        {
            get
            {
                return transitionTime_;
            }
            set
            {
                transitionTime_ = value;
            }
        }


        [ASN1Boolean(Name = "")]
        [ASN1Element(Name = "notificationLost", IsOptional = false, HasTag = true, Tag = 6, HasDefaultValue = true)]
        public bool NotificationLost
        {
            get
            {
                return notificationLost_;
            }
            set
            {
                notificationLost_ = value;
            }
        }


        [ASN1Element(Name = "alarmAcknowledgmentRule", IsOptional = true, HasTag = true, Tag = 7, HasDefaultValue = false)]
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


        [ASN1Element(Name = "actionResult", IsOptional = true, HasTag = true, Tag = 8, HasDefaultValue = false)]
        public ActionResultSequenceType ActionResult
        {
            get
            {
                return actionResult_;
            }
            set
            {
                actionResult_ = value;
                actionResult_present = true;
            }
        }

        public void initWithDefaults()
        {
            bool param_NotificationLost =
                false;
            NotificationLost = param_NotificationLost;
        }

        public IASN1PreparedElementData PreparedData
        {
            get
            {
                return preparedData;
            }
        }


        public bool isCurrentStatePresent()
        {
            return currentState_present;
        }

        public bool isAlarmAcknowledgmentRulePresent()
        {
            return alarmAcknowledgmentRule_present;
        }

        public bool isActionResultPresent()
        {
            return actionResult_present;
        }

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "actionResult", IsSet = false)]
        public class ActionResultSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ActionResultSequenceType));
            private ObjectName eventActionName_;


            private SuccessOrFailureChoiceType successOrFailure_;

            [ASN1Element(Name = "eventActionName", IsOptional = false, HasTag = false, HasDefaultValue = false)]
            public ObjectName EventActionName
            {
                get
                {
                    return eventActionName_;
                }
                set
                {
                    eventActionName_ = value;
                }
            }


            [ASN1Element(Name = "successOrFailure", IsOptional = false, HasTag = false, HasDefaultValue = false)]
            public SuccessOrFailureChoiceType SuccessOrFailure
            {
                get
                {
                    return successOrFailure_;
                }
                set
                {
                    successOrFailure_ = value;
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

            [ASN1PreparedElement]
            [ASN1Choice(Name = "successOrFailure")]
            public class SuccessOrFailureChoiceType : IASN1PreparedElement
            {
                private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(SuccessOrFailureChoiceType));
                private FailureSequenceType failure_;
                private bool failure_selected;
                private SuccessSequenceType success_;
                private bool success_selected;


                [ASN1Element(Name = "success", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
                public SuccessSequenceType Success
                {
                    get
                    {
                        return success_;
                    }
                    set
                    {
                        selectSuccess(value);
                    }
                }


                [ASN1Element(Name = "failure", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
                public FailureSequenceType Failure
                {
                    get
                    {
                        return failure_;
                    }
                    set
                    {
                        selectFailure(value);
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


                public bool isSuccessSelected()
                {
                    return success_selected;
                }


                public void selectSuccess(SuccessSequenceType val)
                {
                    success_ = val;
                    success_selected = true;


                    failure_selected = false;
                }


                public bool isFailureSelected()
                {
                    return failure_selected;
                }


                public void selectFailure(FailureSequenceType val)
                {
                    failure_ = val;
                    failure_selected = true;


                    success_selected = false;
                }

                [ASN1PreparedElement]
                [ASN1Sequence(Name = "failure", IsSet = false)]
                public class FailureSequenceType : IASN1PreparedElement
                {
                    private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(FailureSequenceType));
                    private Unsigned32 modifierPosition_;

                    private bool modifierPosition_present;


                    private ServiceError serviceError_;

                    [ASN1Element(Name = "modifierPosition", IsOptional = true, HasTag = true, Tag = 0, HasDefaultValue = false)]
                    public Unsigned32 ModifierPosition
                    {
                        get
                        {
                            return modifierPosition_;
                        }
                        set
                        {
                            modifierPosition_ = value;
                            modifierPosition_present = true;
                        }
                    }

                    [ASN1Element(Name = "serviceError", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
                    public ServiceError ServiceError
                    {
                        get
                        {
                            return serviceError_;
                        }
                        set
                        {
                            serviceError_ = value;
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

                    public bool isModifierPositionPresent()
                    {
                        return modifierPosition_present;
                    }
                }

                [ASN1PreparedElement]
                [ASN1Sequence(Name = "success", IsSet = false)]
                public class SuccessSequenceType : IASN1PreparedElement
                {
                    private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(SuccessSequenceType));
                    private ConfirmedServiceResponse confirmedServiceResponse_;


                    private Response_Detail cs_Response_Detail_;

                    private bool cs_Response_Detail_present;

                    [ASN1Element(Name = "confirmedServiceResponse", IsOptional = false, HasTag = false, HasDefaultValue = false)]
                    public ConfirmedServiceResponse ConfirmedServiceResponse
                    {
                        get
                        {
                            return confirmedServiceResponse_;
                        }
                        set
                        {
                            confirmedServiceResponse_ = value;
                        }
                    }

                    [ASN1Element(Name = "cs-Response-Detail", IsOptional = true, HasTag = true, Tag = 79, HasDefaultValue = false)]
                    public Response_Detail Cs_Response_Detail
                    {
                        get
                        {
                            return cs_Response_Detail_;
                        }
                        set
                        {
                            cs_Response_Detail_ = value;
                            cs_Response_Detail_present = true;
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

                    public bool isCs_Response_DetailPresent()
                    {
                        return cs_Response_Detail_present;
                    }
                }
            }
        }
    }
}