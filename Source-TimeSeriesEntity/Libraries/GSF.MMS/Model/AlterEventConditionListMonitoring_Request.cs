//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;
using GSF.ASN1.Types;

namespace GSF.MMS.Model
{
    [CompilerGenerated]
    [ASN1PreparedElement]
    [ASN1Sequence(Name = "AlterEventConditionListMonitoring_Request", IsSet = false)]
    public class AlterEventConditionListMonitoring_Request : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(AlterEventConditionListMonitoring_Request));
        private bool enabled_;
        private ObjectName eventConditionListName_;


        private PriorityChangeChoiceType priorityChange_;

        private bool priorityChange_present;

        [ASN1Element(Name = "eventConditionListName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public ObjectName EventConditionListName
        {
            get
            {
                return eventConditionListName_;
            }
            set
            {
                eventConditionListName_ = value;
            }
        }

        [ASN1Boolean(Name = "")]
        [ASN1Element(Name = "enabled", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public bool Enabled
        {
            get
            {
                return enabled_;
            }
            set
            {
                enabled_ = value;
            }
        }


        [ASN1Element(Name = "priorityChange", IsOptional = true, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public PriorityChangeChoiceType PriorityChange
        {
            get
            {
                return priorityChange_;
            }
            set
            {
                priorityChange_ = value;
                priorityChange_present = true;
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

        public bool isPriorityChangePresent()
        {
            return priorityChange_present;
        }

        [ASN1PreparedElement]
        [ASN1Choice(Name = "priorityChange")]
        public class PriorityChangeChoiceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(PriorityChangeChoiceType));
            private NullObject priorityReset_;
            private bool priorityReset_selected;
            private long priorityValue_;
            private bool priorityValue_selected;


            [ASN1Integer(Name = "")]
            [ASN1Element(Name = "priorityValue", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
            public long PriorityValue
            {
                get
                {
                    return priorityValue_;
                }
                set
                {
                    selectPriorityValue(value);
                }
            }


            [ASN1Null(Name = "priorityReset")]
            [ASN1Element(Name = "priorityReset", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public NullObject PriorityReset
            {
                get
                {
                    return priorityReset_;
                }
                set
                {
                    selectPriorityReset(value);
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


            public bool isPriorityValueSelected()
            {
                return priorityValue_selected;
            }


            public void selectPriorityValue(long val)
            {
                priorityValue_ = val;
                priorityValue_selected = true;


                priorityReset_selected = false;
            }


            public bool isPriorityResetSelected()
            {
                return priorityReset_selected;
            }


            public void selectPriorityReset()
            {
                selectPriorityReset(new NullObject());
            }


            public void selectPriorityReset(NullObject val)
            {
                priorityReset_ = val;
                priorityReset_selected = true;


                priorityValue_selected = false;
            }
        }
    }
}